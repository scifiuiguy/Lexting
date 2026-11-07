using Pixelplacement;
using Pixelplacement.TweenSystem;
using SpatialGestureText.Utilities;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace SpatialGestureText.Tutorial
{
    /// <summary>
    /// Draws a 3-point check stroke. Place <see cref="waypointStart"/>, <see cref="waypointKnuckle"/>,
    /// and <see cref="waypointEnd"/> empties in the editor; runtime anim anchors lerp between them.
    /// Inspector <b>Check on / Check off</b> preview the stroke in Edit Mode (instant pose, no tweens).
    /// </summary>
    public class TutorialCheckMarkHandler : MonoBehaviour
    {
        private const float CheckmarkStrokeDuration = 0.3f;
        private const float CheckmarkWhooshVolume = 0.5f;

        [SerializeField] private Transform waypointStart;
        [SerializeField] private Transform waypointKnuckle;
        [SerializeField] private Transform waypointEnd;

        private LineRenderer _lineRenderer;
        private Transform _animKnuckle;
        private Transform _animEnd;
        private bool _isMoving;
        private TweenBase _knuckleTween;
        private TweenBase _endTween;
        public bool checkOn;

        private void Awake()
        {
            EnsureRuntimeReady();
        }

        /// <summary>Idempotent init (Awake order-safe; safe to call from tutorial before Unity runs Awake).</summary>
        public void EnsureRuntimeReady()
        {
            ResolveWaypoints();
            CreateAnimAnchors();
            InitLineRenderer();
        }

        private void ResolveWaypoints()
        {
            waypointStart = waypointStart
                            ?? transform.Find("CheckmarkWaypoints/Waypoint_Start")
                            ?? transform.Find("Waypoint_Start");
            waypointKnuckle = waypointKnuckle
                              ?? transform.Find("CheckmarkWaypoints/Waypoint_Knuckle")
                              ?? transform.Find("Waypoint_Knuckle")
                              ?? transform.Find("knuckle");
            waypointEnd = waypointEnd
                          ?? transform.Find("CheckmarkWaypoints/Waypoint_End")
                          ?? transform.Find("Waypoint_End")
                          ?? transform.Find("end");

            if (waypointStart == null)
            {
                var go = new GameObject("Waypoint_Start");
                go.transform.SetParent(transform, false);
                waypointStart = go.transform;
            }

            if (waypointKnuckle == null || waypointEnd == null)
                Debug.LogWarning($"{nameof(TutorialCheckMarkHandler)} on {name}: assign knuckle/end waypoints in the editor.", this);
        }

        private void InitLineRenderer()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            PrepareLineRenderer();
            if (_lineRenderer == null)
                return;
            _lineRenderer.positionCount = 3;
            CollapseLineToStart();
        }

        private void CreateAnimAnchors()
        {
            if (_animKnuckle == null)
                _animKnuckle = CreateAnimAnchor("Anim_Knuckle");
            if (_animEnd == null)
                _animEnd = CreateAnimAnchor("Anim_End");
        }

        private Transform CreateAnimAnchor(string anchorName)
        {
            var existing = transform.Find(anchorName);
            if (existing != null)
                return existing;

            var go = new GameObject(anchorName);
            go.transform.SetParent(transform, false);
            go.hideFlags = HideFlags.HideInHierarchy;
            return go.transform;
        }

        private void PrepareLineRenderer()
        {
            if (_lineRenderer == null)
                _lineRenderer = GetComponent<LineRenderer>();
            if (_lineRenderer == null)
                return;

            _lineRenderer.useWorldSpace = true;
            _lineRenderer.widthMultiplier = 1f;
            _lineRenderer.startWidth = 0.004f;
            _lineRenderer.endWidth = 0.004f;
            if (_lineRenderer.widthCurve == null || _lineRenderer.widthCurve.length == 0)
            {
                var curve = new AnimationCurve();
                curve.AddKey(0f, 0f);
                curve.AddKey(0.5f, 1f);
                curve.AddKey(1f, 0f);
                _lineRenderer.widthCurve = curve;
            }
            _lineRenderer.alignment = LineAlignment.View;
            _lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _lineRenderer.receiveShadows = false;
        }

        private void Update()
        {
            if (!_isMoving || _lineRenderer == null || waypointStart == null)
                return;

            _lineRenderer.SetPositions(new[]
            {
                waypointStart.position,
                _animKnuckle.position,
                _animEnd.position
            });
        }

        public void DrawCheckmark(bool on)
        {
            checkOn = on;
            EnsureRuntimeReady();
            if (waypointStart == null || waypointKnuckle == null || waypointEnd == null)
                return;
            if (_animKnuckle == null || _animEnd == null)
                return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                ApplyCheckmarkImmediateForEditor(on);
                return;
            }
#endif

            StopActiveTweens();
            _isMoving = true;
            if (_lineRenderer != null)
                _lineRenderer.enabled = true;

            if (on)
            {
                SnapAnimAnchorsToStart();
                DrawKnucklePhase(on, () => DrawEndPhase(on, () => _isMoving = false));
            }
            else
            {
                DrawEndPhase(on, () => DrawKnucklePhase(on, () =>
                {
                    _isMoving = false;
                    CollapseLineToStart();
                }));
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Edit Mode / paused inspector preview: sets anim anchors to final on or off pose and updates the LineRenderer
        /// (Pixelplacement tweens and <see cref="MonoBehaviour.StartCoroutine"/> do not run outside Play Mode).
        /// </summary>
        private void ApplyCheckmarkImmediateForEditor(bool on)
        {
            StopActiveTweens();
            _isMoving = false;
            if (_lineRenderer != null)
                _lineRenderer.enabled = true;

            if (on)
            {
                _animKnuckle.position = waypointKnuckle.position;
                _animEnd.position = waypointEnd.position;
                FlushLineRendererPositions();
            }
            else
                CollapseLineToStart();

            SceneView.RepaintAll();
            if (gameObject.scene.IsValid())
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }

        private void FlushLineRendererPositions()
        {
            if (_lineRenderer == null || waypointStart == null || _animKnuckle == null || _animEnd == null)
                return;
            _lineRenderer.SetPositions(new[]
            {
                waypointStart.position,
                _animKnuckle.position,
                _animEnd.position
            });
        }
#endif

        private void DrawKnucklePhase(bool on, UnityAction callback)
        {
            Lexting.audio?.Play(on ? "Whoosh_1" : "Whoosh_2", CheckmarkWhooshVolume);

            if (on)
            {
                _animKnuckle.position = waypointStart.position;
                _animEnd.position = waypointStart.position;
                _knuckleTween = Tween.Position(_animKnuckle, waypointKnuckle.position, CheckmarkStrokeDuration, 0, Tween.EaseIn, Tween.LoopType.None, null,
                    () => callback.I());
            }
            else
            {
                // Lerp-OFF phase 2: knuckle → start. End was already merged into knuckle in phase 1; do not snap back to waypointEnd.
                _animKnuckle.position = waypointKnuckle.position;
                if (_animEnd != null)
                    _animEnd.position = waypointKnuckle.position;
                _knuckleTween = Tween.Position(_animKnuckle, waypointStart.position, CheckmarkStrokeDuration, 0, Tween.EaseOut, Tween.LoopType.None, null,
                    () => callback.I());
            }
        }

        private void DrawEndPhase(bool on, UnityAction callback)
        {
            Lexting.audio?.Play(on ? "Whoosh_2" : "Whoosh_1", CheckmarkWhooshVolume);

            if (on)
            {
                _animEnd.position = waypointStart.position;
                _endTween = Tween.Position(_animEnd, waypointEnd.position, CheckmarkStrokeDuration, 0, Tween.EaseOut, Tween.LoopType.None, null,
                    () => callback.I());
            }
            else
            {
                _animKnuckle.position = waypointKnuckle.position;
                _animEnd.position = waypointEnd.position;
                _endTween = Tween.Position(_animEnd, waypointKnuckle.position, CheckmarkStrokeDuration, 0, Tween.EaseIn, Tween.LoopType.None, null,
                    () => callback.I());
            }
        }

        private void StopActiveTweens()
        {
            _knuckleTween?.Stop();
            _endTween?.Stop();
            _knuckleTween = null;
            _endTween = null;
        }

        private void SnapAnimAnchorsToStart()
        {
            if (waypointStart == null || _animKnuckle == null || _animEnd == null)
                return;
            _animKnuckle.position = waypointStart.position;
            _animEnd.position = waypointStart.position;
        }

        private void CollapseLineToStart()
        {
            SnapAnimAnchorsToStart();
            if (_lineRenderer == null || waypointStart == null)
                return;

            var p = waypointStart.position;
            _lineRenderer.SetPositions(new[] { p, p, p });
        }

        public IEnumerator UnitTest_DrawCheckmarkOn()
        {
            DrawCheckmark(true);
            yield break;
        }

        public IEnumerator UnitTest_DrawCheckmarkOff()
        {
            DrawCheckmark(false);
            yield break;
        }
    }
}
