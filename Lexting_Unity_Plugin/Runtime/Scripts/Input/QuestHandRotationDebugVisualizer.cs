#if !UNITY_STANDALONE && UNITY_ANDROID
using SpatialGestureText.LetterCube;
using SpatialGestureText.Utilities;
using UnityEngine;

namespace SpatialGestureText.Input
{
    /// <summary>
    /// In-headset lines: palm normal (cyan) and active comparison axis (yellow) + TextMesh status label.
    /// </summary>
    public sealed class QuestHandRotationDebugVisualizer : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Attach()
        {
            if (!QuestHandRotationDebugSettings.Enabled)
                return;

            var go = new GameObject("[Lexting] RotDebug");
            go.hideFlags = HideFlags.DontSave;
            DontDestroyOnLoad(go);
            go.AddComponent<QuestHandRotationDebugVisualizer>();
        }

        HandViz _right;
        HandViz _left;

        void Awake()
        {
            _right = HandViz.Create(transform, true, new Color(0.2f, 0.95f, 1f), new Color(1f, 0.85f, 0.2f));
            _left = HandViz.Create(transform, false, new Color(0.2f, 0.95f, 1f), new Color(1f, 0.85f, 0.2f));
            QuestHandLog.Info(
                $"[RotDebug] ON tag={QuestHandRotationDebugSettings.BuildTag} palm={QuestHandRotationDebugSettings.PalmNormalSource}");
        }

        void Update()
        {
            if (!QuestHandRotationDebugSettings.Enabled)
            {
                if (gameObject.activeSelf)
                    gameObject.SetActive(false);
                return;
            }

            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            _right?.Refresh();
            _left?.Refresh();
        }

        sealed class HandViz
        {
            readonly bool _right;
            readonly LineRenderer _palmLine;
            readonly LineRenderer _compareLine;
            readonly TextMesh _label;
            readonly Transform _labelTransform;

            HandViz(bool right, LineRenderer palmLine, LineRenderer compareLine, TextMesh label)
            {
                _right = right;
                _palmLine = palmLine;
                _compareLine = compareLine;
                _label = label;
                _labelTransform = label.transform;
            }

            public static HandViz Create(Transform parent, bool right, Color palmColor, Color compareColor)
            {
                var root = new GameObject(right ? "Right" : "Left").transform;
                root.SetParent(parent, false);

                var palmLine = CreateLine(root, "PalmNormal", palmColor);
                var compareLine = CreateLine(root, "CompareAxis", compareColor);

                var labelGo = new GameObject("Label");
                labelGo.transform.SetParent(root, false);
                var label = labelGo.AddComponent<TextMesh>();
                label.fontSize = 64;
                label.characterSize = QuestHandRotationDebugSettings.LabelCharacterSize;
                label.anchor = TextAnchor.MiddleLeft;
                label.color = Color.white;
                label.text = right ? "R …" : "L …";

                return new HandViz(right, palmLine, compareLine, label);
            }

            static LineRenderer CreateLine(Transform parent, string name, Color color)
            {
                var go = new GameObject(name);
                go.transform.SetParent(parent, false);
                var lr = go.AddComponent<LineRenderer>();
                lr.useWorldSpace = true;
                lr.positionCount = 2;
                lr.startWidth = 0.004f;
                lr.endWidth = 0.004f;
                lr.startColor = lr.endColor = color;
                lr.material = LineMaterial(color);
                lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lr.receiveShadows = false;
                lr.numCapVertices = 4;
                return lr;
            }

            static Material LineMaterial(Color color)
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit")
                    ?? Shader.Find("Unlit/Color")
                    ?? Shader.Find("Sprites/Default");
                var mat = new Material(shader);
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", color);
                else
                    mat.color = color;
                return mat;
            }

            public void Refresh()
            {
                if (!Lexting.controllers.Ready())
                    return;

                var hand = Lexting.input.hand(_right);
                if (!hand.palm.Ready())
                    return;

                var origin = hand.palm.position;
                var marker = _right ? Lexting.rHandMarker : Lexting.lHandMarker;
                if (marker == null)
                    return;

                var palmNormal = QuestHandPalmNormal.Compute(_right, hand, QuestHandPalmNormal.DefaultSource);
                if (palmNormal.sqrMagnitude < 1e-6f)
                    palmNormal = marker.palmNormal;
                if (palmNormal.sqrMagnitude < 1e-6f)
                    return;

                palmNormal.Normalize();
                var markerNormal = marker.palmNormal;
                var len = QuestHandRotationDebugSettings.VizLineLength;
                SetLine(_palmLine, origin, origin + palmNormal * len);

                var cube = Lexting.controllers.transform;
                var heldState = Lexting.rotState(_right);
                var compareAxis = QuestHandRotationMapping.ComparisonAxis(heldState, cube, _right).normalized;
                SetLine(_compareLine, origin, origin + compareAxis * len);

                var dots = QuestHandRotationMapping.Measure(palmNormal, cube, _right);
                var cubeRelaxAxis = (_right ? 1f : -1f) * cube.right;
                var relaxCube = Vector3.Dot(palmNormal, cubeRelaxAxis);
                var fwdCube = Vector3.Dot(palmNormal, -cube.forward);
                var backCube = Vector3.Dot(palmNormal, cube.forward);
                var matched = QuestHandRotationMapping.TryClassify(palmNormal, cube, _right, out var classified, out var classifyDots);
                var status = matched ? classified.ToString() : $"{heldState}?";
                var activeDot = dots.DotForState(matched ? classified : heldState);
                var palmSrc = QuestHandPalmNormal.DefaultSource;
                var legacyDiff = markerNormal.sqrMagnitude > 1e-6f
                    ? Vector3.Dot(palmNormal, markerNormal.normalized)
                    : 0f;
                _label.text =
                    $"{(_right ? "R" : "L")} hold={heldState} cls={status} d={activeDot:F2}\n" +
                    $"relax={dots.Relax:F2} r·cube={relaxCube:F2} fwd={dots.Forward:F2} f·cube={fwdCube:F2}\n" +
                    $"back={dots.OtherBack:F2} b·cube={backCube:F2}\n{palmSrc} XR={QuestHandBindingState.UnityXRHandsActive}\n{dots}";

                var labelPos = origin + compareAxis * (len + 0.015f);
                _labelTransform.position = labelPos;
                FaceLabelToHead();
            }

            void FaceLabelToHead()
            {
                if (Lexting.head.Ready())
                {
                    var toHead = Lexting.head.transform.position - _labelTransform.position;
                    if (toHead.sqrMagnitude > 1e-6f)
                        _labelTransform.rotation = Quaternion.LookRotation(-toHead.normalized, Vector3.up);
                }
            }

            static void SetLine(LineRenderer lr, Vector3 a, Vector3 b)
            {
                lr.SetPosition(0, a);
                lr.SetPosition(1, b);
            }
        }
    }
}
#endif
