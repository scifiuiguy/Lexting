//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using static OVRInput;
//using SpatialGestureText.LetterCube;
//using static SpatialGestureText.LetterCube.CubeTouchStates;
//using UnityEngine.Events;
//using SpatialGestureText.Utilities;

//namespace SpatialGestureText {
//    /// <summary>
//    /// DEPRECATED
//    /// Maps Oculus Touch open/closed finger inputs to relevant gesture states using capacitive touch whenever possible
//    /// </summary>
//    public class OculusTouchInputTest : Singleton<OculusTouchInputTest>
//    {
//        //#region vars
//        //[SerializeField] public float holdActionThreshold;
//        ////[HideInInspector] public UnityAction<HandGestureState, HandGestureState> LeftStateChanged, RightStateChanged;
//        ////[HideInInspector] public UnityAction<CubeRotationState, CubeRotationState> LeftRotationStateChanged, RightRotationStateChanged;
//        ////[HideInInspector] public UnityAction<bool, TouchStateIndex> HoldAction;
//        ////[HideInInspector] public UnityAction<bool, bool[]> RefreshHeldLongState;
//        //[HideInInspector] public SpatialGestureEvent<HandGestureState, HandGestureState> LeftStateChanged = new SpatialGestureEvent<HandGestureState, HandGestureState>();
//        //[HideInInspector] public SpatialGestureEvent<HandGestureState, HandGestureState> RightStateChanged = new SpatialGestureEvent<HandGestureState, HandGestureState>();
//        //[HideInInspector] public SpatialGestureEvent<CubeRotationState, CubeRotationState> LeftRotationStateChanged = new SpatialGestureEvent<CubeRotationState, CubeRotationState>();
//        //[HideInInspector] public SpatialGestureEvent<CubeRotationState, CubeRotationState> RightRotationStateChanged = new SpatialGestureEvent<CubeRotationState, CubeRotationState>();
//        //[HideInInspector] public SpatialGestureEvent<bool, TouchStateIndex> HoldAction = new SpatialGestureEvent<bool, TouchStateIndex>();
//        //[HideInInspector] public SpatialGestureEvent<bool, bool[]> RefreshHeldLongState = new SpatialGestureEvent<bool, bool[]>();

//        //[HideInInspector] public TouchStates rightHandState, leftHandState;
//        //[HideInInspector] public HandGestureState currentRightHandGestureState, currentLeftHandGestureState;
//        //[HideInInspector] public CubeRotationState currentRightRotationState, currentLeftRotationState;

//        //public bool[] rightHeldLong, leftHeldLong;
        
//        //private GameObject _selectionRightStartingMarker, _selectionLeftStartingMarker;
//        //#endregion vars

//        //#region init
//        //private void Awake()
//        //{

//        //    rightHandState = new TouchStates();
//        //    leftHandState = new TouchStates();
//        //    currentRightHandGestureState = HandGestureState.Neutral;
//        //    currentLeftHandGestureState = HandGestureState.Neutral;
//        //    currentRightRotationState = CubeRotationState.relax;
//        //    currentLeftRotationState = CubeRotationState.relax;

//        //    // track whether a currently held finger has passed the hold action threshold
//        //    // index values match the order of the TouchStates enum
//        //    rightHeldLong = new bool[3] { false, false, false };
//        //    leftHeldLong = new bool[3] { false, false, false };

//        //    _selectionRightStartingMarker = new GameObject("RightStartMarker");
//        //    _selectionLeftStartingMarker = new GameObject("LeftStartMarker");
//        //}
//        //#endregion init

//        //#region loops
//        //void Update()
//        //{
//        //    #region thumb
//        //    if (OVRInput.GetDown(OVRInput.Touch.One, Controller.RTouch))
//        //    {
//        //        rightHandState |= TouchStates.thumb;
//        //        UpdateHandState(true);
//        //        StartCoroutine(HoldThresholdTrigger(true, TouchStates.thumb, TouchStateIndex.thumb, Time.time));
//        //    }

//        //    if (OVRInput.GetUp(OVRInput.Touch.One, Controller.RTouch))
//        //    {
//        //        rightHandState &= ~TouchStates.thumb;
//        //        UpdateHandState(true);
//        //        rightHeldLong[(int)TouchStateIndex.thumb] = false;
//        //        RefreshHeldLongState.I(true, rightHeldLong);
//        //    }

//        //    if (OVRInput.GetDown(OVRInput.Touch.One, Controller.LTouch))
//        //    {
//        //        leftHandState |= TouchStates.thumb;
//        //        UpdateHandState(false);
//        //        StartCoroutine(HoldThresholdTrigger(false, TouchStates.thumb, TouchStateIndex.thumb, Time.time));
//        //    }

