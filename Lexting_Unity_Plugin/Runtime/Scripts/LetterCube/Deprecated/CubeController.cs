//using SpatialGestureText.Input;
//using SpatialGestureText.Transformation;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using static SpatialGestureText.LetterCube.CubeTouchStates;
//using static OVRSkeleton;
//using UnityEngine.Events;
//using SpatialGestureText.Zones;
//using UniRx;
//using UniRx.Triggers;
//using SpatialGestureText.Tutorial;
//using System.Linq;
//using SpatialGestureText.Utilities;
//using SpatialGestureText.Streaming;
//using SpatialGestureText.TwoZoneCube;
//using SpatialGestureText.Interfaces;

//namespace SpatialGestureText.LetterCube
//{
//    /// <summary>
//    /// DEPRECATED
//    /// Handles display and functionality of the zone-texting cube.
//    /// </summary>
//    public class CubeController : Singleton<CubeController>, IController
//    {
//        #region vars
//        public CubeHand hand;
//        public ITrigger currentRightTrigger {
//            get
//            {
//                return _currentRightTrigger;
//            }
//            set {
//                _currentRightTrigger = value;
//            }
//        }
//        private static ITrigger _currentRightTrigger;

//        public ITrigger currentLeftTrigger {
//            get
//            {
//                return _currentLeftTrigger;
//            }
//            set
//            {
//                _currentLeftTrigger = value;
//            }
//        }
//        private static ITrigger _currentLeftTrigger;

//        [Range(-0.1f, 0.1f)] public float collisionOffset;

//        public static int[] currentRightTriggerIndex, currentLeftTriggerIndex;

//        // if true, this is the PC client receiving a local wireless input stream via UDP - set this true in Inspector for the PC receiver client only
//        public bool isInputStreamReceiver {
//            get {
//                return _isInputStreamReceiver;
//            }
//            set {
//                _isInputStreamReceiver = value;
//            }
//        }

//        [SerializeField] private bool _isInputStreamReceiver;

//        // deprecated
//        public static CubeSuffixTrigger currentRightSuffixTrigger, currentLeftSuffixTrigger;

//        public Vector3 centerPosition {
//            get {
//                if (_spacebarObject.transform != null)
//                    return _spacebarObject.transform.position;
//                else {
//                    Debug.Log("Can't find space bar object - can't determine centerPosition of cube - probably can't tell if a hand is in/out of bounds");
//                    return Vector3.zero;
//                } 
//            }
//        }

//        /// <summary>
//        /// cached ref of either right or left current trigger at time of status change
//        /// </summary>
//        private ITrigger _currentTrigger;

//        /// <summary>
//        /// cached index of either hand upon hover update
//        /// </summary>
//        private int[] _currentTriggerIndex;

//        /// <summary>
//        /// cached ref of either right or left current upon status change
//        /// </summary>
//        private string _currentHighlightedLetter;

//        /// <summary>
//        /// cached ref of either right or left rotation state upon rotation status change
//        /// </summary>
//        private CubeRotationState _currentRotationState;
        
//        /// <summary>
//        /// cached direction of either hand upon hover update
//        /// </summary>
//        private Vector3 _currentHandDirection;

//        /// <summary>
//        /// cached ref to either right or left drag line visualizer upon status change
//        /// </summary>
//        private CubeDragLineVisualizer _currentDragLineVisualizer;

//        /// <summary>
//        /// cached ref to the drag line visualizer that is opposite of current
//        /// </summary>
//        private CubeDragLineVisualizer _otherDragLineVisualizer;

//        private SpatialGestureInput _input;
//        private MeshRenderer[] _renderers;
//        private Collider[] _colliders;
//        private CubeColliderParent[] _colliderParents;
//        private ITrigger[] _triggers;
//        private CubeCharDisplayController[] _charControllers;
//        private GameObject _spacebarObject;
//        private const float _backspaceLoopInterval = 0.1f;
//        private IEnumerator _backspaceLoop;
//        private Vector3 _numberPadOffset = new Vector3(0, 0.10f, 0);
//        private Vector3 _currentCubePosition;
//        private CubeTriggerScaleController _scaleController;
//        private CubeHighlightHandler _cubeHighlightHandler;
//        private bool _pcMouseActive;
        
//        public Vector3 inward;
        
