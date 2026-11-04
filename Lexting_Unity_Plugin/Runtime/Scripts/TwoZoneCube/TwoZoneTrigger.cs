using SpatialGestureText.Debugging;
using SpatialGestureText.Hotkeys;
using SpatialGestureText.Input;
using SpatialGestureText.Interfaces;
using SpatialGestureText.LetterCube;
using SpatialGestureText.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static SpatialGestureText.Hotkeys.LextingHotkeyManager;

namespace SpatialGestureText.TwoZoneCube
{
    /// <summary>
    /// A single trigger among a collection of triggers in a two-by-two-by-two grid pattern
    /// </summary>
	public class TwoZoneTrigger : MonoBehaviour, ITrigger
    {
        #region vars
        #region public vars
        [HideInInspector] public TwoZoneCubeControllers controllerParent;

        public IController controller { get; set; }

        /// <summary>
        /// The half of the controller dedicated to just this hand
        /// </summary>
        [HideInInspector]
        public TwoZoneCubeController subcontroller;

        /// <summary>
        /// the hard-coded index representing spatial location of this trigger in the UI
        /// </summary>
        [SerializeField] public int index;

        public int Index {
            get {
                return index;
            }
        }

        /// <summary>
        /// the most recently hovered letter
        /// </summary>
        public static string currentRightHighlightedLetter = "", currentLeftHighlightedLetter = "";
        public static string currentHighlightedLetter(bool right) => right ? currentRightHighlightedLetter : currentLeftHighlightedLetter;

        /// <summary>
        /// Has this trigger been toggled to a visibly highlighted color different from its neutral color?
        /// </summary>
        public bool isHighlightedByRight { get; set; }
        public bool isHighlightedByLeft { get; set; }

        /// <summary>
        /// Is that hand currently hovering inside this trigger? Note that this is different from isHighlighted 
        /// because up to two zones per hand can be highlighted during word assembly mode, but only one per hand
        /// can be hovered.
        /// </summary>
        public bool isHoveredByRight { get; set; }
        public bool isHoveredByLeft { get; set; }

        public bool IsReady() => this.Ready();

        public Transform center => transform;

        public bool IsCenter => transform.name.Equals("CenterZone");

        public bool IsHotkey => IsCenter && _matrixStateIndexOffset == 0
            && ((_right && Lexting.rhRotState == CubeRotationState.relax) || (!_right && Lexting.lhRotState == CubeRotationState.relax));

        public bool IsCapsFirst => IsCenter && _matrixStateIndexOffset == 0
            && ((_right && Lexting.rhRotState == CubeRotationState.pronate) || (!_right && Lexting.lhRotState == CubeRotationState.pronate));

        public bool IsMatrixState => IsCenter
            && ((_right && Lexting.rhRotState == CubeRotationState.supinate) || (!_right && Lexting.lhRotState == CubeRotationState.supinate));

        public bool IsAlpha => !(IsHotkey || IsCapsFirst || IsMatrixState) && _matrixStateIndexOffset == 0;

        /// <summary>
        /// A string representation of the matrix display state from the hotkey manager (specifies whether matrix is alpha, #s, or special)
        /// </summary>
        public string matrixDisplayState { get; set; }
        #endregion public vars
        #region private vars

        private bool _right => subcontroller.IsRight;

        /// <summary>
        /// controls bump-scale when trigger activates
        /// </summary>
        private TwoZoneTriggerScaler _scaler;

        /// <summary>
        /// refreshes text display of this trigger's letter based upon user hand rotation input
        /// </summary>
        private LetterSwitcher _letterSwitcher;

        /// <summary>
        /// highlighted mesh that signifies hover for this trigger when visible
        /// </summary>
        private TwoZoneTriggerHighlight _highlight;

        /// <summary>
        /// Cached rotation state of the current hand
        /// </summary>
        private CubeRotationState _currentRotationState => Lexting.rotState(_right);

        /// <summary>
        /// Cached rotation state of the non-current hand
        /// </summary>
        private CubeRotationState _otherRotationState => Lexting.rotState(!_right);

        /// <summary>
        /// Cached current highlight state store at time of latest highlight change.
        /// If right hand just entered or exited, this is isHighlightedByRight, 
        /// otherwise it is isHighlightedByLeft.
        /// </summary>
        private bool _isHighlighted => (_right) ? isHighlightedByRight : isHighlightedByLeft;

        /// <summary>
        /// Cached current hover state at time of latest hover change. If right hand just entered or exited,
        /// this is isHoveredByRight, otherwise it is isHoveredByLeft.
        /// </summary>
        private bool _isHovered => (_right) ? isHoveredByRight : isHoveredByLeft;