//        //    if (OVRInput.GetUp(OVRInput.Touch.One, Controller.LTouch))
//        //    {
//        //        leftHandState &= ~TouchStates.thumb;
//        //        UpdateHandState(false);
//        //        leftHeldLong[(int)TouchStateIndex.thumb] = false;
//        //        RefreshHeldLongState.I(true, rightHeldLong);
//        //    }
//        //    #endregion thumb

//        //    #region pointer
//        //    if (OVRInput.GetDown(OVRInput.Touch.PrimaryIndexTrigger, Controller.LTouch))
//        //    {
//        //        leftHandState |= TouchStates.pointer;
//        //        UpdateHandState(false);
//        //        StartCoroutine(HoldThresholdTrigger(false, TouchStates.pointer, TouchStateIndex.pointer, Time.time));
//        //    }

//        //    if (OVRInput.GetUp(OVRInput.Touch.PrimaryIndexTrigger, Controller.LTouch))
//        //    {
//        //        leftHandState &= ~TouchStates.pointer;
//        //        UpdateHandState(false);
//        //        leftHeldLong[(int)TouchStateIndex.pointer] = false;
//        //        RefreshHeldLongState.I(true, rightHeldLong);
//        //    }

//        //    if (OVRInput.GetDown(OVRInput.Touch.PrimaryIndexTrigger, Controller.RTouch))
//        //    {
//        //        rightHandState |= TouchStates.pointer;
//        //        UpdateHandState(true);
//        //        StartCoroutine(HoldThresholdTrigger(true, TouchStates.pointer, TouchStateIndex.pointer, Time.time));
//        //    }

//        //    if (OVRInput.GetUp(OVRInput.Touch.PrimaryIndexTrigger, Controller.RTouch))
//        //    {
//        //        rightHandState &= ~TouchStates.pointer;
//        //        UpdateHandState(true);
//        //        rightHeldLong[(int)TouchStateIndex.pointer] = false;
//        //        RefreshHeldLongState.I(true, rightHeldLong);
//        //    }
//        //    #endregion pointer

//        //    #region grip
//        //    if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, Controller.LTouch))
//        //    {
//        //        leftHandState |= TouchStates.middle;
//        //        UpdateHandState(false);
//        //        StartCoroutine(HoldThresholdTrigger(false, TouchStates.middle, TouchStateIndex.middle, Time.time));
//        //    }

//        //    if (OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, Controller.LTouch))
//        //    {
//        //        leftHandState &= ~TouchStates.middle;
//        //        UpdateHandState(false);
//        //        leftHeldLong[(int)TouchStateIndex.middle] = false;
//        //        RefreshHeldLongState.I(true, rightHeldLong);
//        //    }

//        //    if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, Controller.RTouch))
//        //    {
//        //        rightHandState |= TouchStates.middle;
//        //        UpdateHandState(true);
//        //        StartCoroutine(HoldThresholdTrigger(true, TouchStates.middle, TouchStateIndex.middle, Time.time));
//        //    }

//        //    if (OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, Controller.RTouch))
//        //    {
//        //        rightHandState &= ~TouchStates.middle;
//        //        UpdateHandState(true);
//        //        rightHeldLong[(int)TouchStateIndex.middle] = false;
//        //        RefreshHeldLongState.I(true, rightHeldLong);
//        //    }
//        //    #endregion grip

//        //    #region rotation
//        //    if (currentRightHandGestureState == HandGestureState.DragSelection &&
//        //        ((Vector3.Dot(Vector3.up, Lexting.rHand.transform.up) < 0.75f && currentRightRotationState == CubeRotationState.relax) ||
//        //        (Vector3.Dot(Vector3.up, Lexting.rHand.transform.right) < 0.75f && currentRightRotationState == CubeRotationState.pronate) ||
//        //        (Vector3.Dot(Vector3.up, -1 * Lexting.rHand.transform.right) < 0.75f && currentRightRotationState == CubeRotationState.supinate) ||
//        //        (Vector3.Dot(_selectionRightStartingMarker.transform.forward, Lexting.rHand.transform.right) < 0.75f && currentRightRotationState == CubeRotationState.forward))
//        //        )
//        //    {
//        //        UpdateRotationState(true);
//        //    }