//        public SpatialGestureEvent<bool, string> OutputBegan { get; set; }
//        public SpatialGestureEvent<bool> ControllerActivated { get; set; }

//        #region IController interface implementation
//        [HideInInspector] public bool textCubeActive { get; set; }
//        [HideInInspector] public bool rightHandInBounds { get; set; }
//        [HideInInspector] public bool leftHandInBounds { get; set; }
//        public CubeDragLineVisualizer rightDragLineHandler { get; set; }
//        public CubeDragLineVisualizer leftDragLineHandler { get; set; }
//        public bool IsReady() => this.Ready();
//        public readonly string spacebarObjectName = "_111";
//        #endregion IController interface implementation

//        public Dictionary<int[], Transform> triggersByIndex = new Dictionary<int[], Transform>(new IntArrayEqualityComparer());

//        #region unit test vars
//        //[SerializeField] private bool unitTest_Flash;
//        #endregion unit test vars

//        #endregion vars

//        #region init
//        private void Awake()
//        {
//            OutputBegan = new SpatialGestureEvent<bool, string>();
//            ControllerActivated = new SpatialGestureEvent<bool>();
//            rightDragLineHandler = RightDragMarker.Instance.transform.GetComponent<CubeDragLineVisualizer>();
//            leftDragLineHandler = LeftDragMarker.Instance.transform.GetComponent<CubeDragLineVisualizer>();

//            _input = SpatialGestureInput.Instance;
//            RegisterAllEventsAndActions();
//            //if (isInputStreamReceiver)
//            _spacebarObject = GetComponentInChildren<CubeSpaceHandler>().gameObject;
//            _scaleController = GetComponentInChildren<CubeTriggerScaleController>();
//            _cubeHighlightHandler = GetComponentInChildren<CubeHighlightHandler>();
//            this.Chs(ref _renderers, ref _colliders, ref _colliderParents);
//            this.ChsFE( ref _charControllers,   x => x.controller = this,
//                        ref _triggers,          x => {  x.controller = this;
//                                                        triggersByIndex.Add((x as CubeTrigger).gridIndex, x.center.transform); });
//            ToggleCharDisplay(false);
//        }

//        private void OnEnable()
//        {
//            //rightDragLineHandler.controller = this;
//            //leftDragLineHandler.controller = this;
//            _currentCubePosition = transform.position;
//            ToggleCubeVisiblity(false, false);
//            ToggleColliders(false);
//            InitializeColliderPositions();
//            inward = (transform.name.Contains("Right")) ? -1 * transform.right : transform.right;
//        }
        
//        public void InitializeColliderPositions()
//        {
//            foreach (var colliderParent in _colliderParents)
//                colliderParent.transform.localPosition += new Vector3(0, collisionOffset*colliderParent.transform.localScale.y, 0);
//            if (!isInputStreamReceiver) return;
//            var spaceCollider = _spacebarObject.GetComponent<SphereCollider>();
//            spaceCollider.center += new Vector3(0, collisionOffset, 0);
//        }
//        #endregion Init

//        #region loops
//        //private void Update()
//        //{
//        //    if (unitTest_Flash)
//        //    {
//        //        unitTest_Flash = false;
//        //        UnitTest_Flash(true);
//        //    }
//        //}

//        IEnumerator BackspaceLoopSequence(bool right, TouchStateIndex finger)
//        {
//            while (BackspaceRequirementsAreMet(right, finger) && (right) ? _input.rightHeldLong[(int)finger] : _input.leftHeldLong[(int)finger])
//            {
//                // The index finger is being held long during insertion in backspace position
//                CubeSpaceHandler.Instance.Insert(right);
//                CubeAudioManager.Instance.Play("BackspaceClick");
//                yield return new WaitForSecondsRealtime(_backspaceLoopInterval);
//            }
//        }
//        #endregion loops

//        #region event handlers
//        private void RegisterAllEventsAndActions()
//        {
//            //this.Subscribe(CubeHandTrackingVisualizer.OnHoverUpdate, (right, inCenter) => OnHoverUpdate(right,inCenter));
//            //this.Subscribe(HandCursor.OnHoverUpdate, (right, inCenter) => OnHoverUpdate(right,inCenter));
            
