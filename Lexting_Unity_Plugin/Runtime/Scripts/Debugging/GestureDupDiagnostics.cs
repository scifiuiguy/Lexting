using System.Reflection;
using SpatialGestureText.Input;
using SpatialGestureText.LetterCube;
using SpatialGestureText.TwoZoneCube;
using UnityEngine;
using UnityEngine.Events;

namespace SpatialGestureText.Debugging
{
    /// <summary>
    /// One-off QA instrumentation for triple insert / duplicate OnStateChanged delivery.
    /// Set <see cref="Enabled"/> to false or delete this file after root cause is confirmed.
    /// Grep logcat: GestureDup
    /// </summary>
    public static class GestureDupDiagnostics
    {
        public const bool Enabled = true;

        const string Prefix = "[GestureDup]";

        static int _forwardWave;
        static int _deliveryIndexInWave;
        static int _xrBindingsInitAttempts;
        static int _getGesturesRegistrationCount;

        public static int CurrentForwardWave => _forwardWave;

        public static void LogXrBindingsInit(bool applied, bool skippedAlreadyInitialized)
        {
            if (!Enabled)
                return;
            _xrBindingsInitAttempts++;
            var openXr = OpenXRHandInput.Instance;
            var rightListeners = openXr != null ? CountUnityEventListeners(openXr.RightStateChanged) : -1;
            Write(
                $"InitXrHandEventBindings attempt={_xrBindingsInitAttempts} applied={applied} " +
                $"skippedAlreadyInit={skippedAlreadyInitialized} openXrRightStateListeners={rightListeners}");
        }

        public static void LogInitGestureRecognizers(OpenXRHandInput source, int callIndex)
        {
            if (!Enabled || source == null)
                return;
            Write(
                $"InitGestureRecognizers callIndex={callIndex} openXrInst={source.GetInstanceID()} " +
                $"go={source.gameObject.name}");
        }

        public static void NoteGetGesturesRegistration(MonoBehaviour host, bool includesRotationCallback)
        {
            if (!Enabled || host == null)
                return;
            _getGesturesRegistrationCount++;
            Write(
                $"GetGestures register #{_getGesturesRegistrationCount} host={host.GetType().Name} " +
                $"inst={host.GetInstanceID()} go={host.gameObject.name} rotationToo={includesRotationCallback}");
        }

        public static void LogListenerSnapshotAfterWidgetReveal()
        {
            if (!Enabled)
                return;

            var input = SpatialGestureInput.Instance;
            var onStateListeners = input != null ? CountUnityEventListeners(input.OnStateChanged) : -1;
            var openXr = OpenXRHandInput.Instance;
            var openXrRight = openXr != null ? CountUnityEventListeners(openXr.RightStateChanged) : -1;

            var cubeControllers = Object.FindObjectsOfType<TwoZoneCubeController>(true);
            var openXrComponents = Object.FindObjectsOfType<OpenXRHandInput>(true);
            var rightCubes = 0;
            var leftCubes = 0;
            foreach (var cube in cubeControllers)
            {
                if (cube == null)
                    continue;
                if (cube.IsRight)
                    rightCubes++;
                else
                    leftCubes++;
            }

            Write(
                $"snapshot after widget reveal OnStateChangedListeners={onStateListeners} " +
                $"openXrRightStateListeners={openXrRight} openXrHandInputComponents={openXrComponents.Length} " +
                $"twoZoneCubeControllers={cubeControllers.Length} rightHandCubes={rightCubes} " +
                $"leftHandCubes={leftCubes} xrBindingsAttempts={_xrBindingsInitAttempts}");
        }

        public static int BeginOpenXrForward(bool right, HandGestureState current, HandGestureState previous)
        {
            if (!Enabled)
                return 0;

            _forwardWave++;
            _deliveryIndexInWave = 0;

            Write(
                $"forward #{_forwardWave} right={right} {previous}->{current} frame={Time.frameCount}");
            return _forwardWave;
        }

        public static int LogCubeOnStateChanged(
            TwoZoneCubeController cube,
            bool right,
            HandGestureState current,
            HandGestureState previous,
            bool reachedInsertPath)
        {
            if (!Enabled || cube == null)
                return 0;

            _deliveryIndexInWave++;
            Write(
                $"cube handler wave={_forwardWave} delivery={_deliveryIndexInWave} " +
                $"right={right} cubeIsRight={cube.IsRight} inst={cube.GetInstanceID()} go={cube.gameObject.name} " +
                $"{previous}->{current} insertPath={reachedInsertPath} frame={Time.frameCount}");
            return _deliveryIndexInWave;
        }

        public static void LogInsertNewLetter(TwoZoneCubeController cube, bool right, string letter)
        {
            if (!Enabled)
                return;
            Write(
                $"InsertNewLetter wave={_forwardWave} delivery={_deliveryIndexInWave} " +
                $"inst={cube?.GetInstanceID()} letter={letter} frame={Time.frameCount}");
        }

        public static void LogInsertNewLetterDeduped(TwoZoneCubeController cube, bool right, string letter)
        {
            if (!Enabled)
                return;
            Write(
                $"InsertNewLetter SKIPPED dedupe inst={cube?.GetInstanceID()} right={right} letter={letter} " +
                $"frame={Time.frameCount}");
        }

        public static int CountUnityEventListeners(UnityEventBase unityEvent)
        {
            if (unityEvent == null)
                return -1;

            var callsField = typeof(UnityEventBase).GetField(
                "m_Calls",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (callsField == null)
                return -1;

            var invokeCallList = callsField.GetValue(unityEvent);
            if (invokeCallList == null)
                return 0;

            var countProperty = invokeCallList.GetType().GetProperty(
                "Count",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (countProperty != null)
                return (int)countProperty.GetValue(invokeCallList);

            return -1;
        }

        static void Write(string message)
        {
            var line = $"{Prefix} {message}";
            Debug.Log(line);
#if !UNITY_STANDALONE && UNITY_ANDROID
            QuestHandLog.Info(line);
#endif
        }
    }
}
