using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Samples.VisualizerSample;

namespace SpatialGestureText.Input
{
    /// <summary>
    /// Resolves hand mesh roots spawned by Unity's <see cref="HandVisualizer"/> sample under an XRRig.
    /// </summary>
    internal static class XrHandVisualizerUtility
    {
        public static Transform FindHandRoot(Transform xrHandsRoot, bool right)
        {
            if (xrHandsRoot == null)
                return null;

            var handVisualizer = xrHandsRoot.Find("HandVisualizer");
            if (handVisualizer == null)
                return null;

            var want = right ? Handedness.Right : Handedness.Left;
            foreach (Transform child in handVisualizer)
            {
                var events = child.GetComponent<XRHandTrackingEvents>();
                if (events != null && events.handedness == want)
                    return child;
            }

            var legacyName = right ? "HandVisualizer/RightHand(Clone)" : "HandVisualizer/LeftHand(Clone)";
            return xrHandsRoot.Find(legacyName);
        }

        public static Transform TryGetJoint(Transform handRoot, XRHandJointID jointId)
        {
            if (handRoot == null)
                return null;

            var driver = handRoot.GetComponent<XRHandSkeletonDriver>();
            if (driver?.jointTransformReferences == null)
                return null;

            foreach (var reference in driver.jointTransformReferences)
            {
                if (reference.jointTransform != null && reference.xrHandJointID == jointId)
                    return reference.jointTransform;
            }

            return null;
        }
    }
}