        /// <summary>
        /// The drag handler assigned to this trigger's subcontroller (either right or left)
        /// </summary>
        private CubeDragLineVisualizer _dragLine => Lexting.line(_right);

        
        private static int _currentRightStartingIndex;

        private static int _currentLeftStartingIndex;

        /// <summary>
        /// Cached value of current hand's starting index during drag 
        /// </summary>
        private int _currentStartingIndex;

        /// <summary>
        /// Cached value of non-current hand's starting index during drag
        /// </summary>
        private int _otherStartingIndex;

        /// <summary>
        /// if right hand just activated this trigger, this is currentRightHighlightedLetter, otherwise currentLeftHighlightedLetter
        /// </summary>
        private string _currentHighlightedLetter {
            get {
                return (_right) ? currentRightHighlightedLetter : currentLeftHighlightedLetter;
            }
            set {
                if (_right) currentRightHighlightedLetter = value;
                else currentLeftHighlightedLetter = value;
            }
        }

        private string _audioSourceName;

        /// <summary>
        /// if true, this trigger is able to show x or z when user gestures forward palm pose
        /// </summary>
        private bool _hasOverflow => index <= 1;

        /// <summary>
        /// An offset to shift the available chars for this zone into the subset appropriate for the current matrix type
        /// e.g. offset is 0 if matrix state is "Alpha" and offset is 3 if matrix state is "Numbers"
        /// </summary>
        private int _matrixStateIndexOffset = 0;

        #endregion private vars
        #endregion vars

        #region init
        private void Awake()
        {
            matrixDisplayState = "->Numbers";
            index = transform.parent.GetSiblingIndex();
            _scaler = transform.GetComponentInParent<TwoZoneTriggerScaler>();
            this.Ch(ref _letterSwitcher, ref _highlight);
            controller = LextingController.Instance;
        }

        public void Init()
        {
            RegisterAllEventsAndActions();
        }
        #endregion init

        #region event handlers
        private void RegisterAllEventsAndActions()
        {
            this.GetGestures(
                (right, current, previous) => {/*trigger state*/},
                (right, current, previous) =>
                {// rotation state

                    if (right != _right || !_isHovered) return;

                    if (current == CubeRotationState.forward && !_hasOverflow)
                        return;
                    else
                    {
                        if (current == CubeRotationState.forward && _hasOverflow)
                            _currentHighlightedLetter = _letterSwitcher.letters[(_matrixStateIndexOffset==0)?9:10];
                        else
                        {
                            if (IsHotkey)
                                _currentHighlightedLetter = "HotKey";
                            else if (IsCapsFirst)
                                _currentHighlightedLetter = "Caps_1st";
                            else if (IsMatrixState)
                                _currentHighlightedLetter = matrixDisplayState;
                            else if ((int)current <= 3)
                                _currentHighlightedLetter = "";
                            if ((int)current <= 3 && IsAlpha)
                                _currentHighlightedLetter = _letterSwitcher.letters[(int)current + _matrixStateIndexOffset];
                        }
                        UpdateWordAssembler();
                    }
                }
            );

            this.Subscribe(Lexting.hotkeys.OnMatrixDisplayStateChanged, (newMatrixState) => {
                _matrixStateIndexOffset = newMatrixState switch
                {
                    MatrixDisplayState.alpha => 0,
                    MatrixDisplayState.numbers => 3,
                    MatrixDisplayState.special => 6,
                    _ => throw new NotImplementedException()
                };

                if (!IsMatrixState) return;
                matrixDisplayState = newMatrixState switch
                {
                    MatrixDisplayState.alpha => "->Numbers",
                    MatrixDisplayState.numbers => "->Special",
                    MatrixDisplayState.special => "->Alpha",
                    _ => throw new NotImplementedException()
                };
            });
        }

        /// <summary>
        /// A helper for both regular TriggerEnter and ReturnTriggerEnterMethods
        /// </summary>
        public void HandleTriggerEnter(bool right)
        {
            //if (TwoZoneGrabState.firstReady(_right) && IsCenter)
            //{
            //    LogOutputReceiver.Instance.Log("Just dragged into center... ignoring hover.");
            //    return; // skip center zone if dragging into it from elsewhere
            //}


            if (right != subcontroller.IsRight) return;
            _highlight.Toggle(true);
            _letterSwitcher.ToggleMaterial(true);
            UpdateHoverEntryState();
            UpdateWordAssembler();
            UpdateFinalizer();
            _audioSourceName = IsCenter ? "Hoverclick_Hard" : "Hoverclick_Soft";
            Lexting.audio.Play(_audioSourceName);
        }

