//using SpatialGestureText.Input;
//using SpatialGestureText.Interfaces;
//using SpatialGestureText.Transformation;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UniRx;
//using UniRx.Triggers;
//using UnityEngine;
//using UnityEngine.Events;
//using UnityEngine.UI;

//namespace SpatialGestureText.LetterCube
//{
//    /// <summary>
//    /// DEPRECATED - only compatible w/ the 3-zone UI
//    /// Controls display and functionality of a single letter zone within the zone text cube.
//    /// </summary>
//    public class CubeTrigger : MonoBehaviour, ITrigger
//    {
//        #region vars
//        [HideInInspector] public static Action<string> LetterHighlightActivated;
//        [HideInInspector] public IController controller { get; set; }

//        public static string[,,] rightCube, leftCube;
//        public static string currentRightHighlightedLetter = "", currentLeftHighlightedLetter = "";
        
//        public CubeLetterTransition currentLerp;

//        /// <summary>
//        /// 0 = letter, 1 = special chars, 2 = numbers
//        /// </summary>
//        public static int characterDisplayIndex;
//        public bool isHighlightedByRight { get; set; }
//        public bool isHighlightedByLeft { get; set; }
//        public CubeTextZone textZone;

//        public bool IsCurrentRight => isHighlightedByRight && CubeController.currentRightTriggerIndex.SequenceEqual(gridIndex);
//        public bool IsCurrentLeft => isHighlightedByLeft && CubeController.currentLeftTriggerIndex.SequenceEqual(gridIndex);

//        public Transform center { get; set; }

//        public CubeCharDisplayController charDisplayController;

//        public bool IsReady() => this.Ready();

//        public bool IsSpace => transform.name == "_111";

//        public bool IsMatrixState { get;  }

//        /// <summary>
//        /// added for ITrigger compatibility even though this class is deprecated
//        /// </summary>
//        public bool IsHotkey { get; }

//        /// <summary>
//        /// not needed for the 3x3x3 cube - only needed to satisfy ITrigger dependencies
//        /// </summary>
//        public int Index {
//            get {
//                return characterDisplayIndex;
//            }
//        }

//        public string matrixDisplayState { get; set; }

//        //public bool isFirstInDragPair;

//        /// <summary>
//        /// if right hand just triggered ontriggerenter, this is currentRightHighlightedLetter, otherwise currentLeftHighlightedLetter
//        /// </summary>
//        private string _currentHighlightedLetter = "";

//        /// <summary>
//        /// DEPRECATED - it no longer matters which hand activated the word list
//        /// if true, right hand activated the active word list, otherwise left did
//        /// </summary>
//        private bool _currentActivatedList;

//        /// <summary>
//        /// the drag handler of the hand that just triggered exit
//        /// </summary>
//        private CubeDragLineVisualizer _currentDragHandler;

//        /// <summary>
//        /// Cached rotation state of the current hand
//        /// </summary>
//        private CubeRotationState _currentRotationState;

//        /// <summary>
//        /// Cached rotation state of the non-current hand
//        /// </summary>
//        private CubeRotationState _otherRotationState;

//        /// <summary>
//        /// if right hand just exited, this = isHighlightedByRight, otherwise it = isHighlightedByLeft
//        /// </summary>
//        private bool _isHighlighted;


//        private int collisionTally;

//        // deprecated
//        //private CubeChildTrigger[] childTriggers;

