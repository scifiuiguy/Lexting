using Pixelplacement;
using Pixelplacement.TweenSystem;
using SpatialGestureText.EnglishDictionaries;
using SpatialGestureText.Input;
using SpatialGestureText.Interfaces;
using SpatialGestureText.LetterCube;
using SpatialGestureText.TwoZoneCube;
using SpatialGestureText.Utilities;
using SpatialGestureText.Zones;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SpatialGestureText.Tutorial
{
    public class TutorialManager : Singleton<TutorialManager>
    {
        public const string PlayerPrefNeverShowKey = "Lexting_InAppTutorial_NeverShow";

        [Range(0, 1)]
        [SerializeField] public float tutorialOpacity = 0.4f;

        [SerializeField] private float stepFadeDuration = 0.35f;

        // Obsolete pop-gun flag; kept for YAML / Android player script layout compatibility.
        [SerializeField] [HideInInspector] private bool autoAdvanceTutorialPages;

        public bool tutorialCompleted;

        [FormerlySerializedAs("currentPage")]
        public int currentStep;

        /// <summary>Steps 3–7 (0-based): single-letter typing on the live widget; word assembly must stay off.</summary>
        public bool IsSingleLetterTutorialTypingStep =>
            !tutorialCompleted && currentStep >= 3 && currentStep <= 7;

        [Tooltip("World-space gap below the two-zone widget bottom edge for Skip / Don't-show controls.")]
        [SerializeField] private float skipControlsMarginBelowWidget = 0.1f;

        [FormerlySerializedAs("skipControlsRoot")]
        [SerializeField] [HideInInspector] private GameObject legacySkipControlsRoot;

        [Tooltip("Optional Text used for red error flash during typing steps. If empty, uses LextingOutputManager registered InputField/displays.")]
        [SerializeField] private Text outputFlashText;

        public SpatialGestureEvent OnTutorialCompleted = new SpatialGestureEvent();

        private SpatialGestureInput _input;
        private CanvasGroup canvasGroup;
        private TutorialHandHandler _leftHand;
        private TutorialHandHandler _rightHand;
        private TutorialCheckMarkHandler _leftCheckmark;
        private TutorialCheckMarkHandler _rightCheckmark;

        private Coroutine _stepCoroutine;
        private string _stepStartOutput = "";
        private bool _widgetRevealed;
        private bool _handsHidden;
        private int _tantalizeBlockStartStep = 11;
        private bool _outputWatchActive;
        private string _lastSeenOutput = "";
        private bool _step6TheValidated;
        private bool _typingInsertLocked;
        private Coroutine _flashOutputCoroutine;

        private TutorialPageHandler pageHandler;
        private TweenBase _masterFadeTween;
        private GameObject _skipControlsRoot;
        private TutorialSkipControlsLayout _skipControlsLayout;
        private bool _tutorialUiBegun;

        private void Awake()
        {
            _input = SpatialGestureInput.Instance;
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;

            pageHandler = GetComponentInChildren<TutorialPageHandler>(true);
            if (pageHandler != null)
                pageHandler.BindToExistingHierarchy();
            else
                Debug.LogWarning($"{nameof(TutorialManager)}: no {nameof(TutorialPageHandler)} under Pages.", this);

            this.GetGestures(OnGestureChanged);

            if (PlayerPrefs.GetInt(PlayerPrefNeverShowKey, 0) != 0)
            {
                tutorialCompleted = true;
                this.WaitS(0.1f, () =>
                {
                    DismissImmediate();
                    OnTutorialCompleted.I();
                });
                return;
            }

            this.WaitS(1, () =>
            {
                if (LextingController.Instance.isInputStreamReceiver)
                {
                    tutorialCompleted = true;
                    DismissImmediate();
                    OnTutorialCompleted.I();
                }
            });

            if (Lexting.controllers != null)
                Lexting.controllers.gameObject.SetActive(false);

            EnsureSkipControlsRoot();
        }

        private void Start()
        {
            if (tutorialCompleted)
                return;

            this.Wait(CanBeginTutorialUi, BeginTutorialUi);
            this.WaitS(5f, () =>
            {
                if (!tutorialCompleted && !_tutorialUiBegun)
                    BeginTutorialUi();
            });
        }

        private static bool CanBeginTutorialUi()
        {
            return ZoneTextingDemoCanvas.Instance != null
                   && Lexting.inputManager != null
                   && Lexting.inputManager.hasHead;
        }

        private void BeginTutorialUi()
        {
            if (_tutorialUiBegun || tutorialCompleted)
                return;
            _tutorialUiBegun = true;

            ZoneTextingDemoCanvas.Instance.EnsureInitialWorldDepth();
            ResolveTutorialChrome();
            EnterStep(0);
            Introduce();
        }

        private void Update()
        {
            if (tutorialCompleted || !_outputWatchActive)
                return;

            var output = Lexting.printer.CurrentOutputValue ?? "";
            if (output == _lastSeenOutput)
                return;

            _lastSeenOutput = output;
            HandleOutputChanged(output);
        }

        private void OnGestureChanged(bool right, HandGestureState current, HandGestureState previous)
        {
            if (tutorialCompleted)
                return;

            if (IsSingleLetterTutorialTypingStep
                && previous == HandGestureState.Insert
                && current != HandGestureState.Insert)
            {
                var output = Lexting.printer.CurrentOutputValue ?? "";
                if (output == _stepStartOutput)
                    _typingInsertLocked = false;
            }

            if (currentStep == 1 && !right && current == HandGestureState.Insert && previous != HandGestureState.Insert)
                OnLeftGrabMatched();
            else if (currentStep == 2 && right && current == HandGestureState.Insert && previous != HandGestureState.Insert)
                OnRightGrabMatched();
        }

        #region Step flow

        private void EnterStep(int step)
        {
            currentStep = step;
            StopStepCoroutine();
            if (step == 5)
                _step6TheValidated = false;
            if (step == 10 || step == 11)
                CaptureStepStartOutput(Lexting.printer.CurrentOutputValue ?? "");
            if (step == 15 && Lexting.finalizer.isSpatialListActive)
                Lexting.assembler.RebuildWordListFromGrabComp();
            _outputWatchActive = step >= 3 && step <= 7;

            if (step >= 3 && step <= 7)
            {
                string revert = RevertOutputForStep(step);
                Lexting.printer.UpdateOutputs(revert);
                CaptureStepStartOutput(revert);
                _typingInsertLocked = false;
                if (Lexting.assembler.isWordAssemblyActive)
                    Lexting.assembler.DeactivateWordAssembler();
            }
            else
                _lastSeenOutput = Lexting.printer.CurrentOutputValue ?? "";

            ApplyStepPresentation(step);
            _stepCoroutine = StartCoroutine(RunStepRoutine(step));
        }

        /// <summary>Master fade on TutorialGroup CanvasGroup: off → swap copy → on at <see cref="tutorialOpacity"/>.</summary>
        private void ApplyStepPresentation(int step)
        {
            if (step == 0 && canvasGroup.alpha <= 0.01f)
            {
                pageHandler?.ApplyStepCopy(step);
                StopMasterFadeTween();
                TweenMasterAlpha(tutorialOpacity, Mathf.Max(stepFadeDuration, 0.35f));
                return;
            }

            StopMasterFadeTween();

            TweenMasterAlpha(0f, stepFadeDuration, () =>
            {
                pageHandler?.ApplyStepCopy(step);
                TweenMasterAlpha(tutorialOpacity, stepFadeDuration);
            });
        }

        private void TweenMasterAlpha(float target, float duration, UnityAction onComplete = null)
        {
            StopMasterFadeTween();
            _masterFadeTween = Tween.CanvasGroupAlpha(canvasGroup, target, duration, 0, Tween.EaseInOut, Tween.LoopType.None,
                null, onComplete != null ? () => onComplete.I() : null);
        }

        private void StopMasterFadeTween()
        {
            _masterFadeTween?.Stop();
            _masterFadeTween = null;
        }

        private IEnumerator RunStepRoutine(int step)
        {
            switch (step)
            {
                case 0:
                    yield return RunPalmGateStep();
                    yield return PlayBothCheckmarksPulse();
                    if (currentStep == 0)
                        AdvanceStep();
                    break;
                case 1:
                    _leftHand?.SetOpenImmediate();
                    _rightHand?.SetOpenImmediate();
                    yield return new WaitForSeconds(2f);
                    _leftHand?.AnimateToClosed();
                    yield return WaitForLeftGrab();
                    break;
                case 2:
                    yield return new WaitForSeconds(2f);
                    _rightHand?.AnimateToClosed();
                    yield return WaitForRightGrab();
                    break;
                case 3:
                case 4:
                case 6:
                case 7:
                    while (currentStep == step)
                        yield return null;
                    break;
                case 5:
                    while (currentStep == 5 && !_step6TheValidated)
                        yield return null;
                    if (currentStep == 5)
                        yield return RunStep6ScriptedSequence();
                    if (currentStep == 5)
                        AdvanceStep();
                    break;
                case 8:
                    yield return new WaitForSeconds(7f);
                    AdvanceStep();
                    break;
                case 9:
                    yield return RunWordModeTheStart();
                    break;
                case 10:
                    yield return RunWordModeTheAutofill();
                    break;
                case 11:
                case 12:
                case 13:
                    yield return RunTantalizeAssemblyStep(step);
                    break;
                case 14:
                    yield return RunFinalizationEntryStep();
                    break;
                case 15:
                    yield return RunFinalizationCommitStep();
                    break;
                case 16:
                    yield return new WaitForSeconds(7f);
                    CompleteTutorial();
                    break;
            }
        }

        private IEnumerator RunPalmGateStep()
        {
            float held = 0f;
            const float required = 2f;
            const float palmDotThreshold = -0.7f;

            while (true)
            {
                if (!PalmsFacingEachOther(palmDotThreshold))
                {
                    held = 0f;
                    yield return null;
                    continue;
                }

                held += Time.deltaTime;
                if (held >= required)
                    yield break;
                yield return null;
            }
        }

        private bool PalmsFacingEachOther(float dotThreshold)
        {
            if (!Lexting.input.leftHand.palm.Ready() || !Lexting.input.rightHand.palm.Ready())
                return false;

            var nL = QuestHandPalmNormal.Compute(false, Lexting.input.leftHand, QuestHandPalmNormal.DefaultSource);
            var nR = QuestHandPalmNormal.Compute(true, Lexting.input.rightHand, QuestHandPalmNormal.DefaultSource);
            if (nL.sqrMagnitude < 1e-6f || nR.sqrMagnitude < 1e-6f)
                return false;

            return Vector3.Dot(nL.normalized, nR.normalized) <= dotThreshold;
        }

        private IEnumerator WaitForLeftGrab()
        {
            var previousLeft = Lexting.lhState;
            while (currentStep == 1)
            {
                var currentLeft = Lexting.lhState;
                if (currentLeft == HandGestureState.Insert && previousLeft != HandGestureState.Insert)
                    OnLeftGrabMatched();
                previousLeft = currentLeft;
                yield return null;
            }
        }

        private IEnumerator WaitForRightGrab()
        {
            while (currentStep == 2)
                yield return null;
        }

        private void OnLeftGrabMatched()
        {
            if (currentStep != 1)
                return;
            Lexting.audio?.Play("Success");
            _leftCheckmark?.DrawCheckmark(true);
            AdvanceStep();
        }

        private void OnRightGrabMatched()
        {
            if (currentStep != 2)
                return;
            Lexting.audio.Play("Success");
            _rightCheckmark?.DrawCheckmark(true);
            RevealWidgetAndOutput();
            FadeHandsAndCheckmarks();
            AdvanceStep();
        }

        private IEnumerator RunStep6ScriptedSequence()
        {
            yield return new WaitForSeconds(2f);
            Lexting.printer.UpdateOutputs("");
            yield return new WaitForSeconds(1f);
            Lexting.printer.UpdateOutputs("fo");
            _stepStartOutput = "fo";
            _lastSeenOutput = "fo";
        }

        private IEnumerator RunWordModeTheStart()
        {
            while (currentStep == 9)
            {
                if (WordModeHoldMatches("th"))
                    AdvanceStep();
                yield return null;
            }
        }

        private IEnumerator RunWordModeTheAutofill()
        {
            var release = new WordModeDualReleaseTracker();
            while (currentStep == 10)
            {
                var leftInsert = Lexting.lhState == HandGestureState.Insert;
                var rightInsert = Lexting.rhState == HandGestureState.Insert;
                release.NoteBothHeld(leftInsert, rightInsert);

                if (release.TryConsumeBothReleased(leftInsert, rightInsert))
                {
                    yield return WaitForTutorialWordCommit("the", 3f);
                    var output = Lexting.printer.CurrentOutputValue ?? "";
                    if (TutorialOutputHasCommittedWord(output, "the"))
                        AdvanceStep();
                    else
                    {
                        Debug.Log(
                            $"[Tutorial] WM autofill step failed validation output='{output}' " +
                            $"asmActive={Lexting.assembler.isWordAssemblyActive} listActive={Lexting.finalizer.isSpatialListActive}");
                        FlashOutputErrorAndRevert(_stepStartOutput);
                    }
                }

                yield return null;
            }
        }

        private IEnumerator RunTantalizeAssemblyStep(int step)
        {
            var release = new WordModeDualReleaseTracker();
            while (currentStep == step)
            {
                var leftInsert = Lexting.lhState == HandGestureState.Insert;
                var rightInsert = Lexting.rhState == HandGestureState.Insert;
                release.NoteBothHeld(leftInsert, rightInsert);

                if (release.TryConsumeBothReleased(leftInsert, rightInsert,
                        () => !Lexting.assembler.isWordAssemblyActive))
                {
                    FlashOutputErrorAndRevert(_stepStartOutput);
                    EnterStep(_tantalizeBlockStartStep);
                    yield break;
                }

                if (TantalizeAssemblyStepComplete(step))
                    AdvanceStep();

                yield return null;
            }
        }

        private IEnumerator RunFinalizationEntryStep()
        {
            var release = new WordModeDualReleaseTracker();
            var listRefreshedForStem = false;
            while (currentStep == 14)
            {
                var leftInsert = Lexting.lhState == HandGestureState.Insert;
                var rightInsert = Lexting.rhState == HandGestureState.Insert;
                release.NoteBothHeld(leftInsert, rightInsert);

                if (release.TryConsumeBothReleased(leftInsert, rightInsert,
                        () => !Lexting.assembler.isWordAssemblyActive))
                {
                    FlashOutputErrorAndRevert(_stepStartOutput);
                    EnterStep(_tantalizeBlockStartStep);
                    yield break;
                }

                if (!listRefreshedForStem && Lexting.finalizer.isSpatialListActive &&
                    TantalizeCompHasFinalizationStem())
                {
                    Lexting.assembler.RebuildWordListFromGrabComp();
                    listRefreshedForStem = true;
                }

                if (Lexting.finalizer.isSpatialListActive && rightInsert && !leftInsert &&
                    TantalizeCompHasFinalizationStem())
                    AdvanceStep();

                yield return null;
            }
        }

        private IEnumerator RunFinalizationCommitStep()
        {
            CaptureStepStartOutput(Lexting.printer.CurrentOutputValue ?? "");
            var release = new WordModeDualReleaseTracker();
            while (currentStep == 15)
            {
                var leftInsert = Lexting.lhState == HandGestureState.Insert;
                var rightInsert = Lexting.rhState == HandGestureState.Insert;
                release.NoteBothHeld(leftInsert, rightInsert);

                if (release.TryConsumeBothReleased(leftInsert, rightInsert))
                {
                    var output = Lexting.printer.CurrentOutputValue ?? "";
                    if (IsAcceptedTantalizeCommit(output))
                        AdvanceStep();
                    else if (!string.IsNullOrEmpty(output))
                    {
                        FlashOutputErrorAndRevert(_stepStartOutput);
                        if (Lexting.assembler.isWordAssemblyActive || Lexting.finalizer.isSpatialListActive)
                            Lexting.assembler.DeactivateWordAssembler();
                        EnterStep(_tantalizeBlockStartStep);
                        yield break;
                    }
                }

                // Finalization commits via HandleFullWordSubmission (often single-hand release); dual-hold is not required.
                var committedOutput = Lexting.printer.CurrentOutputValue ?? "";
                if (!leftInsert && !rightInsert
                    && FinalizationCommitOutputReady(committedOutput, _stepStartOutput))
                {
                    AdvanceStep();
                    yield break;
                }

                yield return null;
            }
        }

        /// <summary>
        /// Word-mode append often ends with trailing space/punctuation (e.g. "the "), so EndsWith("the") alone fails.
        /// </summary>
        private static bool TutorialOutputHasCommittedWord(string output, string word)
        {
            if (string.IsNullOrWhiteSpace(output) || string.IsNullOrEmpty(word))
                return false;

            var trimmed = output.Trim();
            if (trimmed.Equals(word, System.StringComparison.OrdinalIgnoreCase))
                return true;

            var trailingWord = Regex.Replace(trimmed, @"[\s\p{P}]+$", "");
            if (trailingWord.EndsWith(word, System.StringComparison.OrdinalIgnoreCase))
                return true;

            foreach (var token in Regex.Split(trimmed, @"\s+"))
            {
                var lettersOnly = Regex.Replace(token, @"[^\w]", "");
                if (lettersOnly.Equals(word, System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static IEnumerator WaitForTutorialWordCommit(string word, float timeoutSeconds)
        {
            var elapsed = 0f;
            while (elapsed < timeoutSeconds)
            {
                var output = Lexting.printer.CurrentOutputValue ?? "";
                if (TutorialOutputHasCommittedWord(output, word))
                    yield break;

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        /// <summary>One release edge per dual-hold cycle (avoids per-frame Flash/Advance while hands stay neutral).</summary>
        private struct WordModeDualReleaseTracker
        {
            bool _sawBothHeld;

            public void NoteBothHeld(bool leftInsert, bool rightInsert)
            {
                if (leftInsert && rightInsert)
                    _sawBothHeld = true;
            }

            public bool TryConsumeBothReleased(bool leftInsert, bool rightInsert, System.Func<bool> additionalGate = null)
            {
                if (!_sawBothHeld || leftInsert || rightInsert)
                    return false;
                if (additionalGate != null && !additionalGate())
                    return false;
                _sawBothHeld = false;
                return true;
            }
        }

        private static string TantalizeCompTarget(int step)
        {
            switch (step)
            {
                case 11: return "ta";
                case 12: return "tan";
                case 13: return "tant";
                default: return "";
            }
        }

        private const string TantalizeFinalizationStem = "tant";

        private static bool TantalizeCompHasFinalizationStem()
        {
            var comp = TwoZoneGrabState.comp ?? "";
            return comp.Length >= TantalizeFinalizationStem.Length
                   && comp.StartsWith(TantalizeFinalizationStem, System.StringComparison.OrdinalIgnoreCase);
        }

        private static bool TantalizeRightHandSecondIsT()
        {
            var second = TwoZoneGrabState.second(true);
            return !string.IsNullOrEmpty(second)
                   && second.Equals("t", System.StringComparison.OrdinalIgnoreCase);
        }

        private static bool TantalizeAssemblyStepComplete(int step)
        {
            if (step == 14)
                return TantalizeCompHasFinalizationStem() && TantalizeRightHandSecondIsT();

            var target = TantalizeCompTarget(step);
            return !string.IsNullOrEmpty(target)
                   && (TwoZoneGrabState.comp ?? "").StartsWith(target, System.StringComparison.OrdinalIgnoreCase);
        }

        private static bool WordModeHoldMatches(string prefix)
        {
            return Lexting.assembler.isWordAssemblyActive &&
                   (TwoZoneGrabState.comp.Equals(prefix) ||
                    (TwoZoneGrabState.first(false).ToLower() == prefix.Substring(0, 1) &&
                     TwoZoneGrabState.first(true).ToLower() == prefix.Substring(1, 1) &&
                     Lexting.lhState == HandGestureState.Insert && Lexting.rhState == HandGestureState.Insert));
        }

        private void HandleOutputChanged(string output)
        {
            switch (currentStep)
            {
                case 3:
                    if (output != "t")
                        OnTypingErrorForCurrentStep();
                    else
                        AdvanceStep();
                    break;
                case 4:
                    if (output != "th")
                        OnTypingErrorForCurrentStep();
                    else
                        AdvanceStep();
                    break;
                case 5:
                    if (output == "the")
                        _step6TheValidated = true;
                    else if (output != "th" && output.Length > 2)
                        OnTypingErrorForCurrentStep();
                    break;
                case 6:
                    if (output == "fox")
                        AdvanceStep();
                    else if (output != "fo")
                        OnTypingErrorForCurrentStep();
                    break;
                case 7:
                    if (output == "fox")
                        return;
                    if (string.IsNullOrEmpty(output))
                        AdvanceStep();
                    else
                        OnTypingErrorForCurrentStep();
                    break;
            }
        }

        private void OnTypingError(string revertTo)
        {
            FlashOutputErrorAndRevert(revertTo);
        }

        private void OnTypingErrorForCurrentStep()
        {
            FlashOutputErrorAndRevert(_stepStartOutput);
        }

        private void AdvanceStep()
        {
            if (tutorialCompleted)
                return;

            if (currentStep + 1 >= TutorialStepContent.StepCount)
            {
                CompleteTutorial();
                return;
            }

            EnterStep(currentStep + 1);
        }

        #endregion

        #region Widget / chrome

        private void RevealWidgetAndOutput()
        {
            if (_widgetRevealed)
                return;
            _widgetRevealed = true;
            _typingInsertLocked = false;

            if (Lexting.assembler != null
                && (Lexting.assembler.isWordAssemblyActive || Lexting.assembler.isWordScrollActive))
                Lexting.assembler.DeactivateWordAssembler();
            Lexting.lhLine?.ResetStateToIdle(false);
            Lexting.rhLine?.ResetStateToIdle(false);

            if (Lexting.controllers != null)
            {
                Lexting.controllers.gameObject.SetActive(true);
                if (Lexting.controllers is TwoZoneCubeControllers twoZone && twoZone.positionController != null)
                    twoZone.positionController.Calibrate();
                ShowSkipControls();
                Lexting.controllers.ControllerActivated?.I(true);
            }
            else
                ShowSkipControls();
        }

        private void FadeHandsAndCheckmarks()
        {
            if (_handsHidden)
                return;
            _handsHidden = true;
            _leftHand?.Fade(false);
            _rightHand?.Fade(false);
            _leftCheckmark?.DrawCheckmark(false);
            _rightCheckmark?.DrawCheckmark(false);
        }

        #endregion

        #region Output helpers

        private static string RevertOutputForStep(int step)
        {
            switch (step)
            {
                case 3: return "";
                case 4: return "t";
                case 5: return "th";
                case 6: return "fo";
                case 7: return "fox";
                default: return "";
            }
        }

        private void CaptureStepStartOutput(string value)
        {
            _stepStartOutput = value;
            _lastSeenOutput = value;
        }

        private void FlashOutputErrorAndRevert(string revertTo)
        {
            if (_flashOutputCoroutine != null)
                return;
            _flashOutputCoroutine = StartCoroutine(FlashOutputErrorRoutine(revertTo));
        }

        private IEnumerator FlashOutputErrorRoutine(string revertTo)
        {
            Lexting.audio.Play("SelectionClick");
            var texts = GetOutputTexts();
            var originals = texts.Select(t => t.color).ToList();
            var flash = new Color(0.85f, 0.2f, 0.2f, 1f);
            foreach (var t in texts)
                t.color = flash;
            yield return new WaitForSeconds(0.25f);
            Lexting.printer.UpdateOutputs(revertTo);
            _lastSeenOutput = revertTo;
            if (IsSingleLetterTutorialTypingStep)
                _typingInsertLocked = false;
            for (int i = 0; i < texts.Count; i++)
                texts[i].color = originals[i];
            _flashOutputCoroutine = null;
        }

        private List<Text> GetOutputTexts()
        {
            var list = new List<Text>();
            if (outputFlashText != null)
                list.Add(outputFlashText);
            foreach (var d in Lexting.printer.displays)
                if (d != null)
                    list.Add(d);
            foreach (var f in Lexting.printer.fields)
            {
                if (f != null && f.textComponent != null)
                    list.Add(f.textComponent);
            }
            return list;
        }

        private static bool FinalizationCommitOutputReady(string output, string stepStartOutput)
        {
            if (!IsAcceptedTantalizeCommit(output))
                return false;
            return !string.Equals((output ?? "").Trim(), (stepStartOutput ?? "").Trim(),
                System.StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAcceptedTantalizeCommit(string output)
        {
            if (string.IsNullOrWhiteSpace(output))
                return false;

            foreach (var rawToken in Regex.Split(output.Trim(), @"\s+"))
            {
                var token = Regex.Replace(rawToken, @"[^\w]", "").ToLowerInvariant();
                if (token.Length < 4 || !token.StartsWith("tantaliz", System.StringComparison.Ordinal))
                    continue;
                if (TantalizeLemmaTokenAccepted(token))
                    return true;
            }

            return false;
        }

        private static bool TantalizeLemmaTokenAccepted(string token)
        {
            var probe = new List<JSONNode>();
            LextingDictionary.DrillDownGet(ref probe, token.Substring(0, System.Math.Min(4, token.Length)),
                LextingDictionary.longerWordDictionary, 32);
            if (probe.Any(n => n["default"].Value.Equals(token, System.StringComparison.OrdinalIgnoreCase)))
                return true;

            return token.Length >= 9 && (token == "tantalize" || token.StartsWith("tantaliz", System.StringComparison.Ordinal));
        }

        #endregion

        #region Public dismiss API

        public void EndTutorialFromSkip(bool dontShowAgain)
        {
            if (dontShowAgain)
                PlayerPrefs.SetInt(PlayerPrefNeverShowKey, 1);
            PlayerPrefs.Save();
            CompleteTutorial();
        }

        public void CompleteTutorial()
        {
            if (tutorialCompleted)
                return;

            tutorialCompleted = true;
            StopStepCoroutine();
            _outputWatchActive = false;
            OnTutorialCompleted.I();
            Dismiss(() => { });
            HideSkipControls();
            if (HighlightableZoneController._.Instance != null)
                HighlightableZoneController._.Instance.Introduce();
        }

        #endregion

        #region Intro / dismiss

        private void Introduce()
        {
            _leftHand?.Fade(true);
            _rightHand?.Fade(true);
            _leftHand?.SetOpenImmediate();
            _rightHand?.SetOpenImmediate();
        }

        private void Dismiss(UnityAction callback)
        {
            StopMasterFadeTween();
            TweenMasterAlpha(0f, 0.5f, () => callback.I());
            _leftHand?.Fade(false);
            _rightHand?.Fade(false);
        }

        private void DismissImmediate()
        {
            canvasGroup.alpha = 0;
            _leftHand?.Fade(false);
            _rightHand?.Fade(false);
            HideSkipControls();
        }

        private void StopStepCoroutine()
        {
            if (_stepCoroutine != null)
            {
                StopCoroutine(_stepCoroutine);
                _stepCoroutine = null;
            }
        }

        private IEnumerator PlayBothCheckmarksPulse()
        {
            _leftCheckmark?.DrawCheckmark(true);
            _rightCheckmark?.DrawCheckmark(true);
            // Full on stroke ≈ 0.3s; hold before off so lerp-off does not cut lerp-on.
            yield return new WaitForSeconds(0.55f);
            _leftCheckmark?.DrawCheckmark(false);
            _rightCheckmark?.DrawCheckmark(false);
            yield return new WaitForSeconds(0.35f);
        }

        /// <summary>Whether this typing step still accepts one cube insert (see <see cref="ConfirmTutorialTypingInsertCommitted"/>).</summary>
        public bool TryConsumeTutorialTypingInsert()
        {
            if (!IsSingleLetterTutorialTypingStep)
                return true;
            return !_typingInsertLocked;
        }

        /// <summary>Call only after a letter was actually appended for the current typing step.</summary>
        public void ConfirmTutorialTypingInsertCommitted()
        {
            if (IsSingleLetterTutorialTypingStep)
                _typingInsertLocked = true;
        }

        private void ResolveTutorialChrome()
        {
            _leftHand = null;
            _rightHand = null;
            _leftCheckmark = null;
            _rightCheckmark = null;

            foreach (var h in GetComponentsInChildren<TutorialHandHandler>(true))
            {
                if (TutorialHandSide.IsPoseReferenceName(h.name))
                    continue;
                h.CacheHandSideFromName();
                if (TutorialHandSide.IsRightSideName(h.name))
                    _rightHand = h;
                else if (TutorialHandSide.IsLeftSideName(h.name))
                    _leftHand = h;
            }

            var allMarks = GetComponentsInChildren<TutorialCheckMarkHandler>(true);
            _leftCheckmark = PickCheckmarkForHand(_leftHand, allMarks);
            _rightCheckmark = PickCheckmarkForHand(_rightHand, allMarks);

            if (_leftCheckmark == null || _rightCheckmark == null)
            {
                var eligible = System.Array.FindAll(allMarks, m => !TutorialHandSide.IsUnderPoseReferenceHierarchy(m.transform));
                System.Array.Sort(eligible, (a, b) => a.transform.localPosition.x.CompareTo(b.transform.localPosition.x));
                if (_leftCheckmark == null && eligible.Length >= 1)
                    _leftCheckmark = eligible[0];
                if (_rightCheckmark == null && eligible.Length >= 2)
                    _rightCheckmark = eligible[eligible.Length - 1];
            }

            AttachCheckmarkToDisplayHand(_leftCheckmark, _leftHand);
            AttachCheckmarkToDisplayHand(_rightCheckmark, _rightHand);
            DisableExtraCheckmarkHandlers(allMarks);

            if (_leftHand == null || _rightHand == null)
                Debug.LogWarning($"{nameof(TutorialManager)}: missing demo hand handler(s). left={_leftHand != null}, right={_rightHand != null}", this);
            if (_leftCheckmark == null || _rightCheckmark == null)
                Debug.LogWarning($"{nameof(TutorialManager)}: missing checkmark handler(s). left={_leftCheckmark != null}, right={_rightCheckmark != null}", this);

            _leftCheckmark?.EnsureRuntimeReady();
            _rightCheckmark?.EnsureRuntimeReady();
        }

        private static TutorialCheckMarkHandler PickCheckmarkForHand(TutorialHandHandler hand, TutorialCheckMarkHandler[] marks)
        {
            if (hand == null || marks == null || marks.Length == 0)
                return null;

            int wantSide = TutorialHandSide.IsRightSideName(hand.name) ? 1 : -1;
            TutorialCheckMarkHandler onPoseRefFallback = null;

            foreach (var mark in marks)
            {
                if (TutorialHandSide.SideFromTransform(mark.transform) != wantSide)
                    continue;

                if (!TutorialHandSide.IsUnderPoseReferenceHierarchy(mark.transform))
                    return mark;

                onPoseRefFallback = mark;
            }

            return onPoseRefFallback;
        }

        private static void AttachCheckmarkToDisplayHand(TutorialCheckMarkHandler mark, TutorialHandHandler hand)
        {
            if (mark == null || hand == null)
                return;

            var pose = hand.GetComponent<TutorialProceduralHandPose>();
            Transform attach = pose != null ? pose.PoseRoot : hand.transform;
            if (mark.transform.parent == attach)
                return;

            mark.transform.SetParent(attach, worldPositionStays: true);
        }

        private void DisableExtraCheckmarkHandlers(TutorialCheckMarkHandler[] marks)
        {
            if (marks == null)
                return;

            foreach (var mark in marks)
            {
                if (mark == null || mark == _leftCheckmark || mark == _rightCheckmark)
                    continue;
                mark.enabled = false;
            }
        }

        private void EnsureSkipControlsRoot()
        {
            if (_skipControlsRoot != null)
                return;

            if (Lexting.controllers == null)
                return;

            _skipControlsRoot = new GameObject("TutorialSkipControls");
            _skipControlsRoot.transform.SetParent(Lexting.controllers.transform, false);
            _skipControlsLayout = _skipControlsRoot.AddComponent<TutorialSkipControlsLayout>();
            _skipControlsLayout.MarginBelowWidget = skipControlsMarginBelowWidget;

            AdoptSkipControlNodes(_skipControlsRoot.transform);

            if (legacySkipControlsRoot != null && legacySkipControlsRoot != _skipControlsRoot)
            {
                AdoptSkipControlNodes(_skipControlsRoot.transform, legacySkipControlsRoot.transform);
                if (Application.isPlaying)
                    Destroy(legacySkipControlsRoot);
                legacySkipControlsRoot = null;
            }

            _skipControlsRoot.SetActive(false);
        }

        private void AdoptSkipControlNodes(Transform destination, Transform optionalExtraRoot = null)
        {
            if (optionalExtraRoot != null)
            {
                for (var i = optionalExtraRoot.childCount - 1; i >= 0; i--)
                    optionalExtraRoot.GetChild(i).SetParent(destination, true);
            }

            var hooks = GetComponentsInChildren<TutorialDismissControls>(true);
            foreach (var hook in hooks)
            {
                if (hook.transform.IsChildOf(destination))
                    continue;
                hook.transform.SetParent(destination, true);
            }
        }

        private void ShowSkipControls()
        {
            EnsureSkipControlsRoot();
            if (_skipControlsRoot == null || _skipControlsRoot.transform.childCount == 0)
                return;

            if (_skipControlsLayout != null)
                _skipControlsLayout.MarginBelowWidget = skipControlsMarginBelowWidget;

            _skipControlsRoot.SetActive(true);
            _skipControlsLayout?.SyncToWidget();
        }

        private void HideSkipControls()
        {
            if (_skipControlsRoot != null)
                _skipControlsRoot.SetActive(false);
        }

        #endregion
    }
}
