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
        private const float TutorialSuccessSfxVolume = 0.5f;

        public const string PlayerPrefNeverShowKey = "Lexting_InAppTutorial_NeverShow";

        [Range(0, 1)]
        [SerializeField] public float tutorialOpacity = 0.4f;

        [SerializeField] private float stepFadeDuration = 0.35f;

        // Obsolete pop-gun flag; kept for YAML / Android player script layout compatibility.
        [SerializeField] [HideInInspector] private bool autoAdvanceTutorialPages;

        public bool tutorialCompleted;

        [FormerlySerializedAs("currentPage")]
        public int currentStep;

        private bool _playedSuccessSfxForSkipJump;

        /// <summary>
        /// Typing steps on the live widget (indices 3,5,7,10): single-letter inserts; word assembly must stay off.
        /// Excludes posture/timed indices 4,6,8,9 and Back-Word (11).
        /// </summary>
        public bool IsSingleLetterTutorialTypingStep =>
            !tutorialCompleted
            && (currentStep == 3 || currentStep == 5 || currentStep == 7 || currentStep == 10);

        [Tooltip("World-space meters below the TwoZoneCube origin for Skip (~0.12 m ≈ 4.7 in).")]
        [SerializeField] private float skipControlsMarginBelowWidget = 0.12f;

        [Tooltip("If scene-authored Don't-show-again nodes exist, keep them hidden when false (Skip only).")]
        [SerializeField] private bool includeDontShowAgainControl;

        [Tooltip("When no TutorialDismissControls exist under this hierarchy, spawn a default SKIP zone below the widget.")]
        [SerializeField] private bool provisionRuntimeSkipWhenMissing = true;

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
        /// <summary>Printer output at step 19 entry — baseline for conjugation fast-path vs bare lemma on screen.</summary>
        private string _outputSnapshotStep19 = "";
        private bool _widgetRevealed;
        private bool _handsHidden;
        private Coroutine _postRightGrabFadeCoroutine;
        private int _tantalizeBlockStartStep = 15;
        private const float ReadyPosePalmHoldSeconds = 1f;
        /// <summary>Step 16 (index 15): keep WM autofill output visible before clearing for tantalize.</summary>
        private const float WordModeCarryOverDisplaySeconds = 2f;
        private bool _outputWatchActive;
        private string _lastSeenOutput = "";
        private bool _step6TheValidated;
        private bool _step6ScriptRunning;
        private bool _typingInsertLocked;
        private Coroutine _flashOutputCoroutine;
        private List<Text> _flashOutputTexts;
        private List<Color> _flashOutputOriginalColors;

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
            CancelOutputFlash();
            currentStep = step;
            StopStepCoroutine();
            if (step == 7)
                _step6TheValidated = false;
            if (step == 13)
            {
                // Word Mode intro (step index 12) allows typing with no gating; start hold-T+H on an empty field.
                ClearWordModeLessonOutputAndGrabState();
            }
            else if (step == 14)
                CaptureStepStartOutput(Lexting.printer.CurrentOutputValue ?? "");
            else if (step == 15)
                CaptureStepStartOutput(Lexting.printer.CurrentOutputValue ?? "");
            else if (step == 16)
                CaptureStepStartOutput(Lexting.printer.CurrentOutputValue ?? "");
            else if (step == 19)
            {
                _outputSnapshotStep19 = Lexting.printer.CurrentOutputValue ?? "";
                CaptureStepStartOutput(_outputSnapshotStep19);
            }
            else if (step == 20)
                CaptureStepStartOutput(Lexting.printer.CurrentOutputValue ?? "");
            if (step == 21)
                HideSkipControls();
            if (step == 20 && Lexting.finalizer.isSpatialListActive)
                Lexting.assembler.RebuildWordListFromGrabComp();
            // Typing / posture / FOX block: flash + revert on unexpected output (incl. 4,6,8,9 — whitelist in HandleOutputChanged).
            _outputWatchActive = step >= 3 && step <= 11;

            if (step >= 3 && step <= 11)
            {
                // Posture / no-forced-revert (4, 6, 8, 9): keep printer as-is at step entry (8 runs scripted the→fo in routine).
                if (step == 4 || step == 6 || step == 8 || step == 9)
                {
                    _lastSeenOutput = Lexting.printer.CurrentOutputValue ?? "";
                    CaptureStepStartOutput(_lastSeenOutput);
                }
                else if (step == 10)
                {
                    _lastSeenOutput = Lexting.printer.CurrentOutputValue ?? "";
                    CaptureStepStartOutput(_lastSeenOutput);
                    _typingInsertLocked = false;
                }
                else
                {
                    string revert = RevertOutputForStep(step);
                    Lexting.printer.UpdateOutputs(revert);
                    CaptureStepStartOutput(revert);
                    _typingInsertLocked = false;
                }

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
                    while (currentStep == step)
                        yield return null;
                    break;
                case 4:
                    yield return WaitUntilEitherPalmRotation(CubeRotationState.supinate);
                    if (currentStep == 4)
                        AdvanceStep();
                    break;
                case 5:
                    while (currentStep == step)
                        yield return null;
                    break;
                case 6:
                    yield return WaitUntilEitherPalmRotation(CubeRotationState.pronate);
                    if (currentStep == 6)
                        AdvanceStep();
                    break;
                case 7:
                    while (currentStep == 7 && !_step6TheValidated)
                        yield return null;
                    if (currentStep == 7)
                        AdvanceStep();
                    break;
                case 8:
                    if (currentStep == 8)
                    {
                        yield return RunStep6ScriptedSequence();
                        _lastSeenOutput = Lexting.printer.CurrentOutputValue ?? "";
                    }

                    yield return new WaitForSeconds(7f);
                    if (currentStep == 8)
                        AdvanceStep();
                    break;
                case 9:
                    yield return WaitUntilEitherPalmRotation(CubeRotationState.forward);
                    if (currentStep == 9)
                        AdvanceStep();
                    break;
                case 10:
                    while (currentStep == step)
                        yield return null;
                    break;
                case 11:
                    while (currentStep == step)
                        yield return null;
                    break;
                case 12:
                    yield return new WaitForSeconds(7f);
                    AdvanceStep();
                    break;
                case 13:
                    yield return RunWordModeTheStart();
                    break;
                case 14:
                    yield return RunWordModeTheAutofill();
                    break;
                case 15:
                case 16:
                case 17:
                case 18:
                    yield return RunTantalizeAssemblyStep(step);
                    break;
                case 19:
                    yield return RunFinalizationEntryStep();
                    break;
                case 20:
                    yield return RunFinalizationCommitStep();
                    break;
                case 21:
                    yield return new WaitForSeconds(7f);
                    CompleteTutorial();
                    break;
            }
        }

        private IEnumerator RunPalmGateStep()
        {
            float held = 0f;
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
                if (held >= ReadyPosePalmHoldSeconds)
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

        /// <summary>
        /// Advance when either hand matches the live rotation classifier (same mapping as the two-zone cube).
        /// </summary>
        private static IEnumerator WaitUntilEitherPalmRotation(CubeRotationState required)
        {
            while (true)
            {
                if (Lexting.input != null)
                {
                    if (Lexting.input.currentLeftRotationState == required || Lexting.input.currentRightRotationState == required)
                        yield break;
                }

                yield return null;
            }
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
            Lexting.audio?.Play("Success", TutorialSuccessSfxVolume);
            _leftCheckmark?.DrawCheckmark(true);
            AdvanceStep();
        }

        private void OnRightGrabMatched()
        {
            if (currentStep != 2)
                return;
            Lexting.audio.Play("Success", TutorialSuccessSfxVolume);
            _rightCheckmark?.DrawCheckmark(true);
            RevealWidgetAndOutput();
            if (_postRightGrabFadeCoroutine != null)
                StopCoroutine(_postRightGrabFadeCoroutine);
            _postRightGrabFadeCoroutine = StartCoroutine(FadeHandsAndCheckmarksAfterRightCheckmark());
            AdvanceStep();
        }

        private IEnumerator FadeHandsAndCheckmarksAfterRightCheckmark()
        {
            yield return new WaitForSeconds(TutorialCheckMarkHandler.FullOnStrokeSeconds + 0.15f);
            _postRightGrabFadeCoroutine = null;
            FadeHandsAndCheckmarks();
        }

        private IEnumerator RunStep6ScriptedSequence()
        {
            _step6ScriptRunning = true;
            try
            {
                yield return new WaitForSeconds(2f);
                Lexting.printer.UpdateOutputs("");
                yield return new WaitForSeconds(1f);
                Lexting.printer.UpdateOutputs("fo");
                if (currentStep == 8)
                {
                    _stepStartOutput = "fo";
                    _lastSeenOutput = "fo";
                }
            }
            finally
            {
                _step6ScriptRunning = false;
            }
        }

        private IEnumerator RunWordModeTheStart()
        {
            var release = new WordModeDualReleaseTracker();
            while (currentStep == 13)
            {
                var leftInsert = Lexting.lhState == HandGestureState.Insert;
                var rightInsert = Lexting.rhState == HandGestureState.Insert;
                release.NoteBothHeld(leftInsert, rightInsert);

                if (WordModeHoldMatches("th"))
                    AdvanceStep();
                else if (release.TryConsumeBothReleased(leftInsert, rightInsert)
                         && Lexting.assembler.isWordAssemblyActive)
                {
                    FlashOutputErrorAndRevert(_stepStartOutput);
                    EnterStep(13);
                    yield break;
                }

                yield return null;
            }
        }

        private IEnumerator RunWordModeTheAutofill()
        {
            var release = new WordModeDualReleaseTracker();
            while (currentStep == 14)
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
                        EnterStep(13);
                    }
                }

                yield return null;
            }
        }

        private IEnumerator RunTantalizeAssemblyStep(int step)
        {
            if (step == 15)
            {
                // Carry committed output from WM autofill (doc step 15 / index 14); show ~2 s, then wipe before hold-T+A.
                yield return new WaitForSeconds(WordModeCarryOverDisplaySeconds);
                if (currentStep != 15)
                    yield break;
                ClearWordModeLessonOutputAndGrabState();
            }

            var release = new WordModeDualReleaseTracker();
            while (currentStep == step)
            {
                var leftInsert = Lexting.lhState == HandGestureState.Insert;
                var rightInsert = Lexting.rhState == HandGestureState.Insert;
                release.NoteBothHeld(leftInsert, rightInsert);

                if (release.TryConsumeBothReleased(leftInsert, rightInsert))
                {
                    if (!Lexting.assembler.isWordAssemblyActive
                        || !TantalizeGrabStateValidForStep(step))
                    {
                        FlashOutputErrorAndRevert(_stepStartOutput);
                        EnterStep(_tantalizeBlockStartStep);
                        yield break;
                    }
                }

                if (TantalizeAssemblyStepComplete(step))
                    AdvanceStep();

                yield return null;
            }
        }

        /// <summary>After both hands release, grab comp must match this step's expected tantalize stem.</summary>
        private static bool TantalizeGrabStateValidForStep(int step)
        {
            if (step == 18)
                return TantalizeCompHasFinalizationStem() && TantalizeRightHandSecondIsT();
            var target = TantalizeCompTarget(step);
            return !string.IsNullOrEmpty(target)
                   && (TwoZoneGrabState.comp ?? "").StartsWith(target, System.StringComparison.OrdinalIgnoreCase);
        }

        private IEnumerator RunFinalizationEntryStep()
        {
            var release = new WordModeDualReleaseTracker();
            var listRefreshedForStem = false;
            while (currentStep == 19)
            {
                var leftInsert = Lexting.lhState == HandGestureState.Insert;
                var rightInsert = Lexting.rhState == HandGestureState.Insert;
                release.NoteBothHeld(leftInsert, rightInsert);

                if (release.TryConsumeBothReleased(leftInsert, rightInsert))
                {
                    var output = Lexting.printer.CurrentOutputValue ?? "";
                    if (ShouldFastPathPastFinalizationCoaching(output))
                    {
                        EnterStep(21);
                        yield break;
                    }

                    if (!Lexting.assembler.isWordAssemblyActive)
                    {
                        FlashOutputErrorAndRevert(_stepStartOutput);
                        EnterStep(_tantalizeBlockStartStep);
                        yield break;
                    }
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
            var outputAtEntry = Lexting.printer.CurrentOutputValue ?? "";
            if (ShouldFastPathPastFinalizationCoaching(outputAtEntry))
            {
                EnterStep(21);
                yield break;
            }

            var release = new WordModeDualReleaseTracker();
            while (currentStep == 20)
            {
                var leftInsert = Lexting.lhState == HandGestureState.Insert;
                var rightInsert = Lexting.rhState == HandGestureState.Insert;
                release.NoteBothHeld(leftInsert, rightInsert);

                if (release.TryConsumeBothReleased(leftInsert, rightInsert))
                {
                    var output = Lexting.printer.CurrentOutputValue ?? "";
                    if (HasCompletedTantalizeFinalization(output))
                    {
                        // Conjugated commit — advance once; do not fall through to idle check same frame.
                        AdvanceStep();
                        yield break;
                    }
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
                if (!leftInsert && !rightInsert)
                {
                    if (FinalizationCommitOutputReady(committedOutput, _stepStartOutput))
                    {
                        AdvanceStep();
                        yield break;
                    }

                    // Handoff stuck fix: conjugated commit already on printer but equals step-20 stepStart (no output delta).
                    if (HasCompletedTantalizeFinalization(committedOutput))
                    {
                        AdvanceStep();
                        yield break;
                    }
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
                case 15: return "ta";
                case 16: return "tan";
                case 17: return "tant";
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
            if (step == 18)
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
            if (_step6ScriptRunning)
                return;

            // If the learner already completes "fox" during the FOX block (steps 8–10, indices 8–10),
            // skip palm-forward / type-x pages and land on Back-Word (11). No red flash for that path.
            if (TryEnterBackWordWhenFoxComplete(output))
                return;

            switch (currentStep)
            {
                case 3:
                    if (output != "t")
                        OnTypingErrorForCurrentStep();
                    else
                        AdvanceStep();
                    break;
                case 4:
                    if (output != "t")
                        OnTypingError("t");
                    break;
                case 5:
                    if (output != "th")
                        OnTypingErrorForCurrentStep();
                    else
                        AdvanceStep();
                    break;
                case 6:
                    if (output != "th")
                        OnTypingError("th");
                    break;
                case 7:
                    if (string.Equals(output, "the", System.StringComparison.OrdinalIgnoreCase))
                        _step6TheValidated = true;
                    else if (!string.Equals(output, "th", System.StringComparison.OrdinalIgnoreCase))
                        OnTypingError("th");
                    break;
                case 8:
                    // Scripted sequence: "the" (hold) → "" → "fo". Only enforce once script finished.
                    if (_step6ScriptRunning) return;
                    if (!string.Equals(output, "fo", System.StringComparison.OrdinalIgnoreCase))
                        OnTypingError("fo");
                    break;
                case 9:
                    if (!string.Equals(output, "fo", System.StringComparison.OrdinalIgnoreCase))
                        OnTypingError("fo");
                    break;
                case 10:
                    if (string.Equals(output, "fox", System.StringComparison.OrdinalIgnoreCase))
                        AdvanceStep();
                    else if (!string.Equals(output, "fo", System.StringComparison.OrdinalIgnoreCase))
                        OnTypingErrorForCurrentStep();
                    break;
                case 11:
                    if (string.Equals(output, "fox", System.StringComparison.OrdinalIgnoreCase))
                        return;
                    if (string.IsNullOrEmpty(output))
                        AdvanceStep();
                    else
                        OnTypingErrorForCurrentStep();
                    break;
            }
        }

        /// <summary>
        /// Completing <c>fox</c> during the FOX tutorial block (timed intro, palm-forward, or type-x)
        /// jumps straight to Back-Word; avoids treating the result as a validation error on earlier pages.
        /// </summary>
        private bool TryEnterBackWordWhenFoxComplete(string output)
        {
            if (!string.Equals(output, "fox", System.StringComparison.OrdinalIgnoreCase))
                return false;
            if (currentStep != 8 && currentStep != 9 && currentStep != 10)
                return false;
            EnterStep(11);
            return true;
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
                Lexting.controllers.ControllerActivated?.I(true);
                ShowSkipControls();
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
                case 5: return "t";
                case 7: return "th";
                case 8: return "the";
                case 10: return "fo";
                case 11: return "fox";
                default: return "";
            }
        }

        private void CaptureStepStartOutput(string value)
        {
            _stepStartOutput = value;
            _lastSeenOutput = value;
        }

        private void ClearWordModeLessonOutputAndGrabState()
        {
            TwoZoneGrabState.Clear();
            if (Lexting.assembler.isWordAssemblyActive)
                Lexting.assembler.DeactivateWordAssembler();
            Lexting.lhLine?.ResetStateToIdle(false);
            Lexting.rhLine?.ResetStateToIdle(false);
            Lexting.printer.UpdateOutputs("");
            CaptureStepStartOutput("");
        }

        private void FlashOutputErrorAndRevert(string revertTo)
        {
            if (_flashOutputCoroutine != null)
                return;
            _flashOutputCoroutine = StartCoroutine(FlashOutputErrorRoutine(revertTo));
        }

        /// <summary>
        /// Stops an in-flight red flash so a delayed <see cref="FlashOutputErrorRoutine"/> cannot overwrite
        /// outputs cleared by a later step transition (QA-M3-053: stale "ta" after tantalize reset).
        /// </summary>
        private void CancelOutputFlash()
        {
            if (_flashOutputCoroutine == null)
                return;
            StopCoroutine(_flashOutputCoroutine);
            _flashOutputCoroutine = null;
            if (_flashOutputTexts != null && _flashOutputOriginalColors != null)
            {
                for (int i = 0; i < _flashOutputTexts.Count && i < _flashOutputOriginalColors.Count; i++)
                {
                    if (_flashOutputTexts[i] != null)
                        _flashOutputTexts[i].color = _flashOutputOriginalColors[i];
                }
            }
            _flashOutputTexts = null;
            _flashOutputOriginalColors = null;
        }

        private IEnumerator FlashOutputErrorRoutine(string revertTo)
        {
            Lexting.audio.Play("SelectionClick");
            var texts = GetOutputTexts();
            _flashOutputTexts = texts;
            _flashOutputOriginalColors = texts.Select(t => t.color).ToList();
            var flash = new Color(0.85f, 0.2f, 0.2f, 1f);
            foreach (var t in texts)
                t.color = flash;
            yield return new WaitForSeconds(0.25f);
            Lexting.printer.UpdateOutputs(revertTo);
            _lastSeenOutput = revertTo;
            if (IsSingleLetterTutorialTypingStep)
                _typingInsertLocked = false;
            for (int i = 0; i < texts.Count; i++)
                texts[i].color = _flashOutputOriginalColors[i];
            _flashOutputTexts = null;
            _flashOutputOriginalColors = null;
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

        /// <summary>Skip step 20 coaching only when a conjugated commit landed after step 19 began (not bare assembled lemma).</summary>
        private bool ShouldFastPathPastFinalizationCoaching(string output)
        {
            if (!HasCompletedTantalizeFinalization(output))
                return false;
            return !string.Equals((output ?? "").Trim(), (_outputSnapshotStep19 ?? "").Trim(),
                System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Accepted commit that is not the naked stem <c>tantalize</c> (finalization suffix pick).</summary>
        private static bool HasCompletedTantalizeFinalization(string output)
        {
            return IsAcceptedTantalizeCommit(output) && !IsBareTantalizeLemma(output);
        }

        private static bool IsBareTantalizeLemma(string output)
        {
            if (string.IsNullOrWhiteSpace(output))
                return false;

            var trimmed = Regex.Replace(output.Trim(), @"[\s\p{P}]+$", "");
            trimmed = Regex.Replace(trimmed, @"^[\s\p{P}]+", "");
            var token = Regex.Replace(trimmed, @"[^\w]", "").ToLowerInvariant();
            return token == "tantalize";
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

        /// <summary>Skip control: wrap-up page (step 21), celebration now, full dismiss after its timer.</summary>
        public void JumpToWrapUpFromSkip()
        {
            if (tutorialCompleted)
                return;

            TwoZoneGrabState.Clear();
            if (Lexting.assembler.isWordAssemblyActive)
                Lexting.assembler.DeactivateWordAssembler();
            if (Lexting.finalizer.isSpatialListActive)
                Lexting.finalizer.ShowList(false);
            Lexting.lhLine?.ResetStateToIdle(false);
            Lexting.rhLine?.ResetStateToIdle(false);

            HideSkipControls();
            _outputWatchActive = false;
            Lexting.audio?.Play("Success_Mid", TutorialSuccessSfxVolume);
            _playedSuccessSfxForSkipJump = true;
            EnterStep(21);
        }

        public void CompleteTutorial()
        {
            if (tutorialCompleted)
                return;

            tutorialCompleted = true;
            StopStepCoroutine();
            _outputWatchActive = false;
            if (!_playedSuccessSfxForSkipJump)
                Lexting.audio?.Play("Success_Mid", TutorialSuccessSfxVolume);
            _playedSuccessSfxForSkipJump = false;
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
            yield return new WaitForSeconds(TutorialCheckMarkHandler.FullOnStrokeSeconds + 0.35f);
            _leftCheckmark?.DrawCheckmark(false);
            _rightCheckmark?.DrawCheckmark(false);
            yield return new WaitForSeconds(TutorialCheckMarkHandler.FullOnStrokeSeconds + 0.35f);
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
            var eligible = System.Array.FindAll(allMarks,
                m => m != null && !TutorialHandSide.IsUnderPoseReferenceHierarchy(m.transform));

            _leftCheckmark = PickCheckmarkForHand(_leftHand, allMarks);
            _rightCheckmark = PickCheckmarkForHand(_rightHand, allMarks);

            if ((_leftCheckmark == null || _rightCheckmark == null || _leftCheckmark == _rightCheckmark)
                && eligible.Length >= 1)
            {
                if (_leftCheckmark == null)
                {
                    foreach (var m in eligible)
                    {
                        if (m != null && TutorialHandSide.HasAncestorNameContaining(m.transform, "_Left"))
                        {
                            _leftCheckmark = m;
                            break;
                        }
                    }
                }
                if (_rightCheckmark == null)
                {
                    foreach (var m in eligible)
                    {
                        if (m != null && TutorialHandSide.HasAncestorNameContaining(m.transform, "_Right"))
                        {
                            _rightCheckmark = m;
                            break;
                        }
                    }
                }
                if (_leftCheckmark == null && _leftHand != null)
                {
                    foreach (var m in eligible)
                    {
                        if (m != null && m.transform.IsChildOf(_leftHand.transform))
                        {
                            _leftCheckmark = m;
                            break;
                        }
                    }
                }
                if (_rightCheckmark == null && _rightHand != null)
                {
                    foreach (var m in eligible)
                    {
                        if (m != null && m.transform.IsChildOf(_rightHand.transform))
                        {
                            _rightCheckmark = m;
                            break;
                        }
                    }
                }
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

            bool wantRight = TutorialHandSide.IsRightSideName(hand.name);
            var sideTag = wantRight ? "_Right" : "_Left";

            foreach (var mark in marks)
            {
                if (mark == null || TutorialHandSide.IsUnderPoseReferenceHierarchy(mark.transform))
                    continue;
                if (TutorialHandSide.HasAncestorNameContaining(mark.transform, sideTag))
                    return mark;
            }

            foreach (var mark in marks)
            {
                if (mark == null || TutorialHandSide.IsUnderPoseReferenceHierarchy(mark.transform))
                    continue;
                if (mark.transform.IsChildOf(hand.transform))
                    return mark;
            }

            int wantSide = wantRight ? 1 : -1;
            TutorialCheckMarkHandler onPoseRefFallback = null;

            foreach (var mark in marks)
            {
                if (mark == null)
                    continue;
                if (TutorialHandSide.SideFromTransform(mark.transform) != wantSide)
                    continue;

                if (!TutorialHandSide.IsUnderPoseReferenceHierarchy(mark.transform))
                    return mark;

                onPoseRefFallback = mark;
            }

            return onPoseRefFallback;
        }

        private static Transform GetHandAttachPointForCheckmark(TutorialHandHandler hand)
        {
            if (hand == null)
                return null;
            var pose = hand.GetComponent<TutorialProceduralHandPose>();
            return pose != null ? pose.PoseRoot : hand.transform;
        }

        private static void AttachCheckmarkToDisplayHand(TutorialCheckMarkHandler mark, TutorialHandHandler hand)
        {
            if (mark == null || hand == null)
                return;

            Transform attach = GetHandAttachPointForCheckmark(hand);
            if (attach == null || mark.transform.parent == attach)
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
            var cube = Lexting.controllers.transform;
            var layoutParent = cube.parent != null ? cube.parent : cube;
            _skipControlsRoot.transform.SetParent(layoutParent, false);
            _skipControlsLayout = _skipControlsRoot.AddComponent<TutorialSkipControlsLayout>();
            _skipControlsLayout.MarginBelowWidget = skipControlsMarginBelowWidget;
            if (_skipControlsRoot.GetComponent<TutorialSkipCursorRange>() == null)
                _skipControlsRoot.AddComponent<TutorialSkipCursorRange>();

            AdoptSkipControlNodes(_skipControlsRoot.transform);

            if (legacySkipControlsRoot != null && legacySkipControlsRoot != _skipControlsRoot)
            {
                AdoptSkipControlNodes(_skipControlsRoot.transform, legacySkipControlsRoot.transform);
                if (Application.isPlaying)
                    Destroy(legacySkipControlsRoot);
                legacySkipControlsRoot = null;
            }

            PruneDontShowAgainNodes(_skipControlsRoot.transform);

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

        private void PruneDontShowAgainNodes(Transform root)
        {
            if (includeDontShowAgainControl || root == null)
                return;

            for (var i = root.childCount - 1; i >= 0; i--)
            {
                var child = root.GetChild(i);
                if (IsDontShowAgainNodeName(child.name))
                {
                    if (Application.isPlaying)
                        Destroy(child.gameObject);
                    else
                        DestroyImmediate(child.gameObject);
                }
            }
        }

        private static bool IsDontShowAgainNodeName(string nodeName)
        {
            if (string.IsNullOrEmpty(nodeName))
                return false;
            return (nodeName.IndexOf("dont", System.StringComparison.OrdinalIgnoreCase) >= 0
                    && nodeName.IndexOf("show", System.StringComparison.OrdinalIgnoreCase) >= 0)
                   || (nodeName.IndexOf("never", System.StringComparison.OrdinalIgnoreCase) >= 0
                       && nodeName.IndexOf("show", System.StringComparison.OrdinalIgnoreCase) >= 0)
                   || nodeName.IndexOf("dontask", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void EnsureRuntimeSkipProvisioned()
        {
            if (!provisionRuntimeSkipWhenMissing || _skipControlsRoot == null)
                return;

            if (Lexting.controllers == null || !Lexting.controllers.gameObject.activeInHierarchy)
                return;

            if (_skipControlsRoot.transform.childCount > 0 && !ShouldRebuildRuntimeSkip())
                return;

            if (_skipControlsRoot.transform.childCount > 0)
                DestroyRuntimeSkipChildren();

            CreateRuntimeSkipControl(_skipControlsRoot.transform);
        }

        private void DestroyRuntimeSkipChildren()
        {
            for (var i = _skipControlsRoot.transform.childCount - 1; i >= 0; i--)
            {
                var child = _skipControlsRoot.transform.GetChild(i);
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }

        private bool ShouldRebuildRuntimeSkip()
        {
            if (_skipControlsRoot.transform.childCount == 0)
                return false;

            foreach (var scaler in _skipControlsRoot.GetComponentsInChildren<TwoZoneTriggerScaler>(true))
            {
                if (scaler.transform.localScale.sqrMagnitude < 1e-8f)
                    return true;
            }

            return false;
        }

        private static void CreateRuntimeSkipControl(Transform parent)
        {
            if (TutorialSkipZoneFactory.TryCreateFromCenterTemplate(parent) != null)
                return;

            Debug.LogWarning(
                $"{nameof(TutorialManager)}: could not clone widget CenterZone for Skip; ensure the two-zone widget is active.");
        }

        private void ShowSkipControls()
        {
            TutorialSkipGrabZone.ReleaseAllWidgetBlocks();
            EnsureSkipControlsRoot();

            if (_skipControlsRoot == null)
                return;

            if (_skipControlsLayout != null)
                _skipControlsLayout.MarginBelowWidget = skipControlsMarginBelowWidget;

            _skipControlsRoot.SetActive(true);
            EnsureRuntimeSkipProvisioned();
            _skipControlsLayout?.SyncToWidget();
            if (_skipControlsRoot.transform.childCount == 0)
                this.Wait(() => CanProvisionRuntimeSkip(), RetrySkipProvisionAndLayout);
        }

        private bool CanProvisionRuntimeSkip()
        {
            if (_skipControlsRoot == null || !_skipControlsRoot.activeInHierarchy)
                return false;
            if (Lexting.controllers == null || !Lexting.controllers.gameObject.activeInHierarchy)
                return false;
            if (TwoZoneCubeControllers.Instance == null)
                return false;
            return TutorialSkipZoneFactory.FindWidgetCenterZoneTransform() != null;
        }

        private void RetrySkipProvisionAndLayout()
        {
            if (_skipControlsRoot == null || !_skipControlsRoot.activeInHierarchy)
                return;
            if (!CanProvisionRuntimeSkip())
            {
                this.Wait(CanProvisionRuntimeSkip, RetrySkipProvisionAndLayout);
                return;
            }

            EnsureRuntimeSkipProvisioned();
            _skipControlsLayout?.SyncToWidget();
        }

        private void HideSkipControls()
        {
            TutorialSkipGrabZone.ReleaseAllWidgetBlocks();
            if (_skipControlsRoot != null)
                _skipControlsRoot.SetActive(false);
        }

        #endregion
    }
}
