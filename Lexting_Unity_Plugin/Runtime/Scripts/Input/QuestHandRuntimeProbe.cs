#if !UNITY_STANDALONE && UNITY_ANDROID
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace SpatialGestureText.Input
{
    /// <summary>
    /// Emits <c>adb logcat -s LextingHands</c> lines on release builds for hand pipeline diagnosis.
    /// </summary>
    public sealed class QuestHandRuntimeProbe : MonoBehaviour
    {
        static readonly List<XRHandSubsystem> Subsystems = new List<XRHandSubsystem>();
        float _nextLog;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Attach()
        {
            var go = new GameObject("[Lexting] HandProbe");
            go.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(go);
            go.AddComponent<QuestHandRuntimeProbe>();
        }

        void Update()
        {
            if (Time.unscaledTime < _nextLog)
                return;

            _nextLog = Time.unscaledTime + 5f;

            Subsystems.Clear();
            SubsystemManager.GetSubsystems(Subsystems);

            var running = false;
            XRHandSubsystem active = null;
            foreach (var s in Subsystems)
            {
                if (s == null || !s.running)
                    continue;
                running = true;
                active = s;
                break;
            }

            var tracked = active != null && active.leftHand.isTracked && active.rightHand.isTracked;

#if LEXTING_FOUND_OCULUS_SDK
            var ovrInit = OVRPlugin.initialized;
            var xrApi = OVRPlugin.nativeXrApi.ToString();
            QuestHandLog.Info(
                $"probe ovrInit={ovrInit} xrApi={xrApi} handSubsystemRunning={running} bothTracked={tracked}");
#else
            QuestHandLog.Info(
                $"probe handSubsystemRunning={running} bothTracked={tracked}");
#endif
        }
    }
}
#endif