//        private static string[,,] _rightLetterCube = new string[3, 3, 3] {
////  depth layers                                     n    m    f        n    m    f        n    m    f
////                                                   e    i    a        e    i    a        e    i    a
////                                                   a    d    r        a    d    r        a    d    r
////                                                   r                  r                  r    
////                                                   -    -    -        -    -    -        -    -    -
////  rows                                             l    l    l        m    m    m        h    h    h
////                                                   o    o    o        i    i    i        i    i    i
////                                                   w    w    w        d    d    d        g    g    g
////                                                                                         h    h    h
//                                                { { "l", "v", "k" }, { "m", "a", "s" }, { "g", "n", "h" } },
//                                                { { "c", "i", "w" }, { "y", " ", "o" }, { "r", "e", "t" } },
//                                                { { "q", "x", "z" }, { "p", "u", "f" }, { "j", "d", "b" } }
//                                             };
//        private static string[,,] _leftLetterCube = new string[3, 3, 3] {
//                                                { { "l", "v", "k" }, { "m", "a", "s" }, { "g", "n", "h" } },
//                                                { { "c", "i", "w" }, { "y", " ", "o" }, { "r", "e", "t" } },
//                                                { { "q", "x", "z" }, { "p", "u", "f" }, { "j", "d", "b" } }
//                                             };
//        private static string[,,] numberCube = new string[3, 3, 3] {
//                                                { { "1", ".",   "," }, { "4", "$", "%" }, { "7", "/", "\\" } },
//                                                { { "2", "0",   "#" }, { "5", "=", "@" }, { "8", "*", "(" } },
//                                                { { "3", "ENT", "^" }, { "6", "+", "&" }, { "9", "-", ")" } }
//                                             };
//        private static string[,,] specialCharCube = new string[3, 3, 3] {
//                                                { { "`", ".",    "," }, { "?", "LEFT",  ">" }, { ";",  "|",  "<" } },
//                                                { { "~", "DOWN", "[" }, { "!", "ENT",   "{" }, { ":",  "UP", "(" } },
//                                                { { "&", "&",    "]" }, { "'", "RIGHT", "}" }, { "\"", "-",  ")" } }
//                                             };
        
//        [SerializeField] public int[] gridIndex;
//        private SpatialGestureInput _input;
//        private string _rightLetter
//        {
//            get
//            {
//                return rightCube[gridIndex[0], gridIndex[1], gridIndex[2]];
//            }
//        }
//        private string _leftLetter
//        {
//            get
//            {
//                //Debug.Log(string.Format("CubeTrigger getting left letter {0} for index {1},{2},{3}", leftCube[gridIndex[0], gridIndex[1], gridIndex[2]],
//                //    gridIndex[0], gridIndex[1], gridIndex[2]));

//                return leftCube[gridIndex[0], gridIndex[1], gridIndex[2]];
//            }
//        }

//        private static int[] _currentRightStartingIndex, _currentLeftStartingIndex;

//        /// <summary>
//        /// Cached value of current hand's starting index during drag 
//        /// </summary>
//        private int[] _currentStartingIndex;

//        /// <summary>
//        /// Cached value of non-current hand's starting index during drag
//        /// </summary>
//        private int[] _otherStartingIndex;

//        /// <summary>
//        /// Cached value of the currently highlighted word while word mode is active
//        /// </summary>
//        private string _newCurrentWord;

//        /// <summary>
//        /// Cached value of the name of source to use for center zone vs outer zones
//        /// </summary>
//        private string _audioSourceName;

//        private CubeTriggerScaler _scaler;

//        #region Unit Test Vars
//        //public bool unitTest_Bump;
//        #endregion Unit Test Vars
//        #endregion vars

//        #region init
//        private void Awake()
//        {
//            center = GetComponentInChildren<CubeTriggerCenter>().transform;

//            if (rightCube == null)
//                rightCube = _rightLetterCube;

//            if (leftCube == null)
//                leftCube = _leftLetterCube;

//            _input = SpatialGestureInput.Instance;
//            if (!transform.name.Equals("_111"))
//                _scaler = transform.parent.GetComponent<CubeTriggerScaler>();
//            //charDisplayController = transform.parent.GetComponentInChildren<CubeCharDisplayController>();

            
            
