using SpatialGestureText.Hotkeys;
using SpatialGestureText.Input;
using SpatialGestureText.Interfaces;
using SpatialGestureText.LetterCube;
using SpatialGestureText.Tutorial;
using SpatialGestureText.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SpatialGestureText.TwoZoneCube
{
    /// <summary>
    /// A controller for either the right or left hand of the two-zone cube controller,
    /// which enables that hand to drive character entry via zone triggers in a 2x2x2 grid formation.
    /// </summary>
    public class TwoZoneCubeController : MonoBehaviour
    {
        #region private vars
        /// <summary>
        /// if true, the right hand operates this cube, otherwise left
        /// </summary>
        [SerializeField] private bool _right;
        public bool IsRight => _right;
        

        public TwoZoneCharacterDisplayHandler charDisplayHandler;
        
        private Vector3 _currentHandDirection;

        /// <summary>
        /// cached ref of either right or left current upon status change
        /// </summary>
        private string _currentHighlightedLetter;

        /// <summary>
        /// cached ref to either right or left drag line visualizer upon status change
        /// </summary>
        private CubeDragLineVisualizer _currentDragLineVisualizer;

        /// <summary>
        /// cached ref to the drag line visualizer that is opposite of current
        /// </summary>
        private CubeDragLineVisualizer _otherDragLineVisualizer;

        /// <summary>
        /// converts a 3-axis index to a 1-axis index, which enables the convenience of handling
        /// the elements of a 2x2x2 grid as though they're elements in a simply array
        /// </summary>
        private int[][][] _conversionGrid = new int[][][] {
                                                new int[2][] {new int[2] { 4, 6 }, new int[2] { 2, 0 }},
                                                new int[2][] {new int[2] { 5, 7 }, new int[2] { 3, 1 }}
                                            };

        private ITrigger[] _triggers;

        private bool _pcMouseActive;

        /// <summary>
        /// cached ref of either right or left current at time of status change
        /// </summary>
        private ITrigger _currentTrigger;

        private struct InsertParams
        {
            public bool right;
            public ITrigger currentTrigger;
            public string currentHighlightedLetter;
            public CubeRotationState currentRotationState;
            public CubeDragLineVisualizer currentDragHandler;
        }

        /// <summary>
        /// A cache of the parameters sent to the InsertNewLetter method. We store these in a temp cache if the insert method attempts
        /// to execute while a hotkey is also potentially about to execute
        /// </summary>
        private InsertParams _temporaryParamCache;

        /// <summary>
        /// Delays insert of the first inserted char while hands are positioned for a potential hotkey insertion, so that if 
        /// the hotkey gets inserted within a time threshold after letter insertion began, the letter insertion is cancelled
        /// in favor of the hotkey. Vice versa, if the hotkey fails to execute, the letter insertion goes through with a slight delay.
        /// </summary>
        private IDisposable _insertionDelay;

        /// <summary>Guards duplicate OnStateChanged listeners (Quest logcat showed 3× insert per grab).</summary>
        private static int _insertDedupeFrame = -1;
        private static bool _insertDedupeRight;
        private static string _insertDedupeLetter;

        private bool _isMatrixState => _currentHighlightedLetter.Equals("->Alpha") || _currentHighlightedLetter.Equals("->Numbers") || _currentHighlightedLetter.Equals("->Special");
        #endregion private vars

        #region public vars
        public static int currentRightTriggerIndex, currentLeftTriggerIndex;
        public static ITrigger currentRightTrigger, currentLeftTrigger;

        public int currentTriggerIndex;

        public bool inBounds;
        #endregion public vars

        #region init
        private void Awake()
        {
            this.ChsFE(ref _triggers, (trigger) => {
                (trigger as TwoZoneTrigger).subcontroller = this;
                (trigger as TwoZoneTrigger).Init();
            });
            charDisplayHandler = GetComponentInChildren<TwoZoneCharacterDisplayHandler>();
            charDisplayHandler.controller = this;
            RegisterAllEventsAndActions();
        }

        private void Start()
        {
#if UNITY_EDITOR
            this.WaitS(1, () => Lexting.controllers.ControllerActivated.I(true));
#endif
        }
        #endregion init

        #region event handlers
        private bool _checkingActivity;
        private void RegisterAllEventsAndActions()
        {
            this.Subscribe(Lexting.hotkeys.OnHotkey, (hotkeySuccess)=> 
            {
                if (!Lexting.hotkeys.isHotkeyAvailable) return;
                Debug.Log($"TwoZoneCubeController received OnHotkey event with success: {hotkeySuccess}");
                if (hotkeySuccess)
                    _insertionDelay?.Dispose();
            });

            // gesture state updates
            this.GetGestures(
                // discrete hand pose or button-press changes:
                (right, current, previous) => 
                { 
                    //InitActivityChecker();

                    //Debug.Log(string.Format($"TwoZoneCubeController state changed: right? {right} - newState {current} - previousState {previous} " +
                    //    $"hasHover? {LextingPositionController.Instance.hasHover} - inputActive {Lexting.input.inputActive} - " +
                    //    $"pcMouseActive? {_pcMouseActive}"));

                    if (right != _right /*|| !inBounds*/) return;

                    if (Lexting.controllers == null || !Lexting.controllers.gameObject.activeInHierarchy)
                        return;

                    var positionController = LextingPositionController.Instance;
                    if (positionController == null)
                        return;

                    if (positionController.hasHover || !Lexting.input.inputActive || _pcMouseActive) return;
                    TwoZoneCubeControllers.Instance.currentTrigger =  (right) ? TwoZoneCubeController.currentRightTrigger : TwoZoneCubeController.currentLeftTrigger;
                    _currentHighlightedLetter = (right) ? TwoZoneTrigger.currentRightHighlightedLetter : TwoZoneTrigger.currentLeftHighlightedLetter;
                    _currentDragLineVisualizer = (right) ? Lexting.rhLine : Lexting.lhLine;
                    _otherDragLineVisualizer = (right) ? Lexting.lhLine : Lexting.rhLine;

                    //Debug.Log(string.Format($"OnStateChanged - right? {right} - newState {current} - previousState {previous} " +
                    //    $"current trigger assigned? {TwoZoneCubeControllers.Instance.currentTrigger != null} - current letter {_currentHighlightedLetter}"));

                    if (Lexting.assembler.isWordAssemblyActive || Lexting.finalizer.isSpatialListActive)
                    {
                        if (CheckFourLetterActivation(right, current, previous))
                            Lexting.assembler.HandleFourLetterSubmission(right);
                        else if (CheckFullWordSubmission(right, current, previous))
                            Lexting.assembler.HandleFullWordSubmission();
                        else if (CheckAssemblyCancellation())
                        {
                            Debug.Log("Cancellation tripped.");
                            Lexting.assembler.DeactivateWordAssembler();
                            return;
                        }
                    }
                    
                    if (Lexting.controllers.currentTrigger != null && current == HandGestureState.Insert)
                        InsertNewLetter(right, current, previous);
                    else if (CheckTwoCharacterSubmission(right, current, previous))
                        HandleTwoCharacterSubmission();
                },
                (right, current, previous) => { /*rotation state changes*/ }
            );

            // the cursor moved from one zone to another
            int hoverUpdateTally = 0;
            this.Subscribe(HandCursor.OnHoverUpdate, (right, inCenter) =>
            {
                if (right != _right || (inCenter && TwoZoneGrabState.firstReady(right))) return;
                hoverUpdateTally++;
                //LogOutputReceiver.Instance.Log(
                //  $"OnHoverUpdate: tally: {hoverUpdateTally}\ninCenter? {inCenter}\nfirst ready? {TwoZoneGrabState.firstReady(right)}");
                if (inCenter)
                    currentTriggerIndex = 8;
                else
                {
                    _currentHandDirection = (right) ? HandCursor.hoverDirectionRight : HandCursor.hoverDirectionLeft;
                    currentTriggerIndex = 
                        _conversionGrid[ConvertAxisDotProductToIndex(_currentHandDirection.x)]
                                       [ConvertAxisDotProductToIndex(_currentHandDirection.y)]
                                       [ConvertAxisDotProductToIndex(_currentHandDirection.z)];
                }

                bool alreadyHovered = false;
                if (right)
                {
                    alreadyHovered = currentTriggerIndex == currentRightTriggerIndex;
                    currentRightTriggerIndex = currentTriggerIndex;
                }
                else
                {
                    alreadyHovered = currentTriggerIndex == currentLeftTriggerIndex;
                    currentLeftTriggerIndex = currentTriggerIndex;
                }

                SelectTriggerByIndex(right, (right) ? currentRightTriggerIndex : currentLeftTriggerIndex);
            });

            // Is the cursor outside its cube entirely?
            this.Subscribe(HandCursor.OnHoverBoundaryUpdate, (right, _inBounds) => {
                if (right != _right) return;
                inBounds = _inBounds;
            });
        }

        private void InitActivityChecker()
        {
            if (!Lexting.input.inputActive && !_checkingActivity)
            {
                _checkingActivity = true;
                ActivityChecker();
                this.WaitS(3, () => {
                    if (!inBounds && tally > 11)
                    {
                        if (Lexting.assembler.isWordAssemblyActive)
                            Lexting.assembler.DeactivateWordAssembler();
                        if (Lexting.finalizer.isSpatialListActive)
                            Lexting.finalizer.ShowList(false);
                    }
                    tally = 0;
                    _checkingActivity = false;
                });
            }
        }

        private int tally = 0;
        private void ActivityChecker()
        {
            tally++;
            this.WaitS(0.2f, () => {
                if (tally > 12 || tally == 0) return;
                else {
                    if (!Lexting.input.inputActive)
                        tally++;
                    ActivityChecker();
                }
            });
        }
        #endregion event handlers

        #region helpers
        #region hover helpers
        private int ConvertAxisDotProductToIndex(float axisDotProduct)
        {
            if (axisDotProduct <= 0)
                return 0;
            else return 1;
        }

        private void SelectTriggerByIndex(bool right, int index)
        {
            FindTriggerByIndex(right, index).HandleTriggerEnter(right);
        }

        private ITrigger FindTriggerByIndex(bool right, int index)
        {
            if (right) currentRightTrigger?.HandleExit(right);
            else currentLeftTrigger?.HandleExit(right);

            foreach (var trigger in _triggers)
                if ((trigger as TwoZoneTrigger).index == index)
                {
                    if (right) currentRightTrigger = trigger;
                    else currentLeftTrigger = trigger;
                    return trigger;
                }
            return null;
        }
        #endregion hover helpers

        #region selection/release helpers
        /// <summary>
        /// Appends the currently hovered trigger's letter to current output
        /// </summary>
        /// <param name="right">if true, right hand submitted the letter</param>
        /// <param name="currentTrigger">the trigger whose letter is to be inserted</param>
        /// <param name="currentHighlightedLetter">the letter to be inserted</param>
        /// <param name="currentRotationState">the rotation state of the hand inserting the letter</param>
        /// <param name="currentDragHandler">the drag handler of the hand that's inserting the letter</param>
        private void InsertNewLetter(bool right, HandGestureState current, HandGestureState previous)
        {
            if (Lexting.finalizer.isSpatialListActive) return;

            var tutorial = TutorialManager.Instance;
            if (tutorial != null && !tutorial.TryConsumeTutorialTypingInsert())
                return;

            var letter = _currentHighlightedLetter ?? "";
            if (Time.frameCount == _insertDedupeFrame
                && _insertDedupeRight == right
                && _insertDedupeLetter == letter)
                return;
            _insertDedupeFrame = Time.frameCount;
            _insertDedupeRight = right;
            _insertDedupeLetter = letter;

            Lexting.audio.Play("SelectionClick");
            if (_currentHighlightedLetter.Equals("Caps_1st"))
                Lexting.printer.CapitalizeNextLetter();
            else if (_isMatrixState)
                Lexting.hotkeys.CycleMatrix();
            else if (_currentHighlightedLetter.Equals("[ENTER]"))
                Lexting.printer.Enter();
            else if (_currentHighlightedLetter.Equals("HotKey"))
                Lexting.hotkeys.ActivateHotkeySelector(right, _currentDragLineVisualizer);
            else
            {
                //Debug.Log($"TwoZoneCubeController - InsertNewLetter {_currentHighlightedLetter} " +
                //    $"L rot state: {Lexting.lhRotState} R rot state: {Lexting.rhRotState}");
                if (Lexting.printer.isActive)
                {
                    Lexting.printer.AppendOutputs(
                        _currentHighlightedLetter, !_otherDragLineVisualizer.isLineTracingActive);
                    tutorial?.ConfirmTutorialTypingInsertCommitted();
                }
                Lexting.controllers.OutputBegan?.I(right, _currentHighlightedLetter);
                _currentDragLineVisualizer.currentTwoZoneIndex =
                    (Lexting.controllers.currentTrigger as TwoZoneTrigger).index;

            }
            Lexting.controllers.currentTrigger?.Bump(() => { });
        }

        private void HandleTwoCharacterSubmission()
        {
            TwoZoneGrabState.Clear();
            if (Lexting.hotkeys.isHotkeyAvailable)
                Lexting.hotkeys.ExecuteHotkey(_currentHighlightedLetter);
        }
        private bool CheckFourLetterActivation(bool right, HandGestureState newState, HandGestureState previousState)
        {
            //Debug.Log(((right) ? "Right " : "Left ") + " checking for letter activation - " +
            //        $"newState: {newState} - previousState: {previousState} - " +
            //        $"isWordAssemblyActive: {Lexting.assembler.isWordAssemblyActive} - " +
            //        $"isWordScrollActive: {Lexting.assembler.isWordScrollActive}");
            return previousState == HandGestureState.Insert && newState != HandGestureState.Insert &&
                    Lexting.handState(!right) == HandGestureState.Insert &&
                    Lexting.assembler.isWordAssemblyActive &&
                    !Lexting.finalizer.isSpatialListActive;
        }

        private bool CheckFullWordSubmission(bool right, HandGestureState newState, HandGestureState previousState)
        {
            //Debug.Log($"Checking full word submission: isSpatialListActive? {Lexting.finalizer.isSpatialListActive}" +
            //    $"newState: {newState} other state: {Lexting.handState(!right) != HandGestureState.Insert}");
            return Lexting.finalizer.isSpatialListActive && newState != HandGestureState.Insert &&
                Lexting.handState(!right) != HandGestureState.Insert;
        }

        private bool CheckTwoCharacterSubmission(bool right, HandGestureState newState, HandGestureState previousState)
        {
            return previousState == HandGestureState.Insert &&
                    Lexting.lhState != HandGestureState.Insert &&
                    Lexting.rhState != HandGestureState.Insert &&
                    !Lexting.assembler.isWordAssemblyActive;           
        }

        private bool CheckAssemblyCancellation()
        {
            return Lexting.lhState != HandGestureState.Insert &&
                   Lexting.rhState != HandGestureState.Insert &&
                   (Lexting.assembler.isWordAssemblyActive || Lexting.finalizer.isSpatialListActive);
        }
        #endregion selection/release helpers

        public void ResetAllTriggers()
        {
            foreach (var trigger in _triggers)
                trigger.ResetTrigger();
        }
        #endregion helpers
    }
}