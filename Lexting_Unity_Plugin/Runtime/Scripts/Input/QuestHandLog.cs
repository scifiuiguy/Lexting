#if !UNITY_STANDALONE && UNITY_ANDROID
using UnityEngine;

namespace SpatialGestureText.Input
{
    internal static class QuestHandLog
    {
        const string Tag = "LextingHands";

        public static void Info(string message)
        {
            Debug.Log(message);
            try
            {
                using var log = new AndroidJavaClass("android.util.Log");
                log.CallStatic<int>("i", Tag, message);
            }
            catch
            {
                // ignored
            }
        }

        public static void Warn(string message)
        {
            Debug.LogWarning(message);
            try
            {
                using var log = new AndroidJavaClass("android.util.Log");
                log.CallStatic<int>("w", Tag, message);
            }
            catch
            {
                // ignored
            }
        }
    }
}
#endif