//            if (transform.name.Equals("_000"))
//                this.Wait(() => CubeWordAssemblyController.Instance.Ready(), () => {
//                    this.Subscribe(CubeWordAssemblyController.Instance.OnWordAssemblyBecameInactive, ()=> 
//                    {
//                        ToggleCase(false, true);
//                        //OnRightRotationStateChanged(_input.currentRightRotationState, _input.currentRightRotationState);
//                        //OnLeftRotationStateChanged(_input.currentLeftRotationState, _input.currentLeftRotationState);
//                    });
//                });
            
//            //InitCollisionHoverHandling();
//        }

//        private void Start()
//        {
//            this.Wait(() => controller.IsReady(), () => {
//                this.Subscribe(_input.OnRightRotationStateChanged, OnRightRotationStateChanged);
//                this.Subscribe(_input.OnLeftRotationStateChanged, OnLeftRotationStateChanged);
//            });
//        }

//        /// <summary>
//        /// deprecated
//        /// </summary>
//        //private void InitCollisionHoverHandling()
//        //{
//        //    if (transform.name.Equals("_111"))
//        //        SubscribeToTriggers(this);
//        //    else this.ChsFE(ref childTriggers, (childTrigger) =>
//        //    {
//        //        childTrigger.parentTrigger = this;
//        //        SubscribeToTriggers(childTrigger);
//        //    });
//        //}

//        private void OnDisable()
//        {
//            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
//            if (isHighlightedByRight || isHighlightedByLeft)
//                ResetTrigger();
//        }
//        #endregion init

//        #region loops
//        //private void Update()
//        //{
//        //    if (unitTest_Bump)
//        //    {
//        //        unitTest_Bump = false;
//        //        UnitTest_Bump();
//        //    }
//        //}
//        #endregion loops

//        #region event handlers
//        /// <summary>
//        /// collision-based hover handling -> deprecated (replaced by angular hover handling - see CubeHandTrackingVisualizer.cs)
//        /// </summary>
//        //private void SubscribeToTriggers(MonoBehaviour behaviour)
//        //{
//        //    behaviour.OnTriggerEnterAsObservable().Select(otherCollider => otherCollider.tag).Where(tag => tag.Contains("Activator"))
//        //            .Subscribe((tag) => HandleTriggerEnter(tag.Contains("_Right")));
//        //    behaviour.OnTriggerExitAsObservable().Select(otherCollider => otherCollider.tag).Where(tag => tag.Contains("Activator"))
//        //        .Subscribe((tag) => HandleExit(tag.Contains("_Right")));
//        //}

//        public void HandleTriggerEnter(bool right)
//        {
//            collisionTally++;

//            _currentRotationState = (right) ? _input.currentRightRotationState : _input.currentLeftRotationState;
//            textZone.ToggleCase(_currentRotationState == CubeRotationState.pronate);
            
//            _currentDragHandler = (right) ? controller.rightDragLineHandler : controller.leftDragLineHandler;
//            //_currentActivatedList = (right) ? CubeWordAssemblyController.Instance.rightActivatedList : !CubeWordAssemblyController.Instance.rightActivatedList;
            
//            ToggleHighlightOn(right);
//            _currentHighlightedLetter = (right) ? currentRightHighlightedLetter : currentLeftHighlightedLetter;

//            //Debug.Log($"HandleTriggerEnter - right? {right} isLineTracingActive? {_currentDragHandler.isLineTracingActive} " +
//            //    $"current letter length: {_currentHighlightedLetter.Length}");
//            if (_currentDragHandler.isLineTracingActive && _currentHighlightedLetter.Length == 1)
//            {
//                //Debug.Log((right)?"Right ":"Left" + " trigger about to add letter " + _currentHighlightedLetter);
//                CubeWordAssemblyController.Instance.AddLetter(right, _currentHighlightedLetter);
//                _currentDragHandler.currentGridIndex = gridIndex;
//            }
//            else if (SpatialListController.Instance.isSpatialListActive /*&& _currentActivatedList*/)
//                SetCurrentHighlightedWord();

