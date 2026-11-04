#if !UNITY_STANDALONE && UNITY_ANDROID
namespace SpatialGestureText.Input
{
    /// <summary>
    /// Quest-only wrist rotation mapping debug. Set <see cref="Enabled"/> to false (or remove
    /// <see cref="QuestHandRotationDebugVisualizer"/> bootstrap) to strip viz + extra logs from builds.
    /// </summary>
    public static class QuestHandRotationDebugSettings
    {
        /// <summary>Log <c>adb logcat -s LextingHands</c> and match this string to confirm the APK on headset.</summary>
        public const string BuildTag = "quest-head-sagittal-022";

        public const bool Enabled = false;
        public const bool LogOnStateChange = false;
        public const float VizLineLength = 0.045f;
        public const float LabelCharacterSize = 0.006f;

        /// <summary>Label only — production rotation uses <see cref="QuestHandPalmNormal.DefaultSource"/>.</summary>
        public static QuestHandPalmNormal.Source PalmNormalSource => QuestHandPalmNormal.DefaultSource;
    }
}
#endif