//            //this.Subscribe(CubeHandTrackingVisualizer.OnHoverBoundaryUpdate, (right,inBounds) => {
//            //    //Debug.Log(((right) ? "RIGHT " : "LEFT ") + "BOUNDARY UPDATE: " + ((inBounds) ? "IN BOUNDS" : "OUT OF BOUNDS"));
//            //    if (right) rightHandInBounds = inBounds;
//            //    else leftHandInBounds = inBounds;
//            //});
//            this.Subscribe(HandCursor.OnHoverBoundaryUpdate, (right, inBounds) => {
//                if (right) rightHandInBounds = inBounds;
//                else leftHandInBounds = inBounds;
//            });


//            this.GetGestures(
//                #region OnGestureStateChanged
//                (right, newState, previousState)=> 
//                {
//                    if (LextingPositionController.Instance.hasHover || !_input.inputActive || _pcMouseActive) return;
//                    _currentTrigger = (right) ? currentRightTrigger : currentLeftTrigger;
//                    _currentHighlightedLetter = (right) ? CubeTrigger.currentRightHighlightedLetter : CubeTrigger.currentLeftHighlightedLetter;
//                    _currentRotationState = (right) ? _input.currentRightRotationState : _input.currentLeftRotationState;
//                    _currentDragLineVisualizer = (right) ? rightDragLineHandler : leftDragLineHandler;
//                    _otherDragLineVisualizer = (right) ? leftDragLineHandler : rightDragLineHandler;

//                    Debug.Log(string.Format($"OnStateChanged - right? {right} - newState {newState} - previousState {previousState} " +
//                        $"current trigger assigned? {_currentTrigger != null} - current letter {_currentHighlightedLetter}"));

//#if UNITY_EDITOR
//                    InitializeNewPosition(true);
//#endif

//                    if (GestureChangeNeedsPositionUpdate(right, newState, previousState) && CubeRightMarker.Instance != null)
//                        InitializeNewPosition(true);
//                    else if (_currentTrigger != null && newState == HandGestureState.Insert)
//                        InsertNewLetter(right, _currentTrigger, _currentHighlightedLetter, _currentRotationState, _currentDragLineVisualizer);
//                    else if (CheckFourLetterActivation(right, newState, previousState))
//                        CubeWordAssemblyController.Instance.HandleFourLetterSubmission(right);
//                    else if (CheckFullWordSubmission(right, newState, previousState))
//                        CubeWordAssemblyController.Instance.HandleFullWordSubmission();
//                    else if (CheckTwoCharacterSubmission(right, newState, previousState))
//                        CubeWordAssemblyController.Instance.DeactivateWordAssembler(right);
//                    else if (newState == HandGestureState.Stop)
//                        DismissController(right);
//                    else if (CheckAssemblyCancellation())
//                        CubeWordAssemblyController.Instance.DeactivateWordAssembler();
//                },
//                #endregion OnGestureStateChanged

//                #region OnRotationStateChanged
//                (right, current, previous)=> 
//                {
//                    if (!textCubeActive) return;

//                    if (current == CubeRotationState.forward && previous != CubeRotationState.forward)
//                        transform.position = _currentCubePosition + _numberPadOffset;
//                    else if (current != CubeRotationState.forward && previous == CubeRotationState.forward)
//                        transform.position = _currentCubePosition;
//                }
//                #endregion OnRotationStateChanged
//            );

//            //this.Subscribe(_input.OnHandTrackingToggled, (right,on) => {

//            //});

//            //this.Subscribe(_input.OnHandConfidenceToggled, (right, confidence) => {

//            //});


//            this.Wait(() => HighlightableZoneController._.Instance.Ready(), () =>
//            {
//                this.Subscribe(HighlightableZoneController._.Instance.OnActivateFreestyle, () =>InitializeNewPosition(true));
//                this.Subscribe(HighlightableZoneController._.Instance.OnActivateSpeedTest, ()=>InitializeNewPosition(true));
//                this.Subscribe(HighlightableZoneController._.Instance.OnActivateTypingTutorial, ()=>InitializeNewPosition(true));
//            });

//            if (isInputStreamReceiver)
//            {
//                this.Wait(() => MouseController._.Instance.Ready(), () => {
//                    this.Subscribe(MouseController._.Instance.OnMouseControllerActive, (right, on) => {
//                        ToggleCubeVisiblity(!on, true);
//                        _pcMouseActive = on;
//                    });
//                });
//            }
//        }

