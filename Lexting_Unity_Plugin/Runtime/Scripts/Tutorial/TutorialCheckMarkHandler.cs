using System.Collections;
using SpatialGestureText.Utilities;
using UnityEngine;
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
        public const float StrokeDurationSeconds = 0.3f;

        private const float StrokeDebugSlowdown = 1f;

        private static float EffectiveStrokeDurationSeconds => StrokeDurationSeconds * StrokeDebugSlowdown;
        public static float FullOnStrokeSeconds => EffectiveStrokeDurationSeconds * 2f;

        private const float CheckmarkWhooshVolume = 0.5f;

        [SerializeField] private Transform waypointStart;
        [SerializeField] private Transform waypointKnuckle;
        [SerializeField] private Transform waypointEnd;

        private LineRenderer _lineRenderer;
        private Transform _animKnuckle;
        private Transform _animEnd;
        private Coroutine _drawCoroutine;
        private int _drawGeneration;
        private AnimationCurve _taperWidthCurve;
        private AnimationCurve _knuckleDrawWidthCurve;
        private AnimationCurve _knuckleEraseWidthCurve;
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
            // Keep prefab taper (thin tips, thick mid-stroke); only fill in if missing.
            if (_lineRenderer.widthCurve == null || _lineRenderer.widthCurve.length == 0)
            {
                var curve = new AnimationCurve();
                curve.AddKey(0f, 0f);
                curve.AddKey(0.5f, 1f);
                curve.AddKey(1f, 0f);
                _lineRenderer.widthCurve = curve;
            }

            CacheTaperWidthCurveFromRenderer();
            _lineRenderer.alignment = LineAlignment.View;
            _lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _lineRenderer.receiveShadows = false;
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

            if (_drawCoroutine != null)
            {
                StopCoroutine(_drawCoroutine);
                _drawCoroutine = null;
            }

            _drawGeneration++;
            _drawCoroutine = StartCoroutine(DrawCheckmarkRoutine(on, _drawGeneration));
        }

        private IEnumerator DrawCheckmarkRoutine(bool on, int generation)
        {
            if (_lineRenderer != null)
                _lineRenderer.enabled = true;

            if (on)
            {
                SnapAnimAnchorsToStart();
                yield return StrokeKnuckleOn(generation);
                if (generation != _drawGeneration)
                    yield break;
                yield return StrokeEndOn(generation);
            }
            else
            {
                yield return StrokeEndOff(generation);
                if (generation != _drawGeneration)
                    yield break;
                yield return StrokeKnuckleOff(generation);
                if (generation != _drawGeneration)
                    yield break;
                CollapseLineToStart();
            }

            if (generation == _drawGeneration)
                _drawCoroutine = null;
        }

        private IEnumerator StrokeKnuckleOn(int generation)
        {
            Lexting.audio?.Play("Whoosh_1", CheckmarkWhooshVolume);
            _animEnd.position = waypointStart.position;
            yield return LerpAlongWaypoints(generation, true, (start, knuckle, t) =>
            {
                _animKnuckle.position = Vector3.Lerp(start, knuckle, t);
                if (t >= 1f)
                    ApplyKnuckleOnEndHandoff(start);
                else
                    ApplyKnuckleLegStroke(start, _animKnuckle.position, drawingOn: true);
            });
        }

        private IEnumerator StrokeEndOn(int generation)
        {
            Lexting.audio?.Play("Whoosh_2", CheckmarkWhooshVolume);
            var elapsed = 0f;
            while (elapsed < EffectiveStrokeDurationSeconds && generation == _drawGeneration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / EffectiveStrokeDurationSeconds);
                t = 1f - (1f - t) * (1f - t);
                var start = waypointStart.position;
                var knuckle = waypointKnuckle.position;
                var end = waypointEnd.position;
                _animKnuckle.position = knuckle;
                _animEnd.position = Vector3.Lerp(start, end, t);
                SetLineThreePoint(start, knuckle, _animEnd.position);
                yield return null;
            }

            if (generation == _drawGeneration)
            {
                var start = waypointStart.position;
                var knuckle = waypointKnuckle.position;
                _animKnuckle.position = knuckle;
                _animEnd.position = waypointEnd.position;
                SetLineThreePoint(start, knuckle, _animEnd.position);
            }
        }

        private IEnumerator StrokeEndOff(int generation)
        {
            Lexting.audio?.Play("Whoosh_1", CheckmarkWhooshVolume);
            var elapsed = 0f;
            while (elapsed < EffectiveStrokeDurationSeconds && generation == _drawGeneration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / EffectiveStrokeDurationSeconds);
                t = t * t;
                var start = waypointStart.position;
                var knuckle = waypointKnuckle.position;
                var end = waypointEnd.position;
                _animKnuckle.position = knuckle;
                _animEnd.position = Vector3.Lerp(end, knuckle, t);
                SetLineThreePoint(start, knuckle, _animEnd.position);
                yield return null;
            }

            if (generation == _drawGeneration)
            {
                var start = waypointStart.position;
                var knuckle = waypointKnuckle.position;
                _animKnuckle.position = knuckle;
                _animEnd.position = knuckle;
                SetLineThreePoint(start, knuckle, knuckle);
            }
        }

        private IEnumerator StrokeKnuckleOff(int generation)
        {
            Lexting.audio?.Play("Whoosh_2", CheckmarkWhooshVolume);
            _animEnd.position = waypointKnuckle.position;
            yield return LerpAlongWaypoints(generation, false, (start, knuckle, t) =>
            {
                _animKnuckle.position = Vector3.Lerp(knuckle, start, t);
                if (t >= 1f)
                {
                    ApplyTaperWidthCurve();
                    CollapseLineToStart();
                }
                else
                    ApplyKnuckleLegStroke(start, _animKnuckle.position, drawingOn: false);
            });
        }

        /// <summary>Re-samples start/knuckle waypoints each frame (hand may move).</summary>
        private IEnumerator LerpAlongWaypoints(int generation, bool easeIn, System.Action<Vector3, Vector3, float> apply)
        {
            var elapsed = 0f;
            while (elapsed < EffectiveStrokeDurationSeconds && generation == _drawGeneration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / EffectiveStrokeDurationSeconds);
                t = easeIn ? t * t : 1f - (1f - t) * (1f - t);
                apply(waypointStart.position, waypointKnuckle.position, t);
                yield return null;
            }

            if (generation == _drawGeneration)
                apply(waypointStart.position, waypointKnuckle.position, 1f);
        }

        private void CacheTaperWidthCurveFromRenderer()
        {
            if (_lineRenderer == null || _taperWidthCurve != null)
                return;

            var src = _lineRenderer.widthCurve;
            if (src == null || src.length == 0)
                return;

            _taperWidthCurve = new AnimationCurve();
            foreach (var key in src.keys)
                _taperWidthCurve.AddKey(key);
            RebuildKnuckleLegWidthCurves();
        }

        /// <summary>
        /// Maps the prefab taper onto a 2-point leg: draw uses taper 0→0.5 (same as folded knuckle at mid-path),
        /// erase uses taper 0.5→1 so the knuckle end stays thick while shrinking.
        /// </summary>
        private void RebuildKnuckleLegWidthCurves()
        {
            if (_taperWidthCurve == null)
                return;

            _knuckleDrawWidthCurve = BuildLegWidthCurve(s => _taperWidthCurve.Evaluate(s * 0.5f));
            _knuckleEraseWidthCurve = BuildLegWidthCurve(s => _taperWidthCurve.Evaluate(1f - s * 0.5f));
        }

        private static AnimationCurve BuildLegWidthCurve(System.Func<float, float> widthAtLineT)
        {
            const int samples = 12;
            var curve = new AnimationCurve();
            for (var i = 0; i <= samples; i++)
            {
                var t = i / (float)samples;
                curve.AddKey(t, Mathf.Clamp01(widthAtLineT(t)));
            }

            for (var i = 0; i < curve.length; i++)
                curve.SmoothTangents(i, 0f);
            return curve;
        }

        private void EnsureKnuckleLegWidthCurves()
        {
            if (_knuckleDrawWidthCurve != null && _knuckleEraseWidthCurve != null)
                return;
            if (_taperWidthCurve == null)
                CacheTaperWidthCurveFromRenderer();
            RebuildKnuckleLegWidthCurves();
        }

        private void ApplyTaperWidthCurve()
        {
            if (_lineRenderer == null)
                return;
            if (_taperWidthCurve != null)
                _lineRenderer.widthCurve = _taperWidthCurve;
        }

        /// <summary>Stable 2-point leg; width follows first half of prefab taper (not full 0→1 linear).</summary>
        private void ApplyKnuckleLegStroke(Vector3 start, Vector3 knuckleHead, bool drawingOn)
        {
            if (_lineRenderer == null)
                return;
            EnsureKnuckleLegWidthCurves();
            _lineRenderer.widthCurve = drawingOn ? _knuckleDrawWidthCurve : _knuckleEraseWidthCurve;
            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPositions(new[] { start, knuckleHead });
        }

        /// <summary>Match end-leg start pose (knuckle held, end at start) with prefab taper before phase 2.</summary>
        private void ApplyKnuckleOnEndHandoff(Vector3 start)
        {
            if (_lineRenderer == null)
                return;
            ApplyTaperWidthCurve();
            _animEnd.position = start;
            SetLineThreePoint(start, waypointKnuckle.position, start);
        }

        private void SetLineThreePoint(Vector3 start, Vector3 knuckle, Vector3 end)
        {
            if (_lineRenderer == null)
                return;
            _lineRenderer.positionCount = 3;
            _lineRenderer.SetPositions(new[] { start, knuckle, end });
        }

#if UNITY_EDITOR
        private void ApplyCheckmarkImmediateForEditor(bool on)
        {
            if (_lineRenderer != null)
                _lineRenderer.enabled = true;

            if (on)
            {
                SetLineThreePoint(waypointStart.position, waypointKnuckle.position, waypointEnd.position);
            }
            else
                CollapseLineToStart();

            SceneView.RepaintAll();
            if (gameObject.scene.IsValid())
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif

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
            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPositions(new[] { p, p });
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