//        //    if (currentLeftHandGestureState == HandGestureState.DragSelection &&
//        //        ((Vector3.Dot(Vector3.up, Lexting.lHand.transform.up) < 0.75f && currentLeftRotationState == CubeRotationState.relax) ||
//        //        (Vector3.Dot(Vector3.up, -1 * Lexting.lHand.transform.right) < 0.75f && currentLeftRotationState == CubeRotationState.pronate) ||
//        //        (Vector3.Dot(Vector3.up, Lexting.lHand.transform.right) < 0.75f && currentLeftRotationState == CubeRotationState.supinate) ||
//        //        (Vector3.Dot(_selectionLeftStartingMarker.transform.forward, -1 * Lexting.lHand.transform.right) < 0.75f && currentLeftRotationState == CubeRotationState.forward))
//        //        )
//        //    {
//        //        UpdateRotationState(false);
//        //    }
//        //    #endregion rotation
//        //}

//        //void UpdateHandState(bool right)
//        //{
            
//        //    HandGestureState previousState = (right) ? currentRightHandGestureState : currentLeftHandGestureState;
//        //    if (right)
//        //    {
//        //        // Selection conditions: middle down, pointer up, thumb up
//        //        if (currentRightHandGestureState != HandGestureState.Selection &&
//        //            (rightHandState & TouchStates.middle) != 0 &&
//        //            (rightHandState & TouchStates.pointer) == 0 &&
//        //            (rightHandState & TouchStates.thumb) == 0)
//        //        {
//        //            currentRightHandGestureState = HandGestureState.Selection;
//        //            _selectionRightStartingMarker.transform.position = Lexting.rHand.transform.position;
//        //            _selectionRightStartingMarker.transform.rotation = Lexting.rHand.transform.rotation;
//        //        }

//        //        // Insertion conditions: all three down
//        //        if (currentRightHandGestureState != HandGestureState.Neutral &&
//        //            currentRightHandGestureState != HandGestureState.Insert &&
//        //            (rightHandState & TouchStates.middle) != 0 &&
//        //            (rightHandState & TouchStates.pointer) != 0 &&
//        //            (rightHandState & TouchStates.thumb) != 0)
//        //            currentRightHandGestureState = HandGestureState.Insert;

//        //        // SqueezeAll conditions: all three down from neutral
//        //        if (currentRightHandGestureState == HandGestureState.Neutral &&
//        //            (rightHandState & TouchStates.middle) != 0 &&
//        //            (rightHandState & TouchStates.pointer) != 0 &&
//        //            (rightHandState & TouchStates.thumb) != 0)
//        //            currentRightHandGestureState = HandGestureState.SqueezeAll;

//        //        // DragSelection conditions: middle and thumb down, pointer up
//        //        if (currentRightHandGestureState != HandGestureState.DragSelection &&
//        //            (rightHandState & TouchStates.middle) != 0 &&
//        //            (rightHandState & TouchStates.pointer) == 0 &&
//        //            (rightHandState & TouchStates.thumb) != 0)
//        //            currentRightHandGestureState = HandGestureState.DragSelection;

//        //        // Neutral condtions: all up
//        //        if (currentRightHandGestureState != HandGestureState.Neutral &&
//        //            (rightHandState & TouchStates.middle) == 0 &&
//        //            (rightHandState & TouchStates.pointer) == 0 &&
//        //            (rightHandState & TouchStates.thumb) == 0)
//        //            currentRightHandGestureState = HandGestureState.Neutral;

//        //        // Click conditions (Cube mode only): 
//        //        if (currentRightHandGestureState != HandGestureState.Click &&
//        //            (rightHandState & TouchStates.middle) != 0 &&
//        //            (rightHandState & TouchStates.pointer) != 0 &&
//        //            (rightHandState & TouchStates.thumb) == 0)
//        //            currentRightHandGestureState = HandGestureState.Click;

//        //        RightStateChanged.I(currentRightHandGestureState, previousState);
//        //    }
//        //    else
//        //    {
//        //        if (currentLeftHandGestureState != HandGestureState.Selection &&
//        //            (leftHandState & TouchStates.middle) != 0 &&
//        //            (leftHandState & TouchStates.pointer) == 0 &&
//        //            (leftHandState & TouchStates.thumb) == 0)
//        //        {
//        //            currentLeftHandGestureState = HandGestureState.Selection;
//        //            _selectionLeftStartingMarker.transform.position = Lexting.lHand.transform.position;
//        //            _selectionLeftStartingMarker.transform.rotation = Lexting.lHand.transform.rotation;
//        //        }

//        //        if (currentRightHandGestureState != HandGestureState.Neutral &&
//        //            currentRightHandGestureState != HandGestureState.Insert &&
//        //            (leftHandState & TouchStates.middle) != 0 &&
//        //            (leftHandState & TouchStates.pointer) != 0 &&
//        //            (leftHandState & TouchStates.thumb) != 0)
//        //            currentLeftHandGestureState = HandGestureState.Insert;