//        private void OnHoverUpdate(bool right, bool inCenter)
//        {
//            //if (inCenter)
//            //    _currentTriggerIndex = new int[3] { 1, 1, 1 };
//            //else
//            //{
//            //    _currentHandDirection = (right) ? CubeHandTrackingVisualizer.hoverDirectionRight : CubeHandTrackingVisualizer.hoverDirectionLeft;
//            //    _currentTriggerIndex = new int[3] {
//            //            ConvertAxisDotProductToIndex(_currentHandDirection.x),
//            //            ConvertAxisDotProductToIndex(_currentHandDirection.y),
//            //            ConvertAxisDotProductToIndex(_currentHandDirection.z)
//            //        };
//            //}

//            //bool alreadyHovered = false;
//            //if (right)
//            //{
//            //    alreadyHovered = (currentRightTriggerIndex != null) ? _currentTriggerIndex.SequenceEqual(currentRightTriggerIndex) : false;
//            //    currentRightTriggerIndex = _currentTriggerIndex;
//            //}
//            //else
//            //{
//            //    alreadyHovered = (currentLeftTriggerIndex != null) ? _currentTriggerIndex.SequenceEqual(currentLeftTriggerIndex) : false;
//            //    currentLeftTriggerIndex = _currentTriggerIndex;
//            //}

//            //if (!alreadyHovered)
//            //    SelectTriggerByIndex(right, (right) ? currentRightTriggerIndex : currentLeftTriggerIndex);
//        }
//        #endregion event handlers

//        #region helpers
//        bool GestureChangeNeedsPositionUpdate(bool right, HandGestureState newState, HandGestureState previousState)
//        {
//            return ((newState == HandGestureState.DragSelection && previousState == HandGestureState.Neutral &&
//                ((right && !rightHandInBounds) || (!right && !leftHandInBounds))));
//        }

//        public void UpdateGridPosition(bool right)
//        {
//            float heightOffsetRelativeToHead = -0.4f;
//            float depthOffsetRelativeToHead = 0.4f;
//            _currentCubePosition = HeadMarker.Instance.transform.position + HeadMarker.Instance.transform.forward * depthOffsetRelativeToHead;
//            _currentCubePosition = new Vector3(
//                _currentCubePosition.x, 
//                HeadMarker.Instance.transform.position.y + heightOffsetRelativeToHead, 
//                _currentCubePosition.z
//            );
//            Vector3 rightwardHorizontal = -1 * Vector3.Cross(HeadMarker.Instance.transform.forward, Vector3.up).normalized;
//            _currentCubePosition -= 0.05f * rightwardHorizontal; // adjust toward user's center line
//            transform.position = _currentCubePosition;

//            Vector3 lookDirectionNonHeight =
//                new Vector3(
//                    ZoneTextingDemoCanvas.Instance.transform.position.x, 
//                    HeadMarker.Instance.transform.position.y + heightOffsetRelativeToHead,
//                    ZoneTextingDemoCanvas.Instance.transform.position.z
//                ) - HeadMarker.Instance.transform.position;
//            transform.rotation = Quaternion.LookRotation(lookDirectionNonHeight);
//        }
        
//        public void ToggleCubeVisiblity(bool on, bool enforceHighlights)
//        {
//            Debug.Log("CubeController - ToggleCubeVisibility - about to toggle cube " + hand);
//            foreach (MeshRenderer renderer in _renderers)
//                if (renderer.tag != "CubeHighlightRenderer" || enforceHighlights)
//                    renderer.enabled = on;

//            //foreach (CubeCharDisplayController displayController in _charControllers)
//            //    if (on)
//            //        displayController.Initialize();
//            //    else displayController.ToggleAllChildren(false);

//            foreach (CubeTrigger trigger in _triggers)
//                if (trigger.isHighlightedByRight || trigger.isHighlightedByLeft)
//                    trigger.ToggleHighlightOn(true);
//        }

//        public void ToggleCharDisplay(bool on)
//        {
//            foreach (CubeCharDisplayController displayController in _charControllers)
//                displayController.ToggleVisibility(on);
//        }

//        public void ToggleColliders(bool on)
//        {
//            foreach (Collider collider in _colliders)
//                collider.enabled = on;
//        }
        
