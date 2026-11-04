namespace SpatialGestureText.Input
{
    /// <summary>
    /// Which Quest hand skeleton path is active after <see cref="OpenXRHandInput"/> bind (mapping layer).
    /// </summary>
    public static class QuestHandBindingState
    {
        public static bool UnityXRHandsActive { get; internal set; }
        public static bool OvrLegacyActive { get; internal set; }

        internal static void SetUnityXRHands()
        {
            UnityXRHandsActive = true;
            OvrLegacyActive = false;
        }

        internal static void SetOvrLegacy()
        {
            UnityXRHandsActive = false;
            OvrLegacyActive = true;
        }
    }
}
