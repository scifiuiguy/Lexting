// Disabled for OpenXR + XR Hands on Quest (010+). Re-enable only when Android loader is Oculus XR Plugin-only.
#if LEXTING_QUEST_EARLY_OVR_BOOTSTRAP && !UNITY_EDITOR && UNITY_ANDROID
using UnityEngine;

namespace SpatialGestureText.Input
{
    static class QuestEarlyOvrBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void EnsureOvrManagerBeforeXrLoaders()
        {
            if (OVRManager.instance != null)
                return;

            var go = new GameObject("[Lexting] OVRManager");
            go.hideFlags = HideFlags.HideAndDontSave;
            Object.DontDestroyOnLoad(go);
            var manager = go.AddComponent<OVRManager>();
            manager.enableMixedReality = false;
        }
    }
}
#endif