//        private bool BackspaceRequirementsAreMet(bool right, TouchStateIndex finger)
//        {
//            bool pointer = finger == TouchStateIndex.pointer;
//            bool textIsPresent = (Lexting.printer.isActive) ? LextingOutputManager.Instance.CurrentOutputValue.Length > 0 : false;
//            bool insertModeActive = (right) ? _input.currentRightHandGestureState == HandGestureState.Click : _input.currentLeftHandGestureState == HandGestureState.Click;
//            bool currentIsBackspace =
//                ((right) ? CubeTrigger.currentRightHighlightedLetter == " " : CubeTrigger.currentLeftHighlightedLetter == " ")
//                && _input.currentRightRotationState == CubeRotationState.pronate;
//            return pointer && textIsPresent && insertModeActive && currentIsBackspace;
//        }

//        private bool CheckFourLetterActivation(bool right, HandGestureState newState, HandGestureState previousState)
//        {
//            Debug.Log(((right) ? "Right " : "Left ") + " checking for letter activation - " +
//                    $"newState: {newState} - previousState: {previousState} - " +
//                    $"isWordAssemblyActive: {CubeWordAssemblyController.Instance.isWordAssemblyActive} - " +
//                    $"isWordScrollActive: {CubeWordAssemblyController.Instance.isWordScrollActive}");
//            return previousState == HandGestureState.Insert &&
//                    newState != HandGestureState.Insert &&
//                    CubeWordAssemblyController.Instance.isWordAssemblyActive &&
//                    !CubeWordAssemblyController.Instance.isWordScrollActive;
//        }

//        private bool CheckFullWordSubmission(bool right, HandGestureState newState, HandGestureState previousState)
//        {
//            Debug.Log(((right) ? "Right " : "Left ") + "checking for full word submission - " +
//                    $"newState: {newState} - previousState: {previousState} - " +
//                    $" isWordAssemblyActive: {CubeWordAssemblyController.Instance.isWordAssemblyActive} - " +
//                    $" isWordScrollActive: {CubeWordAssemblyController.Instance.isWordScrollActive}");
//            return previousState == HandGestureState.Insert &&
//                    newState != HandGestureState.Insert &&
//                    CubeWordAssemblyController.Instance.isWordAssemblyActive &&
//                    CubeWordAssemblyController.Instance.isWordScrollActive;
//        }

//        private bool CheckTwoCharacterSubmission(bool right, HandGestureState newState, HandGestureState previousState)
//        {
//            //CubeDragLineVisualizer currentDragHandler = (right) ? rightDragLineHandler : leftDragLineHandler;
//            //CubeDragLineVisualizer otherDragHandler = (!right) ? rightDragLineHandler : leftDragLineHandler;
//            //Debug.Log(((right) ? "Right " : "Left ") + "checking for two character submission - " +
//            //        $"newState: {newState} - previousState: {previousState} - " +
//            //        $" is line tracing active: {currentDragHandler.isLineTracingActive}" +
//            //        $" is other hand line tracing active: {otherDragHandler.isLineTracingActive}");
//            return previousState == HandGestureState.Insert &&
//                    newState != HandGestureState.Insert &&
//                    _currentDragLineVisualizer.isLineTracingActive && 
//                    !_otherDragLineVisualizer.isLineTracingActive;
//        }

//        private bool CheckAssemblyCancellation()
//        {
//            return _input.currentLeftHandGestureState != HandGestureState.Insert &&
//                   _input.currentRightHandGestureState != HandGestureState.Insert &&
//                   CubeWordAssemblyController.Instance.isWordAssemblyActive;
//        }

//        public void ResetAllTriggers()
//        {
//            foreach (var trigger in _triggers)
//                if (trigger.isHighlightedByRight || trigger.isHighlightedByLeft)
//                    trigger.ResetTrigger();
            
//            currentRightTrigger?.ToggleHighlightOn(true);
//            currentLeftTrigger?.ToggleHighlightOn(false);
//        }

//        public void ResetAllTriggers(bool right)
//        {
//            foreach (CubeTrigger trigger in _triggers)
//                if ((right && trigger.isHighlightedByRight) || (!right && trigger.isHighlightedByLeft))
//                    trigger.ResetTrigger();