//        //        if (currentLeftHandGestureState == HandGestureState.Neutral &&
//        //            (leftHandState & TouchStates.middle) != 0 &&
//        //            (leftHandState & TouchStates.pointer) != 0 &&
//        //            (leftHandState & TouchStates.thumb) != 0)
//        //            currentLeftHandGestureState = HandGestureState.SqueezeAll;

//        //        if (currentLeftHandGestureState != HandGestureState.DragSelection &&
//        //            (leftHandState & TouchStates.middle) != 0 &&
//        //            (leftHandState & TouchStates.pointer) == 0 &&
//        //            (leftHandState & TouchStates.thumb) != 0)
//        //            currentLeftHandGestureState = HandGestureState.DragSelection;

//        //        if (currentLeftHandGestureState != HandGestureState.Neutral &&
//        //                (leftHandState & TouchStates.middle) == 0 &&
//        //                (leftHandState & TouchStates.pointer) == 0 &&
//        //                (leftHandState & TouchStates.thumb) == 0)
//        //            currentLeftHandGestureState = HandGestureState.Neutral;

//        //        if (currentLeftHandGestureState != HandGestureState.Click &&
//        //            (leftHandState & TouchStates.middle) != 0 &&
//        //            (leftHandState & TouchStates.pointer) != 0 &&
//        //            (leftHandState & TouchStates.thumb) == 0)
//        //            currentLeftHandGestureState = HandGestureState.Click;

//        //        LeftStateChanged.I(currentLeftHandGestureState, previousState);
//        //    }
//        //}

//        ///// <summary>
//        ///// Send a message out to any subcribed component that wants to receive an event trigger 
//        ///// when a time threshold is exceeded for a finger being held down 
//        ///// </summary>
//        ///// <param name="right">If true, it's a finger on the right hand, otherwise left</param>
//        ///// <param name="finger">Pointer, thumb, or index finger?</param>
//        ///// <param name="fingerIndex">A non-flag enumeration of the finger</param>
//        ///// <param name="startTime">The time in seconds when the hold began</param>
//        ///// <returns></returns>
//        //IEnumerator HoldThresholdTrigger(bool right, TouchStates finger, TouchStateIndex fingerIndex, float startTime)
//        //{
//        //    yield return new WaitUntil(() => (Time.time - startTime > holdActionThreshold));
//        //    if ((((right) ? rightHandState : leftHandState) & finger) != 0)
//        //    {
//        //        if (right)
//        //            rightHeldLong[(int)fingerIndex] = true;
//        //        else leftHeldLong[(int)fingerIndex] = true;
//        //        HoldAction.I(right, fingerIndex);
//        //    }
//        //}

//        //void UpdateRotationState(bool right)
//        //{
//        //    CubeRotationState previousRotationState = (right) ? currentRightRotationState : currentLeftRotationState;
//        //    if (right)
//        //    {
//        //        if (Vector3.Dot(Vector3.up, Lexting.rHand.transform.up) >= 0.75f)
//        //            currentRightRotationState = CubeRotationState.relax;

//        //        if (Vector3.Dot(Vector3.up, Lexting.rHand.transform.right) >= 0.75f)
//        //            currentRightRotationState = CubeRotationState.pronate;

//        //        if (Vector3.Dot(Vector3.up, -1 * Lexting.rHand.transform.right) >= 0.75f)
//        //            currentRightRotationState = CubeRotationState.supinate;

//        //        if ((rightHandState & TouchStates.middle) != 0 
//        //            && Vector3.Dot(Lexting.rHand.transform.right, _selectionRightStartingMarker.transform.forward) > 0.75)
//        //            currentRightRotationState = CubeRotationState.forward;

//        //        RightRotationStateChanged.I(currentRightRotationState, previousRotationState);
//        //    }
//        //    else
//        //    {
//        //        if (Vector3.Dot(Vector3.up, Lexting.lHand.transform.up) >= 0.75f)
//        //            currentLeftRotationState = CubeRotationState.relax;

//        //        if (Vector3.Dot(Vector3.up, -1 * Lexting.lHand.transform.right) >= 0.75f)
//        //            currentLeftRotationState = CubeRotationState.pronate;

//        //        if (Vector3.Dot(Vector3.up, Lexting.lHand.transform.right) >= 0.75f)
//        //            currentLeftRotationState = CubeRotationState.supinate;

//        //        if ((leftHandState & TouchStates.middle) != 0 &&
//        //            Vector3.Dot(-1 * Lexting.lHand.transform.right, _selectionLeftStartingMarker.transform.forward) > 0.75f)
//        //            currentLeftRotationState = CubeRotationState.forward;
                
//        //        LeftRotationStateChanged.I(currentLeftRotationState, previousRotationState);
//        //    }
//        //}
//        //#endregion loops
//    }
//}

