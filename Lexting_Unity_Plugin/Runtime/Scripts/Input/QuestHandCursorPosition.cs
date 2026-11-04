using SpatialGestureText.Utilities;
using UnityEngine;

namespace SpatialGestureText.Input
{
    /// <summary>
    /// Quest / XR Hands world position for hand cursors (mapping layer — not cube logic).
    /// </summary>
    public static class QuestHandCursorPosition
    {
        public static bool HasTrackingPoint(bool right)
        {
            var hand = Lexting.input.hand(right);
            return hand.indexBase.Ready() || hand.palm.Ready() || hand.trackingWrist.Ready();
        }

        public static Vector3 GetWorldPosition(bool right)
        {
            var hand = Lexting.input.hand(right);
            if (hand.indexBase.Ready())
                return hand.indexBase.position;
            if (hand.palm.Ready())
                return hand.palm.position;
            if (hand.trackingWrist.Ready())
                return hand.trackingWrist.position;

            var marker = right ? Lexting.rHandMarker : Lexting.lHandMarker;
            if (marker != null && marker.cursorAnchor != null)
                return marker.cursorAnchor.transform.position;
            return Vector3.zero;
        }
    }
}