//            currentRightTrigger.ToggleHighlightOn(true);
//            currentLeftTrigger.ToggleHighlightOn(false);
//        }
        
//        private void InsertNewLetter(
//            bool right, 
//            ITrigger currentTrigger, 
//            string currentHighlightedLetter, 
//            CubeRotationState currentRotationState,
//            CubeDragLineVisualizer currentDragHandler)
//        {
//            bool currentIsBackspace = currentHighlightedLetter == " " && currentRotationState == CubeRotationState.pronate;
//            CubeAudioManager.Instance.Play((currentIsBackspace) ? "BackspaceClick" : "SelectionClick");
            
//            if (currentIsBackspace)
//                CubeWordAssemblyController.Instance.ClearEntireList();

//            if (currentHighlightedLetter == " ")
//            {
//                //if (right)
//                CubeSpaceHandler.Instance.Insert(right);
//                //else CubeLeftSpaceHandler.Instance.Insert();

//            }
//            else if (currentHighlightedLetter.Length > 1)
//                CubeTrigger.HandleMultidigitChar(true);
//            else
//            {

//                if(LextingOutputManager.Instance.isActive)
//                        LextingOutputManager.Instance.AppendOutputs(currentHighlightedLetter,
//                                                                    !(_currentDragLineVisualizer.isLineTracingActive
//                                                                      && _otherDragLineVisualizer.isLineTracingActive));
//                OutputBegan.I(right, currentHighlightedLetter);

//                currentDragHandler.currentGridIndex = (currentTrigger as CubeTrigger).gridIndex;
//                Debug.Log("just assigned current " + ((right) ? "right " : "left ") + "grid index "
//                    + $"[{(currentTrigger as CubeTrigger).gridIndex[0]}] [{(currentTrigger as CubeTrigger).gridIndex[1]}] [{(currentTrigger as CubeTrigger).gridIndex[2]}]");
//            }

//            currentTrigger?.Bump(() => { });
//        }

//        private void InitializeNewPosition(bool right)
//        {
//            //UpdateGridPosition(right);
//            ToggleCubeVisiblity(true, false);
//            ToggleColliders(true);
//            textCubeActive = true;
//            ControllerActivated.I(true);
//            ResetAllTriggers();
//        }

//        private void DismissController(bool right)
//        {
//            ToggleCubeVisiblity(false, true);
//            ToggleColliders(false);
//            ResetAllTriggers(right);
//            textCubeActive = false;
//            ControllerActivated.I(false);
//        }

//        private void SelectTriggerByIndex(bool right, int[] gridIndex)
//        {
//            if (right) currentRightTrigger?.HandleExit(right);
//            else currentLeftTrigger?.HandleExit(right);

//            foreach (var trigger in _triggers)
//                if ((trigger as CubeTrigger).gridIndex.SequenceEqual(gridIndex))
//                {
//                    if (right) currentRightTrigger = trigger;
//                    else currentLeftTrigger = trigger;
//                    trigger.HandleTriggerEnter(right);
//                    return;
//                }
//        }

//        private int ConvertAxisDotProductToIndex(float axisDotProduct)
//        {
//            if (axisDotProduct < -1 || axisDotProduct > 1) return -1;
//            else if (axisDotProduct < -0.25f) return 0;
//            else if (axisDotProduct < 0.25f) return 1; 
//            else return 2;
//        }

//        public void FlashCubeHighlight(bool correct)
//        {
//            if (_flash != null)
//                StopCoroutine(_flash);
//            _flash = Flash(correct);
//            StartCoroutine(_flash);
//        }

//        private IEnumerator _flash;
//        private IEnumerator Flash(bool correct)
//        {
//            _cubeHighlightHandler.Toggle(true, true);
//            yield return new WaitForSeconds(0.1f);
//            _cubeHighlightHandler.Toggle(false, true);
//        }

//        public Vector3 GetLetterPosition(char letter, bool right)
//        {
//            return triggersByIndex[CubeLetterIndexConverter.CharToIndex(letter)].position;
//        }
//        #endregion helpers

//        #region Unit Tests
//        public void UnitTest_ActivateControllerTrigger()
//        {
//            ControllerActivated.I(true);
//        }

//        public void UnitTest_Flash(bool correct)
//        {
//            FlashCubeHighlight(correct);
//        }
//        #endregion Unit Tests
//    }
//}