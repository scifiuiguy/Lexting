using SpatialGestureText.LetterCube;
using SpatialGestureText.Utilities;
using UnityEngine;

namespace SpatialGestureText.Input
{
    /// <summary>
    /// Shared palm-normal vs cube-axis tests used by <see cref="OpenXRHandInput"/> rotation detection
    /// and Quest rotation debug tooling (must stay in sync with legacy thresholds).
    /// </summary>
    public static class QuestHandRotationMapping
    {
        public struct DotSample
        {
            public float Forward;
            public float Relax;
            public float Supinate;
            public float Pronate;
            public float OtherBack;

            public float DotForState(CubeRotationState state) => state switch
            {
                CubeRotationState.relax => Relax,
                CubeRotationState.pronate => Pronate,
                CubeRotationState.supinate => Supinate,
                CubeRotationState.forward => Forward,
                CubeRotationState.other => OtherBack,
                _ => 0f
            };

            public override string ToString() =>
                $"fwd={Forward:F2} relax={Relax:F2} sup={Supinate:F2} pro={Pronate:F2} back={OtherBack:F2}";
        }

        /// <summary>
        /// Lateral axis for relax dots. On Quest, use head-stabilized lateral so classification
        /// does not flip when the widget calibrates from world origin (identity right ≈ +X) to
        /// <see cref="HeadMarker.headRight"/> — palm normal debug lines stay world-fixed; only this axis moved.
        /// </summary>
        public static Vector3 RelaxComparisonAxis(bool right, Transform cubeTransform)
        {
#if !UNITY_STANDALONE && UNITY_ANDROID
            if (Lexting.head.Ready())
            {
                var lateral = Lexting.head.headRight.normalized;
                return (right ? -1f : 1f) * lateral;
            }
#endif
            return (right ? 1f : -1f) * cubeTransform.right;
        }

        /// <summary>
        /// Palm "forward" (toward the typing UI). Uses head flat look on Quest so it matches
        /// post-summon calibration; the old ±cube.forward swap was tuned while the widget sat at
        /// world origin (often behind the user) with identity rotation.
        /// </summary>
        public static Vector3 ForwardComparisonAxis(Transform cubeTransform)
        {
#if !UNITY_STANDALONE && UNITY_ANDROID
            if (Lexting.head.Ready())
                return Lexting.head.flatLookDirection.normalized;
#endif
            return cubeTransform.forward;
        }

        /// <summary>Palm back / "other" — opposite of <see cref="ForwardComparisonAxis"/>.</summary>
        public static Vector3 OtherBackComparisonAxis(Transform cubeTransform)
        {
#if !UNITY_STANDALONE && UNITY_ANDROID
            if (Lexting.head.Ready())
                return -Lexting.head.flatLookDirection.normalized;
#endif
            return -cubeTransform.forward;
        }

        public static DotSample Measure(Vector3 palmNormal, Transform cubeTransform, bool right)
        {
            var n = palmNormal.normalized;
            var relaxAxis = RelaxComparisonAxis(right, cubeTransform);
            var forwardAxis = ForwardComparisonAxis(cubeTransform);
            var otherAxis = OtherBackComparisonAxis(cubeTransform);
            return new DotSample
            {
                Forward = Vector3.Dot(n, forwardAxis),
                Relax = Vector3.Dot(n, relaxAxis),
                Supinate = Vector3.Dot(n, Vector3.up),
                Pronate = Vector3.Dot(n, Vector3.down),
                OtherBack = Vector3.Dot(n, otherAxis)
            };
        }

        /// <summary>
        /// Same priority order as <see cref="OpenXRHandInput"/> rotation loop.
        /// </summary>
        public static bool TryClassify(
            Vector3 palmNormal,
            Transform cubeTransform,
            bool right,
            out CubeRotationState state,
            out DotSample dots)
        {
            dots = Measure(palmNormal, cubeTransform, right);

            if (dots.Forward > 0.6f)
            {
                state = CubeRotationState.forward;
                return true;
            }

            if (dots.Relax > 0.85f)
            {
                state = CubeRotationState.relax;
                return true;
            }

            if (dots.Supinate > 0.65f)
            {
                state = CubeRotationState.supinate;
                return true;
            }

            if (dots.Pronate > 0.65f)
            {
                state = CubeRotationState.pronate;
                return true;
            }

            if (dots.OtherBack > 0.75f)
            {
                state = CubeRotationState.other;
                return true;
            }

            state = default;
            return false;
        }

        public static Vector3 ComparisonAxis(CubeRotationState state, Transform cubeTransform, bool right)
        {
            return state switch
            {
                CubeRotationState.forward => ForwardComparisonAxis(cubeTransform),
                CubeRotationState.relax => RelaxComparisonAxis(right, cubeTransform),
                CubeRotationState.supinate => Vector3.up,
                CubeRotationState.pronate => Vector3.down,
                CubeRotationState.other => OtherBackComparisonAxis(cubeTransform),
                _ => ForwardComparisonAxis(cubeTransform)
            };
        }
    }
}
