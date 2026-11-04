using UnityEngine;

namespace SpatialGestureText.Input
{
    /// <summary>
    /// World-space palm facing direction for Quest hand tracking (mapping layer only).
    /// </summary>
    public static class QuestHandPalmNormal
    {
        /// <summary>Production Quest palm normal (independent of rotation debug viz).</summary>
        public const Source DefaultSource = Source.AnatomyCross;

        public enum Source
        {
            /// <summary>Same convention as Leap palm markers: outward normal ≈ -palm.up.</summary>
            PalmJointNegUp = 0,
            PalmJointNegForward = 1,
            /// <summary>Legacy OVR-style cross: (index−little) × (wrist−middleMetacarpal).</summary>
            AnatomyCross = 2,
            /// <summary>Pre–XR Hands heuristic (index−middle) × (wrist marker−middleTip) — debug only.</summary>
            LegacyCrossIndexMiddle = 3
        }

        public static Vector3 Compute(bool right, in LextingHand hand, Source source)
        {
            switch (source)
            {
                case Source.PalmJointNegUp:
                    return FromPalmJoint(right, hand, useUp: true);
                case Source.PalmJointNegForward:
                    return FromPalmJoint(right, hand, useUp: false);
                case Source.AnatomyCross:
                    return FromAnatomyCross(right, hand);
                case Source.LegacyCrossIndexMiddle:
                    return FromLegacyCrossIndexMiddle(right, hand);
                default:
                    return FromPalmJoint(right, hand, useUp: true);
            }
        }

        static Vector3 FromPalmJoint(bool right, in LextingHand hand, bool useUp)
        {
            if (!hand.palm.Ready())
                return Vector3.zero;

            // When bind failed, palm falls back to wrist/marker — joint rotation is not meaningful.
            if (hand.trackingWrist.Ready() && hand.palm == hand.trackingWrist)
                return Vector3.zero;
            if (hand.wrist.Ready() && hand.palm == hand.wrist)
                return Vector3.zero;

            // OpenXR / XR Hands palm joint frames are already mirrored per side; extra ±1 per hand
            // makes both normals point the same way in relax (breaks right-hand relax dots).
            return useUp ? -hand.palm.up : -hand.palm.forward;
        }

        static Vector3 FromAnatomyCross(bool right, in LextingHand hand)
        {
            if (!hand.indexBase.Ready() || !hand.littleBase.Ready() || !hand.middleBase.Ready())
                return Vector3.zero;

            var wrist = hand.trackingWrist.Ready() ? hand.trackingWrist : hand.wrist;
            if (!wrist.Ready())
                return Vector3.zero;

            // OVR-style plane from bone positions (index↔little across palm, middle→wrist).
            // Handedness comes from skeleton; no extra ±1 (that broke right-hand relax on joint -up).
            var acrossPalm = (hand.indexBase.position - hand.littleBase.position).normalized;
            var towardWrist = (wrist.position - hand.middleBase.position).normalized;
            if (acrossPalm.sqrMagnitude < 1e-6f || towardWrist.sqrMagnitude < 1e-6f)
                return Vector3.zero;

            var n = Vector3.Cross(acrossPalm, towardWrist);
            if (n.sqrMagnitude < 1e-6f)
                return Vector3.zero;

            n.Normalize();
            // Prefer outward normal (-palm.up). Do not skip when palm falls back to trackingWrist (same Transform).
            if (hand.palm.Ready())
            {
                var outward = -hand.palm.up;
                if (outward.sqrMagnitude > 1e-6f && Vector3.Dot(n, outward) < 0f)
                    n = -n;
            }

            return n;
        }

        static Vector3 FromLegacyCrossIndexMiddle(bool right, in LextingHand hand)
        {
            if (!hand.indexBase.Ready() || !hand.middleBase.Ready() || !hand.middleTip.Ready())
                return Vector3.zero;

            var wrist = hand.wrist;
            if (!wrist.Ready())
                return Vector3.zero;

            var knuckle = (hand.indexBase.position - hand.middleBase.position).normalized;
            var wristToTip = (wrist.position - hand.middleTip.position).normalized;
            if (knuckle.sqrMagnitude < 1e-6f || wristToTip.sqrMagnitude < 1e-6f)
                return Vector3.zero;

            var sign = right ? 1f : -1f;
            return sign * Vector3.Cross(knuckle, wristToTip).normalized;
        }
    }
}