//            _audioSourceName = (transform.name == "111") ? "Hoverclick_Hard" : "Hoverclick_Soft";
//            CubeAudioManager.Instance.Play(_audioSourceName);
//        }

//        public void HandleReturnTriggerEnter(bool right)
//        { 
//            /// placeholder to temp satisfy the ITrigger interface - this whole class is deprecated, so no need to implement
//        }
//        public void HandleExit(bool right)
//        {
//            if (collisionTally > 0)
//                collisionTally--;

//            // Turn the letter lowercase unless both hands are in uppercase mode
//            _currentRotationState = (right) ? _input.currentRightRotationState : _input.currentLeftRotationState;
//            _otherRotationState = (right) ? _input.currentLeftRotationState : _input.currentRightRotationState;
//            bool handsMatch = _currentRotationState == _otherRotationState;
//            textZone.ToggleCase(_currentRotationState == CubeRotationState.pronate && handsMatch);

//            _isHighlighted = (right) ? isHighlightedByRight : isHighlightedByLeft;
//            if (collisionTally > 0 || !_isHighlighted) return;

//            _currentStartingIndex = (right) ? _currentRightStartingIndex : _currentLeftStartingIndex;
//            _otherStartingIndex = (right) ? _currentLeftStartingIndex : _currentRightStartingIndex;

//            _currentDragHandler = (right) ? controller.rightDragLineHandler : controller.leftDragLineHandler;

//            if (_currentDragHandler.isLineTracingActive && _currentDragHandler.currentGridIndex.Length > 0)
//            {
//                //Debug.Log(string.Format("{0} hand about to exit - " +
//                //        "current drag handler grid index: {1} {2} {3} - " +
//                //        "current startinting drag handler grid index: {4} {5} {6} - " +
//                //        "other startinting drag handler grid index: {7} {8} {9} - " +
//                //        "gridIndex: {10} {11} {12}",
//                //        right ? "Right " : "Left",
//                //        _currentDragHandler.currentGridIndex[0],
//                //        _currentDragHandler.currentGridIndex[1],
//                //        _currentDragHandler.currentGridIndex[2],
//                //        _currentStartingIndex[0],
//                //        _currentStartingIndex[1],
//                //        _currentStartingIndex[2],
//                //        _otherStartingIndex[0],
//                //        _otherStartingIndex[1],
//                //        _otherStartingIndex[2],
//                //        gridIndex[0], gridIndex[1], gridIndex[2]));
//                if (_currentDragHandler.currentGridIndex.SequenceEqual(gridIndex) &&
//                    !_currentDragHandler.currentGridIndex.SequenceEqual(_currentStartingIndex) &&
//                    !_currentDragHandler.currentGridIndex.SequenceEqual(_otherStartingIndex))
//                    ResetTrigger();
//            }
//            else ResetTrigger();

//            if (right) currentRightHighlightedLetter = "";
//            else currentLeftHighlightedLetter = "";
//        }

//        private void OnRightRotationStateChanged(CubeRotationState newState, CubeRotationState previousState)
//        {
//            if (CubeWordAssemblyController.Instance.isWordAssemblyActive) return;

