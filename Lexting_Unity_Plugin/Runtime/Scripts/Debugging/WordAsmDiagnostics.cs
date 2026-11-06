using System;
using SpatialGestureText.Input;
using SpatialGestureText.Tutorial;
using SpatialGestureText.TwoZoneCube;
using SpatialGestureText.Utilities;
using UnityEngine;

namespace SpatialGestureText.Debugging
{
    /// <summary>
    /// QA instrumentation for word assembly vs printer output (grab comp vs displayed text).
    /// Set <see cref="Enabled"/> to false after investigation. Grep logcat: WordAsm
    /// </summary>
    public static class WordAsmDiagnostics
    {
        public const bool Enabled = true;

        const string Prefix = "[WordAsm]";

        public static string GrabComp
        {
            get
            {
                try
                {
                    return TwoZoneGrabState.comp ?? "";
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }

        public static string PrinterOutput =>
            Lexting.printer != null ? Lexting.printer.CurrentOutputValue ?? "" : "<no-printer>";

        public static string GrabDetail()
        {
            return
                $"comp=\"{GrabComp}\" L=({TwoZoneGrabState.first(false)}+{TwoZoneGrabState.second(false)}) " +
                $"R=({TwoZoneGrabState.first(true)}+{TwoZoneGrabState.second(true)}) " +
                $"retL={TwoZoneGrabState.returned(false)} retR={TwoZoneGrabState.returned(true)} " +
                $"returnedThisFrame={TwoZoneGrabState.returnedThisFrame}";
        }

        public static void Log(string tag, string detail = null)
        {
            if (!Enabled)
                return;
            if (MuteHighFrequencyDuringSingleLetterSteps(tag))
                return;

            var assembler = Lexting.assembler;
            var finalizer = Lexting.finalizer;
            var line =
                $"{tag} frame={Time.frameCount} {GrabDetail()} " +
                $"printer=\"{PrinterOutput}\" " +
                $"asmActive={assembler != null && assembler.isWordAssemblyActive} " +
                $"scrollActive={assembler != null && assembler.isWordScrollActive} " +
                $"listActive={finalizer != null && finalizer.isSpatialListActive} " +
                $"lhTrace={Lexting.lhLine?.isLineTracingActive} rhTrace={Lexting.rhLine?.isLineTracingActive} " +
                $"lh={Lexting.lhState} rh={Lexting.rhState}" +
                (string.IsNullOrEmpty(detail) ? "" : $" | {detail}");

            Write(line);
        }

        public static void LogListHead(string tag, string listDefault0)
        {
            if (!Enabled)
                return;
            Log(tag, $"finalizerWordList[0]=\"{listDefault0 ?? ""}\"");
        }

        static bool MuteHighFrequencyDuringSingleLetterSteps(string tag)
        {
            var tm = TutorialManager.Instance;
            if (tm == null || !tm.IsSingleLetterTutorialTypingStep)
                return false;

            return tag.StartsWith("Grab")
                   || tag.StartsWith("AddLetter")
                   || tag.StartsWith("SubtractLetter")
                   || tag.StartsWith("RefreshInProgressWordOutput")
                   || tag.StartsWith("RefreshSingleHand")
                   || tag.StartsWith("RefreshHandReturnToStart")
                   || tag.StartsWith("RefreshTwoHandComp")
                   || tag == "ConstructWordList"
                   || tag == "DragResetStateToIdle";
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