        /// <summary>
        /// Since a point cursor just exited this trigger, update the word assembler if word entry mode is active.
        /// </summary>
        /// <param name="right">if true, right hand just exited this zone, otherwise left did</param>
        public void HandleExit(bool right)
        {
            if (right != subcontroller.IsRight) return;
            if (!_isHovered || !_isHighlighted) return;

            _currentStartingIndex = (right) ? _currentRightStartingIndex : _currentLeftStartingIndex;
            _otherStartingIndex = (right) ? _currentLeftStartingIndex : _currentRightStartingIndex;

            if (_dragLine.isLineTracingActive)
            {
                if (_dragLine.currentTwoZoneIndex == index && _dragLine.currentTwoZoneIndex != _currentStartingIndex &&
                    _dragLine.currentTwoZoneIndex != _otherStartingIndex)
                    ResetTrigger(true);
            }
            else ResetTrigger(true);

            UpdateHoverExitState();
            _currentHighlightedLetter = "";
        }
        #endregion event handlers

        #region helper methods
        private void UpdateHoverEntryState()
        {
           
            if (_right)
            {
                isHighlightedByRight = true;
                isHoveredByRight = true;
            }
            else
            {
                isHighlightedByLeft = true;
                isHoveredByLeft = true;
            }

            if (IsHotkey)
                _currentHighlightedLetter = "HotKey";
            else if (IsCapsFirst)
                _currentHighlightedLetter = "Caps_1st";
            else if (IsMatrixState)
                _currentHighlightedLetter = matrixDisplayState;
            else if (_currentRotationState == CubeRotationState.forward && _hasOverflow)
                _currentHighlightedLetter = _letterSwitcher.letters[9];
            else _currentHighlightedLetter = 
                    _letterSwitcher.letters[((int)_currentRotationState <= 2) ? (int)_currentRotationState + _matrixStateIndexOffset : 1 ];
             
           // Debug.Log($"TwoZoneTrigger {_right} _currentHighlightedLetter: {_currentHighlightedLetter}");
            //if (right && (int)SpatialGestureInput.Instance.currentRightRotationState <= 2)
            //    _currentHighlightedLetter = _letterSwitcher.letters[(int)SpatialGestureInput.Instance.currentRightRotationState];
            //else if (!right && (int)SpatialGestureInput.Instance.currentLeftRotationState <= 2)
            //    _currentHighlightedLetter = _letterSwitcher.letters[(int)SpatialGestureInput.Instance.currentLeftRotationState];
        }

        private void UpdateHoverExitState()
        {
            isHoveredByRight = false;
            isHoveredByLeft = false;
        }

        public void ResetTrigger()
        {
            if (_right && LextingController.Instance.currentRightTrigger.Index == index) return;
            if (!_right && LextingController.Instance.currentLeftTrigger.Index == index) return;
            ResetProperties();
        }

        private void ResetTrigger(bool didExit)
        {
            ResetProperties();
        }

        private void ResetProperties()
        {
            _highlight.Toggle(false);
            _letterSwitcher.ToggleMaterial(false);
            isHighlightedByRight = false;
            isHighlightedByLeft = false;
        }

        #region Word Assembly Helpers

        /// <summary>
        /// Refreshes the assembler's state based on fresh trigger input
        /// </summary>
        public void UpdateWordAssembler()
        {
            if (_dragLine.isLineTracingActive && _currentHighlightedLetter.Length == 1)
            {
                if (TwoZoneGrabState.firstReady(_right)) // Is this not the first letter to be submitted for this hand?
                {
                    if (!TwoZoneGrabState.first(_right).Equals(_currentHighlightedLetter)) // if the next letter isn't the same...
                        Lexting.assembler.AddLetter(_right, _currentHighlightedLetter);
                    else if (TwoZoneGrabState.compSingleReady(_right))                     // if it is, the hand just returned to start...
                        Lexting.assembler.SubtractLetter(_right);
                }
                else Lexting.assembler.AddLetter(_right, _currentHighlightedLetter);

                _dragLine.currentTwoZoneIndex = index;
            }
        }

        private void UpdateFinalizer()
        {
            if (Lexting.finalizer.isSpatialListActive)
            {
                if (!_right)
                { 
                    if (IsAlpha)
                        Lexting.finalizer.SelectWord(index);
                }
                else Lexting.finalizer.SelectPunctuation(index);
            }
        }

        #endregion Word Assembly Helpers

        public void SetStartingIndex(bool right)
        {
            if (right)
                _currentRightStartingIndex = TwoZoneCubeController.currentRightTriggerIndex;
            else _currentLeftStartingIndex = TwoZoneCubeController.currentLeftTriggerIndex;
        }

        public void ToggleHighlightOn(bool right)
        {
            if (right == (controller as TwoZoneCubeController).IsRight)
                _highlight.Toggle(true);
        }

        public void Bump(UnityAction callback)
        {
            _scaler?.Bump(callback);
        }
        #endregion helper methods

        #region Unit Tests

        #endregion Unit Tests
    }
}