//            switch (newState)
//            {
//                case CubeRotationState.relax:
//                    if (transform.name == "_000") // only needed once bc all changes are static
//                    {
//                        rightCube = _rightLetterCube;
//                        ToggleCase(false, true);
//                        Debug.Log($"CubeTrigger just received lowerCase rotation state update for right hand while highlighted letter is {currentRightHighlightedLetter}");
//                        currentRightHighlightedLetter = currentRightHighlightedLetter.ToLower();
//                    }
//                    if (IsCurrentRight)
//                        textZone.ToggleCase(false);
//                    characterDisplayIndex = 0;
//                    break;
//                case CubeRotationState.pronate:
//                    if (transform.name == "_000")
//                    {
//                        rightCube = _rightLetterCube;
//                        ToggleCase(true, true);
//                        Debug.Log($"CubeTrigger just received upperCase rotation state update for right hand while highlighted letter is {currentRightHighlightedLetter}");
//                        currentRightHighlightedLetter = currentRightHighlightedLetter.ToUpper();
//                    }
//                    if (IsCurrentRight)
//                        textZone.ToggleCase(true);
//                    characterDisplayIndex = 0;
//                    break;
//                case CubeRotationState.forward:
//                    characterDisplayIndex = 1;
//                    rightCube = numberCube;
//                    break;
//                case CubeRotationState.supinate:
//                    characterDisplayIndex = 2;
//                    rightCube = specialCharCube;
//                    break;
//            }
//            if (isHighlightedByRight)
//            {
//                Debug.Log($"CubeTrigger - OnRightRotationState - isHighlightedByRight is true - about to update currentRightHighlightedLeter which is currently {currentRightHighlightedLetter} - " +
//                    $"_rightLetter {_rightLetter} - current grid index 0: {gridIndex[0]} 1: {gridIndex[1]} 2: {gridIndex[2]}");
//                currentRightHighlightedLetter = _rightLetter;
//            }
//        }

//        private void OnLeftRotationStateChanged(CubeRotationState newState, CubeRotationState previousState)
//        {
//            // Debug.Log("current left mode: " + ModeController.Instance.currentLeftMode);
//            // Debug.Log("controller.hand: " + controller.hand);

//            if (CubeWordAssemblyController.Instance.isWordAssemblyActive) return;

//            switch (newState)
//            {
//                case CubeRotationState.relax:
//                    if (transform.name == "_000")
//                    {
//                        leftCube = _leftLetterCube;
//                        ToggleCase(false, false);
//                        Debug.Log($"CubeTrigger just received lowerCase rotation state update for left hand while highlighted letter is {currentLeftHighlightedLetter}");
//                        currentLeftHighlightedLetter = currentLeftHighlightedLetter.ToLower();                        
//                    }
//                    if (IsCurrentLeft)
//                        textZone.ToggleCase(false);
//                    characterDisplayIndex = 0;
//                    break;
//                case CubeRotationState.pronate:
//                    if (transform.name == "_000")
//                    {
//                        leftCube = _leftLetterCube;
//                        ToggleCase(true, false);
//                        Debug.Log($"CubeTrigger just received upperCase rotation state update for left hand while highlighted letter is {currentLeftHighlightedLetter}");
//                        currentLeftHighlightedLetter = currentLeftHighlightedLetter.ToUpper();
//                    }
//                    if (IsCurrentLeft)
//                        textZone.ToggleCase(true);
//                    characterDisplayIndex = 0;
//                    break;
//                case CubeRotationState.forward:
//                    characterDisplayIndex = 1;
//                    leftCube = numberCube;
//                    break;
//                case CubeRotationState.supinate:
//                    characterDisplayIndex = 2;
//                    leftCube = specialCharCube;
//                    break;
//            }
//            if (isHighlightedByLeft)
//                currentLeftHighlightedLetter = _leftLetter;
//        }
//        #endregion event handlers

//        #region helpers
//        public void SetCurrentHighlightedWord()
//        {
//            _newCurrentWord = gridIndex[0].ToString() + gridIndex[1].ToString() + gridIndex[2].ToString();
//            //Debug.Log((right) ? "Right " : "Left" + " CubeTrigger issued command to select word: " + newCurrentWord);
//            //LextingWordListController.Instance.SelectWord(_newCurrentWord);
//        }

//        public void SetCurrentHighlightedPunctuation()
//        { 
//            // not needed - this script was used with the old 3x3x3 cube
//            // this method is only declared to adhere to the ITrigger interface
//        }

//        public static int[] GetIndexByLetter(string letter)
//        {
//            for (int i = 0; i < 3; i++)
//                for (int j = 0; j < 3; j++)
//                    for (int k = 0; k < 3; k++)
//                        if (rightCube[i, j, k] == letter)
//                            return new int[3] { i, j, k };
//            return null;
//        }

//        /// <summary>
//        /// deprecated - see CubeCharDisplayController and CubeTextZone instead
//        /// </summary>
//        public static void ToggleCase(bool upper, bool right)
//        {
//            for (int i = 0; i < 3; i++)
//                for (int j = 0; j < 3; j++)
//                    for (int k = 0; k < 3; k++)
//                    {
//                        if (right)
//                            rightCube[i, j, k] = (upper) ? rightCube[i, j, k].ToUpper() : rightCube[i, j, k].ToLower();
//                        else leftCube[i, j, k] = (upper) ? leftCube[i, j, k].ToUpper() : leftCube[i, j, k].ToLower();
//                    }
//        }

//        public void ToggleHighlightOn(bool right)
//        {
//            //isHighlighted = true;
//            if (right)
//            {
//                isHighlightedByRight = true;
//                //if (CubeController.currentRightTrigger != null)
//                //    CubeController.currentRightTrigger.ResetCharDisplay();
//                LextingController.Instance.currentRightTrigger = this as ITrigger;
//                LetterHighlightActivated?.Invoke(_rightLetter);
//                currentRightHighlightedLetter = _rightLetter;
//                //Debug.Log("CubeTrigger.ToggleHighlightOn - current right just updated to new letter: " + currentRightHighlightedLetter);
//            }
//            else {
//                isHighlightedByLeft = true;
//                //if (CubeController.currentLeftTrigger != null)
//                //    CubeController.currentLeftTrigger.ResetCharDisplay();
//                LextingController.Instance.currentLeftTrigger = this as ITrigger;
//                LetterHighlightActivated?.Invoke(_leftLetter);
//                currentLeftHighlightedLetter = _leftLetter;
//                //Debug.Log("CubeTrigger.ToggleHighlightOn - current left just updated to new letter: " + currentLeftHighlightedLetter);
//            }

//            charDisplayController.Hover(true, characterDisplayIndex, (transform.name == "_111") ? transform.GetSiblingIndex() : transform.parent.GetSiblingIndex());
//            transform.GetChild(0).gameObject.SetActive(true); // highlight mesh
//            transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
//            //Debug.Log("CubeTrigger changed highlighted letter to " + currentHighlightedLetter);
//            //charDisplayController.EnlargeCurrent(characterDisplayIndex,transform.GetSiblingIndex());
//        }

//        public void ResetTrigger()
//        {
//            transform.GetChild(0).gameObject.SetActive(false); // highlight mesh
//            //Debug.Log("Turning off " + transform.GetChild(0).name);
//            transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
//            ResetCharDisplay();
//            _isHighlighted = false;
//            isHighlightedByRight = false;
//            isHighlightedByLeft = false;
//            collisionTally = 0;
//        }

//        public void ResetCharDisplay()
//        {
//            charDisplayController.Hover(false, characterDisplayIndex, (transform.name == "_111") ? transform.GetSiblingIndex() : transform.parent.GetSiblingIndex());
//        }
        
//        public static void HandleMultidigitChar(bool right)
//        {
//            switch ((right) ? currentRightHighlightedLetter : currentLeftHighlightedLetter)
//            {
//                case "ENT":

//                    break;
//                case "UP":

//                    break;
//                case "DOWN":

//                    break;
//                case "LEFT":

//                    break;
//                case "RIGHT":

//                    break;
//            }
//        }

//        public void SetStartingIndex(bool right)
//        {
//            if (right)
//                _currentRightStartingIndex = CubeController.currentRightTriggerIndex;
//            else _currentLeftStartingIndex = CubeController.currentLeftTriggerIndex;
//        }

//        public void Bump(UnityAction callback)
//        {
//            _scaler?.Bump(callback);
//        }
//        #endregion helpers

//        #region Unit Tests
//        //private void UnitTest_Bump()
//        //{
//        //    Bump(null);
//        //}
//        #endregion Unit Tests
//    }
//}


