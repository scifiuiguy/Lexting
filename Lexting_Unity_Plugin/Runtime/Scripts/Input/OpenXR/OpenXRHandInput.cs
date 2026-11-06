using SpatialGestureText.LetterCube;
using SpatialGestureText.Transformation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static SpatialGestureText.LetterCube.CubeTouchStates;

using SpatialGestureText.Tutorial;
using UnityEngine.Events;

using UniRx;
using UniRx.Triggers;

using UnityEngine.XR;

#if LEXTING_FOUND_OCULUS_SDK
using static OVRSkeleton;
using static OVRHand;
#endif

using SpatialGestureText.TwoZoneCube;
using SpatialGestureText.Interfaces;
using SpatialGestureText.Utilities;
using System.Threading;
using UnityEngine.XR.Hands.Samples.VisualizerSample;
using UnityEngine.XR.Hands;

namespace SpatialGestureText.Input
{
    /// <summary>OpenXR / Unity XR Hands adapter for tracked-hand gesture input (Quest, PC VR, etc.).</summary>
    public class OpenXRHandInput : Singleton<OpenXRHandInput>
    {


        //[HideInInspector] public UnityAction<HandGestureState, HandGestureState> LeftStateChanged, RightStateChanged;
        //[HideInInspector] public UnityAction<CubeRotationState, CubeRotationState> LeftRotationStateChanged, RightRotationStateChanged;
        //[HideInInspector] public UnityAction<bool,bool> SinglePopGunActive;
        //[HideInInspector] public UnityAction<bool> DualPopGunThumbsInAchieved;
        //[HideInInspector] public SpatialGestureEvent<bool, bool> SinglePopGunActive = new SpatialGestureEvent<bool, bool>();
        //[HideInInspector] public SpatialGestureEvent<bool> DualPopGunThumbsInAchieved = new SpatialGestureEvent<bool>();

        //private bool leftPopGunActive, rightPopGunActive, dualPopGunThumbsInActive;

        //[HideInInspector] public UnityAction<bool, TouchStateIndex> HoldAction;

        //[HideInInspector] public UnityAction<bool> ModeActiveStateChanged;

        //#region debug vars
        ////[SerializeField] public Text rightDebugOutput, leftDebugOutput;
        //private int debugIterator;
        //[SerializeField]
        //private bool _debuggingEnabled;
        //private bool tutorialComplete;
        //#endregion debug vars

        #region public vars
        [SerializeField] public float holdActionThreshold;


        [HideInInspector] public SpatialGestureEvent<HandGestureState, HandGestureState> LeftStateChanged = new SpatialGestureEvent<HandGestureState, HandGestureState>();
        [HideInInspector] public SpatialGestureEvent<HandGestureState, HandGestureState> RightStateChanged = new SpatialGestureEvent<HandGestureState, HandGestureState>();
        [HideInInspector] public SpatialGestureEvent<CubeRotationState, CubeRotationState> LeftRotationStateChanged = new SpatialGestureEvent<CubeRotationState, CubeRotationState>();
        [HideInInspector] public SpatialGestureEvent<CubeRotationState, CubeRotationState> RightRotationStateChanged = new SpatialGestureEvent<CubeRotationState, CubeRotationState>();
        public void TriggerRotationStateChange(bool right) 
        {
            if (right)
                RightRotationStateChanged?.I(currentRotationState(right), _previousRotationState(right));
            else LeftRotationStateChanged?.I(currentRotationState(right), _previousRotationState(right));
        }

        [HideInInspector] public SpatialGestureEvent<bool, TouchStateIndex> HoldAction = new SpatialGestureEvent<bool, TouchStateIndex>();
        [HideInInspector] public TouchStates rightHandState, leftHandState;
        [SerializeField] public HandGestureState currentRightHandGestureState, currentLeftHandGestureState;
        [HideInInspector] public CubeRotationState currentRightRotationState, currentLeftRotationState;
        public CubeRotationState currentRotationState(bool right) => right ? currentRightRotationState : currentLeftRotationState;
        public void SetCurrentRotationState(bool right, CubeRotationState newState)
        {
            if (right) currentRightRotationState = newState;
            else currentLeftRotationState = newState;
        }
        
        [HideInInspector] public SpatialGestureEvent<bool> OnHandTrackingToggled = new SpatialGestureEvent<bool>();
        [HideInInspector] public SpatialGestureEvent<bool> OnHandConfidenceToggled = new SpatialGestureEvent<bool>();
        public bool inputActive;

        /// <summary>
        /// An even that triggers when thumb and index tip come within a distance threshold while hand is in neutral pose
        /// </summary>
        [HideInInspector] public SpatialGestureEvent<bool> OppositeMiddleToPalmTouch = new SpatialGestureEvent<bool>();

        /// <summary>
        /// An event that triggers when index finger tip moves beyond a distance threshold away from its base knuckle
        /// while all other fingers are closed. The boolean param specifies which hand began pointing.
        /// </summary>
        [HideInInspector] public SpatialGestureEvent<bool> IndexPointing = new SpatialGestureEvent<bool>();


        [HideInInspector] public SpatialGestureEvent IndexFingersAtEachOther = new SpatialGestureEvent();

        [HideInInspector] public SpatialGestureEvent<bool, bool> SinglePopGunActive = new SpatialGestureEvent<bool, bool>();
        [HideInInspector] public SpatialGestureEvent<bool> DualPopGunActive = new SpatialGestureEvent<bool>();

        //        public bool[] rightHeldLong, leftHeldLong;
        //        public bool modeActive;

        //        [HideInInspector] public SpatialGestureEvent<bool> ModeActiveStateChanged = new SpatialGestureEvent<bool>();

        //        /// <summary>
        //        ///  the direction each palm would face to point toward the other when hands are positioned symmetrically
        //        /// </summary>
        //        private Vector3 inward;

        //        /// <summary>
        //        /// cached direction vector for the current hand
        //        /// </summary>
        //        private Vector3 currentPalmNormal;

        //#if !UNITY_EDITOR && !UNITY_STANDALONE
        //        /// <summary>
        //        /// cached ref to the current hand's skeleton
        //        /// </summary>
        //        private OVRSkeleton currentHandSkeleton;
        //#endif
        #endregion public vars

        #region private vars

        /// These determine whether the user is performing a grab gesture by observing distance between middle finger and palm
        #region Grab vars
        /// <summary>
        /// A shortcut reference to the palm
        /// </summary>
        private Transform _palm(bool right) => Lexting.input.hand(right).palm;

        /// <summary>
        /// the distance between the palm and the middle fingertip
        /// </summary>
        private float _palm2MiddleTipDistance(bool right) => 
            Mathf.Abs(((_mTip(right)?.position ?? Vector3.zero) - (_palm(right)?.position ?? Vector3.zero)).magnitude);

        /// <summary>
        /// if true, the middle finger has closed the gap toward the palm beyond a predetermined threshold
        /// </summary>
        private bool _palmMiddleLess(bool right) => _palm2MiddleTipDistance(right) < _grabMagnitudeThreshold;

        /// <summary>
        /// The conditions that will trigger a grab event
        /// </summary>
        private bool _grabConditions(bool right) => _palmMiddleLess(right);

        /// <summary>
        /// the conditions that will trigger a release event
        /// </summary>
        private bool _releaseConditions(bool right) => !_palmMiddleLess(right);

        /// <summary>
        /// the threshold between which a middle finger closing toward its palm will trigger the grab event
        /// </summary>
        private float _grabMagnitudeThreshold = 0.08f;
        #endregion Grab vars

        /// These determine whether the user is performing an index finger pointing gesture
        #region Point vars
        /// <summary>
        /// A shortcut reference to the index fingertip
        /// </summary>
        private Transform _iTip(bool right) => Lexting.input.hand(right).indexTip;

        /// <summary>
        /// A shortcut reference to the base knuckle of the index finger
        /// </summary>
        private Transform _iBase(bool right) => Lexting.input.hand(right).indexBase;

        /// <summary>
        /// A shortcut reference to the fingertip of the middle finger
        /// </summary>
        private Transform _mTip(bool right) => Lexting.input.hand(right).middleTip;

        private bool _allPointingNodesReady(bool right) => _iTip(right).Ready() && _mTip(right).Ready() && _iBase(right).Ready();

        /// <summary>
        /// The threshold beyond which the index fingertip moving away from its base knuckle will trigger the pointing event
        /// </summary>
        [SerializeField] private float _pointMagnitudeThreshold = 0.07f;

        /// <summary>
        /// The time interval that must elapse after the ThumbOut even triggers before it can be triggered again
        /// </summary>
        [SerializeField] private float _pointTriggerResetInterval = 1f;

        /// <summary>
        /// The conditions to satisfy a reset of the listener for an index finger pointing event
        /// </summary>
        private bool _pointingTriggerReady(bool right) => _baseIndexLess(right) && _baseMiddleLess(right);

        /// <summary>
        /// The distance between the index fingertip and its own base knuckle
        /// </summary>
        private float _baseIndexDistance(bool right) =>
            (_iBase(right) == null) ? 0 : (_iTip(right).position - _iBase(right).position).magnitude;

        /// <summary>
        /// if true, the index fingertip tip has traveled farther away from its base than a predetermined threshold
        /// </summary>
        private bool _baseIndexGreater(bool right) => _baseIndexDistance(right) > _pointMagnitudeThreshold;

        /// <summary>
        /// if true, index finger just retracted below the threshold to reset
        /// </summary>
        private bool _baseIndexLess(bool right) => _baseIndexDistance(right) < _pointMagnitudeThreshold;

        /// <summary>
        /// The conditions needed to exectue the index finger pointing event
        /// </summary>
        private bool _pointingTriggerConditions(bool right) => _baseIndexGreater(right) && _baseMiddleLess(right);

        /// <summary>
        /// the distance between the tip of the middle finger and the base knuckle of the index finger
        /// </summary>
        private float _baseMiddleDistance(bool right) =>
            (_iBase(right) == null) ? 0 : (_mTip(right).position - _iBase(right).position).magnitude;

        /// <summary>
        /// if true, the middle finger has close the distance between itself and the base knuckle of the index finger 
        /// beyond a predetermined threshold
        /// </summary>
        private bool _baseMiddleLess(bool right) => _baseMiddleDistance(right) < _pointMagnitudeThreshold;
        #endregion Point vars

        /// These determine whether the user is performing the "BackWord" gesture (opposite middle finger touches palm)
        #region BackWord vars
        /// <summary>
        /// the float distance between this palm and the other hand's middle finger
        /// </summary>
        private float _palmOpMiddleDistance(bool right) =>
            ((_palm(right)?.position ?? Vector3.zero) - (_mTip(!right)?.position ?? Vector3.zero)).magnitude;

        /// <summary>
        /// above the threshold?
        /// </summary>
        private bool _palmOpMiddleGreater(bool right) => _palmOpMiddleDistance(right) > _backwordMagnitudeThreshold;

        /// <summary>
        /// below the theshold?
        /// </summary>
        private bool _palmOpMiddleLess(bool right) => _palmOpMiddleDistance(right) < _backwordMagnitudeThreshold;

        /// <summary>
        /// Is opposite middle finger close enough to the palm?
        /// </summary>
        private bool _backwordTriggerConditions(bool right) => _palmOpMiddleLess(right)
            && Lexting.handState(right) != HandGestureState.Insert && Lexting.input.inputActive;

        /// <summary>
        /// Is the middle finger far away enough yet for us to start checking when it may touch again?
        /// </summary>
        private bool _backwordTriggerReady(bool right) => _palmOpMiddleGreater(right) && Lexting.handState(right) != HandGestureState.Insert;


        /// <summary>
        /// The threshold beyond which the opposite middle finger's distance from the palm will trigger an event
        /// </summary>
        [SerializeField] private float _backwordMagnitudeThreshold = 0.05f;

        /// <summary>
        /// The time interval that must elapse after the palm touch event triggers before it can be triggered again
        /// </summary>
        [SerializeField] private float _backwordTriggerResetInterval = 0.01f;

        #endregion Backword vars

        /// These determine whether the user is performing the gesture to reposition the UI
        #region repositioning vars

        private Vector3 _indexDirection(bool right) => (_iTip(right).position - _iBase(right).position).normalized;

        private bool _indexFingersAtEachOther => Vector3.Dot(_indexDirection(true), _indexDirection(false)) < 0.2f;

        private bool _repositionConditions => _pointingTriggerConditions(true) && _pointingTriggerConditions(false)
            && _indexFingersAtEachOther;

        private bool _isLoadingReposition;
        #endregion repositioning vars

        #region Tutorial pop-gun vars
        private const float MiddlePopGunCloseThreshold = 0.11f;
        private const float RingPopGunCloseThreshold = 0.08f;
        private const float PopGunPointerConfidenceThreshold = 0.7f;
        private const float PopGunThumbConfidenceThreshold = 0.7f;
        private const float DualPopGunThumbsDotThreshold = 0.3f;

        private Transform _rightRingTip;
        private Transform _leftRingTip;
        private Transform _rightThumbProximal;
        private Transform _leftThumbProximal;

        private bool _rightPopGunLatched;
        private bool _leftPopGunLatched;
        private bool _dualPopGunLatched;
        #endregion Tutorial pop-gun vars

        /// These determine whether the user in changing state by reorienting the direction of the palm
        #region rotation state vars
        private Vector3 _palmNormal(bool right)
        {
#if !UNITY_STANDALONE && UNITY_ANDROID
            var hand = Lexting.input.hand(right);
            var n = QuestHandPalmNormal.Compute(right, hand, QuestHandPalmNormal.DefaultSource);
            if (n.sqrMagnitude > 1e-6f)
                return n.normalized;
#endif
            var marker = right ? Lexting.rHandMarker : Lexting.lHandMarker;
            return marker != null ? marker.palmNormal : Vector3.zero;
        }
        #endregion reotation state vars
#if !UNITY_STANDALONE
#endif

        private enum HandNodeBindingMode
        {
            Unbound,
            OvrLegacy,
            UnityXRHands
        }

        private HandNodeBindingMode _handNodeBindingMode;

#if LEXTING_FOUND_OCULUS_SDK
        private OVRSkeleton _rightSkeleton;
        private OVRSkeleton _leftSkeleton;
        private OVRHand _rightHandState, _leftHandState;
        private TrackingConfidence _rightConfidence, _leftConfidence;
#endif
#if !UNITY_STANDALONE
        private XRHandTrackingEvents _rightHandXREvents, _leftHandXREvents;
        private Transform _rightXRHandSkeleton;
        private Transform _leftXRHandSkeleton;
#endif


        private HandGestureState _previousRightState;
        private HandGestureState _previousLeftState;
        private CubeRotationState _previousRightRotationState;
        private CubeRotationState _previousLeftRotationState;
        private CubeRotationState _previousRotationState(bool right) => right ? _previousRightRotationState : _previousLeftRotationState;
        private void SetPreviousRotationState(bool right, CubeRotationState update)
        {
            if (right) _previousRightRotationState = update;
            else _previousLeftRotationState = update;
        }
        
        //#region Cube Controller hooks
        ///// <summary>
        ///// if true, disable all refs to CubeController bc CubeController doesn't exist on the stream sender side
        ///// </summary>
        //[SerializeField] private bool _isInputStream;
        //[SerializeField] private bool _isUsingTwoZone;

        //private Transform _cubeControllerTransform;
        //private bool _textCubeActive;
        //private bool _rightHandInBounds;
        //private bool _leftHandInBounds;
        //#endregion Cube Controller hooks

        private bool _rightIsTracked, _leftIsTracked;
        //private GameObject _selectionRightStartingMarker, _selectionLeftStartingMarker;
        //[SerializeField] private OVRSkeleton _rightHandSkeleton, _leftHandSkeleton;
        //private HandGestureState previousRightState;
        //private HandGestureState previousLeftState;
        //private bool _rightReady, _leftReady;
        //private bool _rightDragSelectToggle, _leftDragSelectToggle;
        ////private Vector3 _camForwardHorizontal;

        //private float indexDistanceThreshold = 0.11f;
        //private float middleDistanceThreshold = 0.11f;
        //private float ringDistanceThreshold = 0.08f;
        //private float pinkyDistanceThreshold = 0.08f;
        //private float thumbDistanceThreshold = 0.05f;
        #endregion private vars

        #region init
        private static bool _xrGestureBindingsRequested;
        private static bool _gestureRecognizersInitialized;

        private void Awake()
        {
            var onSameObject = GetComponents<OpenXRHandInput>();
            if (onSameObject.Length > 1)
            {
                for (var i = 1; i < onSameObject.Length; i++)
                {
                    Debug.LogWarning(
                        $"{nameof(OpenXRHandInput)}: removing duplicate on '{gameObject.name}' (prefab + runtime AddComponent).",
                        onSameObject[i]);
                    Destroy(onSameObject[i]);
                }

                if (onSameObject[0] != this)
                    return;
            }

            BindHandNodes(() => {
                if (!_xrGestureBindingsRequested)
                {
                    _xrGestureBindingsRequested = true;
                    Lexting.input.InitXrHandEventBindings();
                }
                InitGestureRecognizers();
                InitTrackingStatusChecker();
                inputActive = true;
            });
        }
        private void BindHandNodes(UnityAction callback)
        {
            this.Wait(() => Lexting.inputManager.Ready(), () => {
                Lexting.input.rightHand.wrist = Lexting.inputManager.rightMarker.transform;
                Lexting.input.leftHand.wrist = Lexting.inputManager.leftMarker.transform;

#if !UNITY_STANDALONE
                TryBeginHandNodeBinding(callback);
#else
                callback?.I();
#endif
            });

            UnitTest_GrabVisualization();
        }

#if !UNITY_STANDALONE
        private void TryBeginHandNodeBinding(UnityAction callback)
        {
#if LEXTING_FOUND_OCULUS_SDK
#if UNITY_ANDROID
            QuestHandLog.Info("[OpenXRHandInput] Android bind: OpenXR XR Hands first, OVR fallback second.");
            this.Wait(() => UnityXRHandSkeletonsReady(), () =>
            {
                if (!BindUnityXRHandNodes())
                {
                    QuestHandLog.Warn("[OpenXRHandInput] XR Hands bind failed after visualizer ready.");
                    return;
                }

                _handNodeBindingMode = HandNodeBindingMode.UnityXRHands;
                QuestHandBindingState.SetUnityXRHands();
                QuestHandLog.Info(
                    $"[OpenXRHandInput] XR Hands bound palm={QuestHandPalmNormal.DefaultSource}");
                SyncXrHandInputActiveFromSubsystem();
                callback?.I();
            });
            this.WaitS(8f, () =>
            {
                if (_handNodeBindingMode != HandNodeBindingMode.Unbound)
                    return;

                QuestHandLog.Warn("[OpenXRHandInput] Trying OVR skeleton fallback after XR Hands timeout.");
                QuestOvrHandRigSpawner.EnsureUnderAnchors(Lexting.input.leftHand.wrist, Lexting.input.rightHand.wrist);
            });
            this.Wait(() => _handNodeBindingMode == HandNodeBindingMode.Unbound && TryBindOvrHandNodes(), () =>
            {
                _handNodeBindingMode = HandNodeBindingMode.OvrLegacy;
                QuestHandBindingState.SetOvrLegacy();
                QuestHandLog.Info("[OpenXRHandInput] Hand nodes bound via OVR skeleton fallback.");
                callback?.I();
            });
            return;
#endif
            if (TryBindOvrHandNodes())
            {
                _handNodeBindingMode = HandNodeBindingMode.OvrLegacy;
                QuestHandBindingState.SetOvrLegacy();
                Debug.Log("[OpenXRHandInput] Hand nodes bound via OVR skeleton under hand anchors.");
                callback?.I();
                return;
            }
#endif
            this.Wait(() => UnityXRHandSkeletonsReady(), () =>
            {
                if (!BindUnityXRHandNodes())
                {
                    Debug.LogError("[OpenXRHandInput] Could not bind hand nodes. Use OVRSkeleton under hand anchors, or XRRig with HandVisualizer + XR Hand subsystem.");
                    return;
                }

                _handNodeBindingMode = HandNodeBindingMode.UnityXRHands;
                QuestHandBindingState.SetUnityXRHands();
                Debug.Log("[OpenXRHandInput] Hand nodes bound via Unity XR Hands (XRRig + HandVisualizer).");
                callback?.I();
            });
        }

#if LEXTING_FOUND_OCULUS_SDK
        private static bool AnchorHasLegacyOvrHandRig(Transform wrist)
        {
            if (wrist == null)
                return false;

            foreach (var behaviour in wrist.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour == null)
                    continue;
                var typeName = behaviour.GetType().Name;
                if (typeName is "OVRCustomSkeleton" or "OVRHand" or "OVRSkeleton")
                    return true;
            }

            return false;
        }

        private bool TryBindOvrHandNodes()
        {
            _rightSkeleton = Lexting.input.rightHand.wrist.GetComponentInChildren<OVRSkeleton>();
            _leftSkeleton = Lexting.input.leftHand.wrist.GetComponentInChildren<OVRSkeleton>();
            _rightHandState = Lexting.input.rightHand.wrist.GetComponentInChildren<OVRHand>();
            _leftHandState = Lexting.input.leftHand.wrist.GetComponentInChildren<OVRHand>();

            if (!OVRPlugin.initialized)
                return false;

            if (_rightSkeleton == null || _leftSkeleton == null || _rightHandState == null || _leftHandState == null)
                return false;

            if (!OvrSkeletonReadyForBind(_rightSkeleton) || !OvrSkeletonReadyForBind(_leftSkeleton))
                return false;

            Lexting.input.rightHand.palm = BoneTransform(_rightSkeleton, BoneId.Hand_WristRoot)
                ?? Lexting.input.rightHand.wrist;
            Lexting.input.leftHand.palm = BoneTransform(_leftSkeleton, BoneId.Hand_WristRoot)
                ?? Lexting.input.leftHand.wrist;

            Lexting.input.rightHand.thumbTip = BoneTransform(_rightSkeleton, BoneId.Hand_ThumbTip);
            Lexting.input.leftHand.thumbTip = BoneTransform(_leftSkeleton, BoneId.Hand_ThumbTip);
            Lexting.input.rightHand.middleTip = BoneTransform(_rightSkeleton, BoneId.Hand_MiddleTip);
            Lexting.input.leftHand.middleTip = BoneTransform(_leftSkeleton, BoneId.Hand_MiddleTip);
            Lexting.input.rightHand.middleBase = BoneTransform(_rightSkeleton, BoneId.Hand_Middle1);
            Lexting.input.leftHand.middleBase = BoneTransform(_leftSkeleton, BoneId.Hand_Middle1);
            Lexting.input.rightHand.indexTip = BoneTransform(_rightSkeleton, BoneId.Hand_IndexTip);
            Lexting.input.leftHand.indexTip = BoneTransform(_leftSkeleton, BoneId.Hand_IndexTip);
            Lexting.input.rightHand.indexBase = BoneTransform(_rightSkeleton, BoneId.Hand_Index1);
            Lexting.input.leftHand.indexBase = BoneTransform(_leftSkeleton, BoneId.Hand_Index1);
            _rightThumbProximal = BoneTransform(_rightSkeleton, BoneId.Hand_Thumb0);
            _leftThumbProximal = BoneTransform(_leftSkeleton, BoneId.Hand_Thumb0);
            _rightRingTip = BoneTransform(_rightSkeleton, BoneId.Hand_RingTip);
            _leftRingTip = BoneTransform(_leftSkeleton, BoneId.Hand_RingTip);

            return Lexting.input.rightHand.indexTip.Ready() && Lexting.input.leftHand.indexTip.Ready();
        }

        private static Transform BoneTransform(OVRSkeleton skeleton, BoneId boneId)
        {
            if (skeleton?.Bones == null || skeleton.Bones.Count <= (int)boneId)
                return null;

            return skeleton.Bones[(int)boneId].Transform;
        }

        private static bool OvrSkeletonReadyForBind(OVRSkeleton skeleton)
        {
            if (skeleton == null || !skeleton.IsInitialized)
                return false;

            var thumb = BoneTransform(skeleton, BoneId.Hand_ThumbTip);
            return thumb != null && thumb.Ready();
        }
#endif

        private Transform XRHandsRoot =>
            Lexting.inputManager.rightHand != null ? Lexting.inputManager.rightHand.parent : null;

        private bool UnityXRHandSkeletonsReady()
        {
            var xrHandsRoot = XRHandsRoot;
            if (xrHandsRoot == null)
                return false;

            _rightXRHandSkeleton = XrHandVisualizerUtility.FindHandRoot(xrHandsRoot, true);
            _leftXRHandSkeleton = XrHandVisualizerUtility.FindHandRoot(xrHandsRoot, false);
            if (_rightXRHandSkeleton == null || _leftXRHandSkeleton == null)
                return false;

            return _rightXRHandSkeleton.GetComponent<XRHandTrackingEvents>() != null
                && _leftXRHandSkeleton.GetComponent<XRHandTrackingEvents>() != null;
        }

        private static Transform FindHandJoint(Transform handRoot, string pathFromWrist)
        {
            if (handRoot == null)
                return null;

            return handRoot.Find(pathFromWrist);
        }

        private bool BindUnityXRHandNodes()
        {
            if (!UnityXRHandSkeletonsReady())
                return false;

            _rightHandXREvents = _rightXRHandSkeleton.GetComponent<XRHandTrackingEvents>();
            _leftHandXREvents = _leftXRHandSkeleton.GetComponent<XRHandTrackingEvents>();

            Lexting.input.rightHand.trackingWrist = XrHandVisualizerUtility.TryGetJoint(_rightXRHandSkeleton, XRHandJointID.Wrist);
            Lexting.input.leftHand.trackingWrist = XrHandVisualizerUtility.TryGetJoint(_leftXRHandSkeleton, XRHandJointID.Wrist);

            Lexting.input.rightHand.palm = XrHandVisualizerUtility.TryGetJoint(_rightXRHandSkeleton, XRHandJointID.Palm)
                ?? Lexting.input.rightHand.trackingWrist;
            Lexting.input.leftHand.palm = XrHandVisualizerUtility.TryGetJoint(_leftXRHandSkeleton, XRHandJointID.Palm)
                ?? Lexting.input.leftHand.trackingWrist;

            Lexting.input.rightHand.littleBase = XrHandVisualizerUtility.TryGetJoint(_rightXRHandSkeleton, XRHandJointID.LittleProximal);
            Lexting.input.leftHand.littleBase = XrHandVisualizerUtility.TryGetJoint(_leftXRHandSkeleton, XRHandJointID.LittleProximal);

            Lexting.input.rightHand.thumbTip = XrHandVisualizerUtility.TryGetJoint(_rightXRHandSkeleton, XRHandJointID.ThumbTip);
            Lexting.input.leftHand.thumbTip = XrHandVisualizerUtility.TryGetJoint(_leftXRHandSkeleton, XRHandJointID.ThumbTip);
            Lexting.input.rightHand.middleTip = XrHandVisualizerUtility.TryGetJoint(_rightXRHandSkeleton, XRHandJointID.MiddleTip);
            Lexting.input.leftHand.middleTip = XrHandVisualizerUtility.TryGetJoint(_leftXRHandSkeleton, XRHandJointID.MiddleTip);
            Lexting.input.rightHand.middleBase = XrHandVisualizerUtility.TryGetJoint(_rightXRHandSkeleton, XRHandJointID.MiddleProximal);
            Lexting.input.leftHand.middleBase = XrHandVisualizerUtility.TryGetJoint(_leftXRHandSkeleton, XRHandJointID.MiddleProximal);
            Lexting.input.rightHand.indexTip = XrHandVisualizerUtility.TryGetJoint(_rightXRHandSkeleton, XRHandJointID.IndexTip);
            Lexting.input.leftHand.indexTip = XrHandVisualizerUtility.TryGetJoint(_leftXRHandSkeleton, XRHandJointID.IndexTip);
            Lexting.input.rightHand.indexBase = XrHandVisualizerUtility.TryGetJoint(_rightXRHandSkeleton, XRHandJointID.IndexProximal);
            Lexting.input.leftHand.indexBase = XrHandVisualizerUtility.TryGetJoint(_leftXRHandSkeleton, XRHandJointID.IndexProximal);
            _rightThumbProximal = XrHandVisualizerUtility.TryGetJoint(_rightXRHandSkeleton, XRHandJointID.ThumbProximal)
                ?? XrHandVisualizerUtility.TryGetJoint(_rightXRHandSkeleton, XRHandJointID.ThumbMetacarpal);
            _leftThumbProximal = XrHandVisualizerUtility.TryGetJoint(_leftXRHandSkeleton, XRHandJointID.ThumbProximal)
                ?? XrHandVisualizerUtility.TryGetJoint(_leftXRHandSkeleton, XRHandJointID.ThumbMetacarpal);
            _rightRingTip = XrHandVisualizerUtility.TryGetJoint(_rightXRHandSkeleton, XRHandJointID.RingTip);
            _leftRingTip = XrHandVisualizerUtility.TryGetJoint(_leftXRHandSkeleton, XRHandJointID.RingTip);

            return Lexting.input.rightHand.palm.Ready() && Lexting.input.leftHand.palm.Ready()
                && Lexting.input.rightHand.indexTip.Ready() && Lexting.input.leftHand.indexTip.Ready()
                && Lexting.input.rightHand.middleTip.Ready() && Lexting.input.leftHand.middleTip.Ready();
        }
#endif

        /// <summary>
        /// If Quest hands are not enabled due to Android not being the build target in Editor, we need the dummy
        /// transforms for each hand point node to share the wrist as a parent so they're out of the way in hierarchy.
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="parent"></param>
        private void CollectPlaceholderHandNodes(ref LextingHand hand, Transform parent)
        {
            CollectPlaceHolderNode(ref hand.palm, parent);
            CollectPlaceHolderNode(ref hand.thumbTip, parent);
            CollectPlaceHolderNode(ref hand.middleTip, parent);
            CollectPlaceHolderNode(ref hand.indexBase, parent);
            CollectPlaceHolderNode(ref hand.indexTip, parent);
        }

        /// <summary>
        /// Assists the CollectPlaceholderHandNodes by snapping a single dummy hand point node to its wrist in hierarchy.
        /// </summary>
        /// <param name="handNode"></param>
        /// <param name="parent"></param>
        private void CollectPlaceHolderNode(ref Transform handNode, Transform parent)
        {
            handNode.SetParent(parent);
            handNode.localPosition = Vector3.zero;
            handNode.localRotation = Quaternion.identity;
        }

        private void InitGestureRecognizers()
        {
            if (_gestureRecognizersInitialized)
                return;

            _gestureRecognizersInitialized = true;
            InitGrabCheck();
            InitPointCheck();
            InitBackWordCheck();
            InitRepositioner();
            InitRotationChecker();
            InitTutorialPopGunCheck();
        }

#if !UNITY_STANDALONE
        private void InitTutorialPopGunCheck()
        {
            this.Wait(
                () => PopGunNodesReady(true) && PopGunNodesReady(false),
                () => this.UpdateAsObservable().Subscribe(_ => RunTutorialPopGunFrame()).AddTo(this));
        }

        private Transform RingTip(bool right) => right ? _rightRingTip : _leftRingTip;

        private Transform ThumbProximal(bool right) => right ? _rightThumbProximal : _leftThumbProximal;

        private bool PopGunNodesReady(bool right)
        {
            var hand = Lexting.input.hand(right);
            return hand.indexTip.Ready() && hand.indexBase.Ready() && hand.middleTip.Ready()
                && hand.thumbTip.Ready() && ThumbProximal(right).Ready() && RingTip(right).Ready()
                && WristTransform(right).Ready();
        }

        private Transform WristTransform(bool right)
        {
            var hand = Lexting.input.hand(right);
            return hand.trackingWrist != null && hand.trackingWrist.Ready() ? hand.trackingWrist : hand.wrist;
        }

        private void RunTutorialPopGunFrame()
        {
            if (!inputActive)
                return;
            if (LextingController.Instance != null && LextingController.Instance.isInputStreamReceiver)
                return;

            var tutorial = TutorialManager.Instance;
            if (tutorial == null || !tutorial.tutorialCompleted)
                return;

            if (!_rightPopGunLatched || !_leftPopGunLatched)
            {
                if (!_rightPopGunLatched && HandHasPopGunGesture(true))
                    LatchSinglePopGun(true);
                if (!_leftPopGunLatched && HandHasPopGunGesture(false))
                    LatchSinglePopGun(false);
            }
            else if (!_dualPopGunLatched && HasDualPopGunThumbsIn())
            {
                _dualPopGunLatched = true;
                DualPopGunActive.I(true);
            }
        }

        private void LatchSinglePopGun(bool right)
        {
            if (right)
                _rightPopGunLatched = true;
            else
                _leftPopGunLatched = true;
            SinglePopGunActive.I(true, right);
        }

        /// <summary>Clears pop-gun latch state so tutorial steps can accept the same gestures again.</summary>
        public void ResetTutorialPopGunLatches()
        {
            _rightPopGunLatched = false;
            _leftPopGunLatched = false;
            _dualPopGunLatched = false;
        }

        private Vector3 AimDirectionTowardTutorial()
        {
            if (Lexting.head == null || TutorialManager.Instance == null)
                return Vector3.zero;
            return (TutorialManager.Instance.transform.position - Lexting.head.transform.position).normalized;
        }

        private float PopGunPointerConfidence(bool right)
        {
            var aim = AimDirectionTowardTutorial();
            if (aim.sqrMagnitude < 1e-6f)
                return 0f;
            var indexDir = (_iTip(right).position - _iBase(right).position).normalized;
            return Vector3.Dot(indexDir, aim);
        }

        private bool PopGunFingersClosed(bool right)
        {
            var wrist = WristTransform(right).position;
            bool middleClosed = (_mTip(right).position - wrist).magnitude < MiddlePopGunCloseThreshold;
            bool ringClosed = (RingTip(right).position - wrist).magnitude < RingPopGunCloseThreshold;
            return middleClosed && ringClosed;
        }

        private float PopGunThumbConfidence(bool right)
        {
            var hand = Lexting.input.hand(right);
            var thumbProx = ThumbProximal(right);
            if (!hand.thumbTip.Ready() || !thumbProx.Ready())
                return 0f;
            var thumbDir = (hand.thumbTip.position - thumbProx.position).normalized;
            return Vector3.Dot(thumbDir, Vector3.up);
        }

        private bool HandHasPopGunGesture(bool right)
        {
            return PopGunPointerConfidence(right) > PopGunPointerConfidenceThreshold
                && PopGunThumbConfidence(right) > PopGunThumbConfidenceThreshold
                && PopGunFingersClosed(right);
        }

        private bool HasDualPopGunThumbsIn()
        {
            if (!PopGunFingersClosed(true) || !PopGunFingersClosed(false))
                return false;
            if (PopGunPointerConfidence(true) <= PopGunPointerConfidenceThreshold
                || PopGunPointerConfidence(false) <= PopGunPointerConfidenceThreshold)
                return false;

            var rightHand = Lexting.input.rightHand;
            var leftHand = Lexting.input.leftHand;
            var rightThumb = (rightHand.thumbTip.position - _rightThumbProximal.position).normalized;
            var leftThumb = (leftHand.thumbTip.position - _leftThumbProximal.position).normalized;
            return Vector3.Dot(rightThumb, leftThumb) < DualPopGunThumbsDotThreshold;
        }
#else
        private void InitTutorialPopGunCheck() { }
#endif

        IDisposable grabLogLoopRight;

        /// <summary>
        /// Begins a binarily recursive sequence to listen for user's grab and release actions in each hand
        /// </summary>
        private void InitGrabCheck()
        {
            this.Wait(() => _palm(true).Ready() && _palm(false).Ready(), () => {
                //this.UpdateAsObservable().Subscribe(_ => {
                //    LogOutputReceiver.Instance.Log($"left conditions: {_grabConditions(false)}\n" +
                //        $"left palm middle less? {_palmMiddleLess(false)}\n" +
                //        $"left palm  {_palm(false).position.x} {_palm(false).position.y} {_palm(false).position.z}\n" +
                //        $"left mTip {_mTip(false).position.x} {_mTip(false).position.y} {_mTip(false).position.z}");
                //});
                this.Wait(() => !_palmMiddleLess(true), () => this.Wait(() => _grabConditions(true), () => TriggerGrab(true)));
                this.Wait(() => !_palmMiddleLess(false), () => this.Wait(() => _grabConditions(false), () => TriggerGrab(false)));
            });
        }

        /// <summary>
        /// Begins a binarily recursive sequence to listen for user's index finger pointing and resetting
        /// </summary>
        private void InitPointCheck()
        {
            this.Wait(() => _allPointingNodesReady(true) && _allPointingNodesReady(false), () => {
                this.Wait(() => _pointingTriggerReady(true), () => IndexPointingCheck(true));
                this.Wait(() => _pointingTriggerReady(false), () => IndexPointingCheck(false));
            });
        }

        /// <summary>
        /// Begins a binarily recursive sequence to listen for user touching and releasing palm with opposite middle finger
        /// to activate the BackWord function.
        /// </summary>
        private void InitBackWordCheck()
        {
            //this.UpdateAsObservable().Subscribe(_ =>
            //{
            //    LogOutputReceiver.Instance.Log($"_palmOpMiddleDistance: {_palmOpMiddleDistance(false)}\n" +
            //        $"_backwordTriggerReady? {_backwordTriggerReady(false)}\n" +
            //        $"r palm ready? {_palm(true).Ready()}\n" +
            //        $"l palm ready? {_palm(false).Ready()}\n" +
            //        $"r middle tip ready? {_mTip(true).Ready()}\n" +
            //        $"l middle tip ready? {_mTip(false).Ready()}");
            //});
            this.Wait(() => _palm(true).Ready() && _palm(false).Ready() && _mTip(true).Ready() && _mTip(false).Ready(), () =>
            {
                this.Wait(() => _backwordTriggerReady(true), () => BackwordTriggerCheck(true));
                this.Wait(() => _backwordTriggerReady(false), () => BackwordTriggerCheck(false));
            });
        }

        private void InitRepositioner()
        {
            this.Wait(
                () => _repositionConditions && !_isLoadingReposition, 
                () => LoadReposition());
        }

        private void InitRotationChecker()
        {
            RotationStateCheckLoop(true);
            RotationStateCheckLoop(false);
        }

        private void InitTrackingStatusChecker()
        {
#if !UNITY_STANDALONE
#if LEXTING_FOUND_OCULUS_SDK
            if (_handNodeBindingMode == HandNodeBindingMode.OvrLegacy)
            {
                this.UpdateAsObservable().Subscribe(_ => {
                    CheckAndUpdateStatus(ref _rightIsTracked, ref _rightHandState);
                    CheckAndUpdateStatus(ref _leftIsTracked, ref _leftHandState);
                    CheckAndUpdateConfidence(ref _rightConfidence, ref _rightHandState);
                    CheckAndUpdateConfidence(ref _leftConfidence, ref _leftHandState);
                });
                return;
            }
#endif
            if (_handNodeBindingMode == HandNodeBindingMode.UnityXRHands)
            {
                CheckAndUpdateStatus(true, ref _rightHandXREvents);
                CheckAndUpdateStatus(false, ref _leftHandXREvents);
                this.UpdateAsObservable().Subscribe(_ => SyncXrHandInputActiveFromSubsystem());
            }
#endif
        }

#if !UNITY_STANDALONE && UNITY_ANDROID
        static readonly List<XRHandSubsystem> XrHandSubsystems = new List<XRHandSubsystem>();

        void SyncXrHandInputActiveFromSubsystem()
        {
            if (_handNodeBindingMode != HandNodeBindingMode.UnityXRHands)
                return;

            XrHandSubsystems.Clear();
            SubsystemManager.GetSubsystems(XrHandSubsystems);
            foreach (var subsystem in XrHandSubsystems)
            {
                if (subsystem == null || !subsystem.running)
                    continue;

                _rightIsTracked = subsystem.rightHand.isTracked;
                _leftIsTracked = subsystem.leftHand.isTracked;
                var active = _rightIsTracked && _leftIsTracked;
                if (active == inputActive)
                    return;

                inputActive = active;
                Lexting.input.inputActive = active;
                OnHandTrackingToggled?.I(active);
                return;
            }
        }
#endif
        //        private void Awake()
        //        {
        //            if (_isInputStream)
        //                _cubeControllerTransform = new GameObject("Controller_Placeholder").transform;
        //            else _cubeControllerTransform = Lexting.controllers.transform;

        //            rightHandState = new TouchStates();
        //            leftHandState = new TouchStates();
        //            currentRightHandGestureState = HandGestureState.Neutral;
        //            currentLeftHandGestureState = HandGestureState.Neutral;
        //            currentRightRotationState = CubeRotationState.relax;
        //            currentLeftRotationState = CubeRotationState.relax;

        //            // track whether a currently held finger has passed the hold action threshold
        //            // index values match the order of the TouchStates enum
        //            rightHeldLong = new bool[3] { false, false, false };
        //            leftHeldLong = new bool[3] { false, false, false };

        //            //_selectionRightStartingMarker = new GameObject("RightStartMarker");
        //            //_selectionLeftStartingMarker = new GameObject("LeftStartMarker");

        //            if (!_isInputStream)
        //                this.Subscribe(TutorialManager.Instance.OnTutorialCompleted, OnTutorialCompleted);

        //#if !UNITY_EDITOR && !UNITY_STANDALONE
        //            this.UpdateAsObservable().Subscribe(_ => {
        //                CheckAndUpdateStatus(ref _rightIsTracked, ref _rightHandState);
        //                CheckAndUpdateStatus(ref _leftIsTracked, ref _leftHandState);
        //                CheckAndUpdateConfidence(ref _rightConfidence, ref _rightHandState);
        //                CheckAndUpdateConfidence(ref _leftConfidence, ref _leftHandState);
        //            });
        //            if (_isInputStream)
        //                tutorialComplete = true;
        //#else
        //            _rightIsTracked = true;
        //            _leftIsTracked = true;
        //            inputActive = true;

        //#endif
        //        }

#if LEXTING_FOUND_OCULUS_SDK
        private void CheckAndUpdateStatus(ref bool isTracked, ref OVRHand hand)
        {
            if (hand.IsTracked != isTracked)
            {
                isTracked = hand.IsTracked;
                //Debug.Log($"Check and update status - _rightIsTracked {_rightIsTracked} - _leftIsTracked {_leftIsTracked} - _rightConfidence {_rightConfidence} - _leftConfidence - {_leftConfidence}");
                inputActive = _rightIsTracked && _leftIsTracked;
                //LogOutputReceiver.Instance.Log($"Input just became {((inputActive) ? "active" : "inacative")} due to new tracking status: {isTracked}");
                OnHandTrackingToggled?.I(isTracked);
            }
        }
#endif

        /// <summary>
        /// Listen for a change in tracking state of Unity's XRHands, refresh variables that track hand states, and trigger a Lexting state change event
        /// </summary>
        /// <param name="right">right hand if true, otherwise false</param>
        /// <param name="_events">the Unity XRHand tracking event object for the specified hand</param>
        private void CheckAndUpdateStatus(bool right, ref XRHandTrackingEvents _events)
        {

            _events?.trackingChanged.AddListener(x => {
                if (right) _rightIsTracked = x;
                else _leftIsTracked = x;
                inputActive = _rightIsTracked && _leftIsTracked;
                Lexting.input.inputActive = inputActive;
                OnHandTrackingToggled?.I(inputActive);
            });
        }
#if LEXTING_FOUND_OCULUS_SDK
        private void CheckAndUpdateConfidence(ref TrackingConfidence confidence, ref OVRHand hand)
        {
            if (hand.HandConfidence != confidence)
            {
                confidence = hand.HandConfidence;
                //Debug.Log($"Check and update confidence - _rightIsTracked {_rightIsTracked} - _leftIsTracked {_leftIsTracked} - _rightConfidence {_rightConfidence} - _leftConfidence - {_leftConfidence}");
                inputActive = _rightIsTracked && _leftIsTracked /*&& _rightConfidence == TrackingConfidence.High && _leftConfidence == TrackingConfidence.High*/;
                Lexting.input.inputActive = inputActive;
                //LogOutputReceiver.Instance.Log($"Input just became {((inputActive) ? "active" : "inacative")} " +
                    //$"due to new confidence status.");
                OnHandConfidenceToggled?.I(confidence==TrackingConfidence.High);
            }
        }
#endif
#endregion init

#region loops and timers
        //        /// <summary>
        //        /// main hand input listener
        //        /// </summary>
        //        private void Update()
        //        {
        //            //Debug.Log($"Tutorial complete? {tutorialComplete} Is Input stream? {_isInputStream}");
        //#if !UNITY_EDITOR && !UNITY_STANDALONE
        //            if (!tutorialComplete)
        //            {
        //                // intro tutorial loop
        //                if (!rightPopGunActive || !leftPopGunActive)
        //                {
        //                    if (!rightPopGunActive)
        //                        CheckForPopGun(true); //right
        //                    if (!leftPopGunActive)
        //                        CheckForPopGun(false); //left
        //                }
        //                else CheckForDualPopGuns(); // both

        //            }
        //            else
        //            {
        //                // main loop
        //                //Debug.Log("main input loop running");
        //                CheckGestureState(true); // right
        //                CheckGestureState(false); // left
        //                CheckRotationState(true); // right
        //                CheckRotationState(false); // left

        //                //// right
        //                //previousRightState = currentRightHandGestureState;
        //                //if (!_rightHandState.IsTracked)
        //                //    UpdateHandInput(true, "Right not found", HandGestureState.Neutral);
        //                //else if (_rightHandState.GetFingerIsPinching(OVRHand.HandFinger.Index))
        //                //    UpdateHandInput(true, "Right Index Finger is pinching", HandGestureState.Insert);
        //                //else if (_rightHandState.GetFingerIsPinching(OVRHand.HandFinger.Middle))
        //                //    UpdateHandInput(true, "Right Middle Finger is pinching", HandGestureState.DragSelection);
        //                //else UpdateHandInput(true, "Right neutral", HandGestureState.Neutral);

        //                //previousLeftState = currentLeftHandGestureState;
        //                //if (!_leftHandState.IsTracked)
        //                //    UpdateHandInput(false, "Left not found", HandGestureState.Neutral);
        //                //else if (_leftHandState.GetFingerIsPinching(OVRHand.HandFinger.Index))
        //                //    UpdateHandInput(false, "Left Index Finger is pinching", HandGestureState.Insert);
        //                //else if (_leftHandState.GetFingerIsPinching(OVRHand.HandFinger.Middle))
        //                //    UpdateHandInput(false, "Left Middle Finger is pinching", HandGestureState.DragSelection);
        //                //else UpdateHandInput(false, "Left neutral", HandGestureState.Neutral);
        //            }
        //#else 
        //            previousRightState = currentRightHandGestureState;
        //            previousLeftState = currentLeftHandGestureState;
        //#endif
        //        }

        //#region loop state checks
        //        // includes checks for popgun (tutorial) state, gesture state, and rotation state

        //        private void CheckForPopGun(bool right)
        //        {
        //            if (HandHasPopGunGesture(right) && ((right) ? !rightPopGunActive : !leftPopGunActive))
        //                TogglePopGunActive(true, right);
        //            //else if (!HandHasPopGunGesture(right) && rightPopGunActive)
        //            //    TogglePopGunActive(false, right);
        //        }

        //        private void CheckForDualPopGuns()
        //        {
        //            if (HasPopGunGesturesThumbsIn() && !dualPopGunThumbsInActive)
        //            {
        //                dualPopGunThumbsInActive = true;
        //                DualPopGunThumbsInAchieved.I(true);
        //            }
        //            else if (!HasPopGunGesturesThumbsIn() && dualPopGunThumbsInActive)
        //            {
        //                dualPopGunThumbsInActive = false;
        //                DualPopGunThumbsInAchieved.I(false);
        //            }
        //        }

        //        /// <summary>
        //        /// Each frame, check each hand for gesture state changes (See HandGestureState enum for options)
        //        /// </summary>
        //        /// <param name="right">if true, right hand, otherwise left</param>
        //        private void CheckGestureState(bool right)
        //        {
        //            if (right) previousRightState = currentRightHandGestureState;
        //            else previousLeftState = currentLeftHandGestureState;
        //            OVRHand currentHandState = (right) ? _rightHandState : _leftHandState;
        //            //if (right)
        //            //    Debug.Log("Is finger pinching? " + currentHandState.GetFingerIsPinching(OVRHand.HandFinger.Index));

        //            if (!currentHandState.IsTracked)
        //                UpdateHandInput(right, ((right) ? "Right " : "Left ") + "not found", HandGestureState.Neutral);
        //            else if (currentHandState.GetFingerIsPinching(OVRHand.HandFinger.Index))
        //                UpdateHandInput(right, ((right) ? "Right " : "Left ") + "Index Finger is pinching", HandGestureState.Insert);
        //            //else if (currentHandState.GetFingerIsPinching(OVRHand.HandFinger.Middle))
        //            //    UpdateHandInput(right, ((right) ? "Right " : "Left ") + "Middle Finger is pinching", HandGestureState.DragSelection);
        //            else if (IsFistClosed((right) ? _rightHandSkeleton : _leftHandSkeleton ))
        //                UpdateHandInput(right, ((right) ? "Right " : "Left ") + "fist closed", HandGestureState.SqueezeAll);
        //            else UpdateHandInput(right, ((right) ? "Right " : "Left ") + "neutral", HandGestureState.Neutral);
        //        }

        //        /// <summary>
        //        /// Each frame, check each hand for rotation state changes (see CubeRotationState for options)
        //        /// </summary>
        //        /// <param name="right">if true, right hand, otherwise left</param>
        //        private void CheckRotationState(bool right)
        //        {
        //            if (!inputActive) return;
        //            CubeRotationState previousRotationState = (right) ? currentRightRotationState : currentLeftRotationState;
        //            if (right)
        //                UpdateRotationState(true, ref currentRightRotationState, previousRotationState);
        //            else UpdateRotationState(false, ref currentLeftRotationState, previousRotationState);
        //        }
        //#endregion loop state checks

        //#region loop state updates (triggers upon checks confirmed)
        //        // includes updates for popgun (tutorial) state, gesture state, and rotation state

        //        private void TogglePopGunActive(bool on, bool right)
        //        {
        //            if (right) rightPopGunActive = on;
        //            else leftPopGunActive = on;
        //            SinglePopGunActive.I(on, right);
        //        }

        //        public void NetworkGestureUpdateTrigger(bool right, HandGestureState newState, HandGestureState previous)
        //        {
        //            UpdateHandInput(right, "Gesture update relayed from server", newState);
        //        }

        //        public void NetworkRotationUpdateTrigger(bool right, CubeRotationState newState, CubeRotationState previous)
        //        {
        //            Debug.Log("Network Rotation update triggered");
        //            BroadcastRotationStateChange(right, newState, previous);
        //        }

        //        /// <summary>
        //        /// Notify the application gesture state has changed
        //        /// </summary>
        //        /// <param name="right">if true, right hand, otherwise left hand</param>
        //        /// <param name="logText">short description of the state change</param>
        //        /// <param name="newState">the new gesture state</param>
        //        private void UpdateHandInput(bool right, string logText, HandGestureState newState)
        //        {
        //            //string gestureStateOutput = "current: " + newState.ToString() + " previous: " +
        //            //    ((right) ? previousRightState.ToString() : previousLeftState.ToString()) + " rightdragselecttoggle: " + _leftDragSelectToggle + " ";
        //            //ILog(gestureStateOutput + logText, right);


        //            if (right)
        //            {
        //                //Debug.Log("_rightDragSelectToggle: " + _rightDragSelectToggle +
        //                //    " current: " + currentRightHandGestureState + 
        //                //    " new state: " + newState + 
        //                //    " previous state: " + previousRightState);
        //                //UpdateHandInputHelper(true, ref newState, ref _rightDragSelectToggle, ref currentRightHandGestureState, ref previousRightState);
        //                if (newState == HandGestureState.DragSelection &&
        //                    (previousRightState != HandGestureState.DragSelection && previousRightState != HandGestureState.Stop))
        //                {
        //                    if (_rightDragSelectToggle && !CheckForTextCubeDragLock(true))
        //                    {
        //                        currentRightHandGestureState = newState = HandGestureState.Stop;
        //                        previousRightState = HandGestureState.DragSelection;
        //                    }
        //                    _rightDragSelectToggle = !_rightDragSelectToggle;
        //                    modeActive = _rightDragSelectToggle;
        //                    ModeActiveStateChanged.I(modeActive);
        //                    ActivateNewState(newState, true);
        //                }
        //                else if (newState == HandGestureState.Stop)
        //                {
        //                    _rightDragSelectToggle = false;
        //                    ActivateNewState(newState, true);
        //                }
        //                else if (_rightDragSelectToggle)
        //                {
        //                    //if (newState == HandGestureState.Insert)
        //                    ActivateNewState(newState, true);
        //                    //else currentRightHandGestureState = newState;
        //                }
        //                else ActivateNewState(newState, true);
        //            }
        //            else
        //            {
        //                //UpdateHandInputHelper(false, ref newState, ref _leftDragSelectToggle, ref currentLeftHandGestureState, ref previousLeftState);
        //                if (newState == HandGestureState.DragSelection &&
        //                    (previousLeftState != HandGestureState.DragSelection && previousLeftState != HandGestureState.Stop))
        //                //(previousLeftState == HandGestureState.Selection || previousLeftState == HandGestureState.Neutral)) // made no dif
        //                {
        //                    if (_leftDragSelectToggle && !CheckForTextCubeDragLock(false))
        //                    {
        //                        currentLeftHandGestureState = newState = HandGestureState.Stop;
        //                        previousLeftState = HandGestureState.DragSelection;
        //                    }
        //                    _leftDragSelectToggle = !_leftDragSelectToggle;
        //                    modeActive = _leftDragSelectToggle;
        //                    ModeActiveStateChanged.I(modeActive);
        //                    ActivateNewState(newState, false);
        //                }
        //                else if (newState == HandGestureState.Stop)
        //                {
        //                    _leftDragSelectToggle = false;
        //                    ActivateNewState(newState, false);
        //                }
        //                else if (_leftDragSelectToggle)
        //                {
        //                    //if (newState == HandGestureState.Insert)
        //                    ActivateNewState(newState, false); // scale mode current: neutral prev: drag doesn't change to current: neutral prev: neutral - why not?
        //                    //else currentLeftHandGestureState = newState;
        //                }
        //                else ActivateNewState(newState, false);
        //            }
        //        }

        //        private void UpdateRotationState(bool right, ref CubeRotationState current, CubeRotationState previous)
        //        {
        //            RefreshTestVectors(right);
        //            //Debug.Log($"Update rotation state right? {right} current? {current} previous? {previous}, dot of inward and palmNormal: {Vector3.Dot(inward, currentPalmNormal)}," +
        //            //    $"dot of down and palmNormal: {Vector3.Dot(Vector3.down, currentPalmNormal)}, dot of up and palmNormal: {Vector3.Dot(Vector3.up, currentPalmNormal)}," +
        //            //    $"palmNormal: {currentPalmNormal.x} {currentPalmNormal.y} {currentPalmNormal.z}, inward: {inward.x} {inward.y} {inward.z}");
        //            if (Vector3.Dot(inward, currentPalmNormal) >= 0.9f && current != CubeRotationState.relax)
        //            {
        //                current = CubeRotationState.relax;
        //                BroadcastRotationStateChange(right, current, previous);
        //            }
        //            else if (Vector3.Dot(Vector3.down, currentPalmNormal) >= 0.33f && current != CubeRotationState.pronate)
        //            {
        //                current = CubeRotationState.pronate;
        //                BroadcastRotationStateChange(right, current, previous);
        //            }
        //            else if (Vector3.Dot(Vector3.up, currentPalmNormal) >= 0.33f && current != CubeRotationState.supinate)
        //            {
        //                current = CubeRotationState.supinate;
        //                BroadcastRotationStateChange(right, current, previous);
        //            }
        //#if !UNITY_EDITOR && !UNITY_STANDALONE
        //            else if (IsPointingWholeHandUpward(currentHandSkeleton, right, currentPalmNormal) && current != CubeRotationState.forward)
        //            {
        //                current = CubeRotationState.forward;
        //                BroadcastRotationStateChange(right, current, previous);
        //            }
        //            else if (IsHandPerpendicular(currentPalmNormal, currentHandSkeleton, right) && current != CubeRotationState.other) // palm backward
        //            {
        //                current = CubeRotationState.other;
        //                BroadcastRotationStateChange(right, current, previous);
        //            }
        //#endif
        //        }
        //#endregion loop state updates

#endregion loops and timers

#region event handlers
        public void IndexStateChanged(bool closed, bool right)
        {
            if (right) IndexStateChanged((closed) ? HandGestureState.Insert : HandGestureState.Neutral,
                ref RightStateChanged, ref currentRightHandGestureState, ref _previousRightState);
            else IndexStateChanged((closed) ? HandGestureState.Insert : HandGestureState.Neutral,
                ref LeftStateChanged, ref currentLeftHandGestureState, ref _previousLeftState);
            //Debug.Log("LeapHandInput " + ((right) ? "Right " : "Left ") + ((closed) ? "activated" : "deactivated"));
        }

        private void IndexStateChanged(
            HandGestureState newState,
            ref SpatialGestureEvent<HandGestureState, HandGestureState> trigger,
            ref HandGestureState currentState,
            ref HandGestureState previousState)
        {
            currentState = newState;
            trigger.I(currentState, previousState);
            previousState = newState;
            Debug.Log($"LeapHandInput just sent a trigger to SpatialGestureInput - newstate: {newState} previous: {previousState}");
        }

        private void TriggerGrab(bool right)
        {
            grabLogLoopRight?.Dispose();
            //Debug.Log($"QuestHandInput triggered {((right) ? "right" : "left")} hand grab.");
            IndexStateChanged(true, right);
            this.Wait(() => _releaseConditions(right), () => TriggerRelease(right));
        }

        private void TriggerRelease(bool right)
        {
            //Debug.Log($"QuestHandInput triggered {((right) ? "right" : "left")} hand release");
            IndexStateChanged(false, right);
            this.Wait(() => _grabConditions(right), () => TriggerGrab(right));
        }

        private void IndexPointingCheck(bool right)
        {
            this.Wait(() => _pointingTriggerConditions(right), () => IndexBeganPointing(right));
        }

        private void IndexBeganPointing(bool right)
        {
            Debug.Log($"Index point triggered @ time: {Time.time}");
            IndexPointing?.I(right);
            this.WaitS(_pointTriggerResetInterval, () => this.Wait(() => _pointingTriggerReady(right), () => IndexPointingCheck(right)));
        }

        private void BackwordTriggerCheck(bool right)
        {
            //if (!right)
            //    this.UpdateAsObservable().Subscribe(_ => {
            //        LogOutputReceiver.Instance.Log($"_palmOpMiddleDistance: {_palmOpMiddleDistance(false)}\n" +
            //            $"_backwordTriggerConditions met? {_backwordTriggerConditions(false)}");
            //    });
            this.Wait(() => _backwordTriggerConditions(right), () => OpMiddleToPalmTouch(right));
        }

        private void OpMiddleToPalmTouch(bool right)
        {
            //Debug.Log($"Opposite-middle-to-palm-touch triggered @ time: {Time.time}");
            //LogOutputReceiver.Instance.Log($"Opposite-middle-to-palm-touch triggered @ time: {Time.time}");
            OppositeMiddleToPalmTouch?.I(right);
            this.WaitS(_backwordTriggerResetInterval, () => this.Wait(() => _backwordTriggerReady(right), () => BackwordTriggerCheck(right)));
        }

        private int _loadTally = 0;
        private int _loadTime = 0;
        private float _countInterval = 0.5f;
        private void LoadReposition()
        {
            //LogOutputReceiver.Instance.Log("lodaing 2-finger-point");
            _isLoadingReposition = true;
            this.WaitS(_countInterval, () => {
                //LogOutputReceiver.Instance.Log($"About to process 2-finger-point. rHandConditions: {_pointingTriggerConditions(true)} " +
                //    $"lHandConditions: {_pointingTriggerConditions(false)}\n" +
                //    $"loadTally {_loadTally}\n" +
                //    $"loadTime {_loadTime}");
                if (_repositionConditions)
                {
                    if (_loadTally > _loadTime)
                    {
                        ResetPositionLoader();
                        RepositionUI();
                    }
                    else
                    {
                        _loadTally++;
                        LoadReposition();
                    }
                }
                else
                {
                    ResetPositionLoader();
                    this.Wait(() => _repositionConditions && !_isLoadingReposition, () => LoadReposition());
                }
            });
        }

        private void ResetPositionLoader()
        {
            _loadTally = 0;
            _isLoadingReposition = false;
        }
        private void RepositionUI()
        {
            IndexFingersAtEachOther?.I();
            this.WaitS(1, () => {
                //LogOutputReceiver.Instance.Log("Awaiting pointer fingers\npointing at each other...");
                this.Wait(() => _repositionConditions && !_isLoadingReposition, () => LoadReposition());
            });
        }

        private void RotationStateCheckLoop(bool right)
        {
            // Do not name the Subscribe parameter "_" — "out _" in this lambda would bind to UniRx.Unit (CS1503).
            this.UpdateAsObservable().Subscribe(__ => {
                if (!Lexting.controllers.Ready())
                    return;

                var palmNormal = _palmNormal(right);
                if (palmNormal.sqrMagnitude < 1e-6f)
                    return;

                if (QuestHandRotationMapping.TryClassify(
                        palmNormal,
                        Lexting.controllers.transform,
                        right,
                        out var targetState,
                        out var rotationDots))
                    RefreshRotationState(targetState, right, rotationDots);
            });
        }

        private void RefreshRotationState(
            CubeRotationState targetState,
            bool right,
            QuestHandRotationMapping.DotSample? rotationDots = null)
        {
            if (currentRotationState(right) == targetState)
                return;

            var previous = currentRotationState(right);
            SetCurrentRotationState(right, targetState);
            TriggerRotationStateChange(right);
            SetPreviousRotationState(right, targetState);

#if !UNITY_STANDALONE && UNITY_ANDROID
            if (QuestHandRotationDebugSettings.Enabled && QuestHandRotationDebugSettings.LogOnStateChange)
            {
                var dots = rotationDots ?? QuestHandRotationMapping.Measure(
                    _palmNormal(right),
                    Lexting.controllers.transform,
                    right);
                QuestHandLog.Info(
                    $"[RotDebug] {(right ? "R" : "L")} {previous} -> {targetState} | bind={_handNodeBindingMode} | {dots}");
            }
#endif
        }
        //private void OnTutorialCompleted()
        //{
        //    tutorialComplete = true;
        //}
        #endregion event handlers

        #region helper methods
        //        /// <summary>
        //        /// Refactors code duplicated for each hand in the UpdatHandInput Method - buggy -
        //        /// come back to complete this refactor when there's time
        //        /// </summary>
        //        private void UpdateHandInputHelper(bool right, ref HandGestureState newState, ref bool dragSelectToggle, ref HandGestureState current, ref HandGestureState previous)
        //        {
        //            if (newState == HandGestureState.DragSelection &&
        //                    (previous != HandGestureState.DragSelection && previous != HandGestureState.Stop))
        //            {
        //                if (dragSelectToggle && !CheckForTextCubeDragLock(right))
        //                {
        //                    current = newState = HandGestureState.Stop;
        //                    previous = HandGestureState.DragSelection;
        //                }
        //                dragSelectToggle = !dragSelectToggle;
        //                modeActive = dragSelectToggle;
        //                ModeActiveStateChanged.I(modeActive);
        //                ActivateNewState(newState, right);
        //            }
        //            else if (newState == HandGestureState.Stop)
        //            {
        //                dragSelectToggle = false;
        //                ActivateNewState(newState, right);
        //            }
        //            else if (dragSelectToggle)
        //            {
        //                //if (newState == HandGestureState.Insert)
        //                ActivateNewState(newState, right);
        //                //else 
        //                current = newState;
        //            }
        //            else ActivateNewState(newState, right);
        //        }

        //        private void BroadcastRotationStateChange(bool right, CubeRotationState current, CubeRotationState previous)
        //        {
        //            if (current == previous) return;
        //            Debug.Log($"About to broadcast rotation state change: right {right} current {current} previous {previous}");
        //            if (right)
        //                RightRotationStateChanged.I(current, previous);
        //            else LeftRotationStateChanged.I(current, previous);
        //        }

        //        ///// <summary>
        //        ///// Refactors duplicate code for each hand in the update loop used to check
        //        ///// whether a state change is needed this frame
        //        ///// </summary>
        //        //private void CheckForUpdates(bool right, ref HandGestureState current, ref HandGestureState previous, ref OVRHand hand, ref OVRSkeleton skeleton)
        //        //{
        //        //    string handName = (right) ? "Right" : "Left";
        //        //    previous = current;
        //        //    if (!hand.IsTracked)
        //        //        UpdateHandInput(true, handName + " not found", HandGestureState.Neutral);
        //        //    else if (IsFistClosedThumbOut(skeleton))
        //        //        UpdateHandInput(true, handName + " fist closed thumb out", HandGestureState.Stop);
        //        //    else if (IsFistClosed(skeleton))
        //        //        UpdateHandInput(true, handName + " fist closed", HandGestureState.SqueezeAll);
        //        //    else if (hand.GetFingerIsPinching(OVRHand.HandFinger.Index))
        //        //        UpdateHandInput(true, handName + " Index Finger is pinching", HandGestureState.Insert);
        //        //    else if (hand.GetFingerIsPinching(OVRHand.HandFinger.Middle))
        //        //        UpdateHandInput(true, handName + " Middle Finger is pinching", HandGestureState.DragSelection);
        //        //    else if (IsPointingUpward(skeleton, true))
        //        //        UpdateHandInput(true, handName + " pointing upward", HandGestureState.DeselectAll);
        //        //    else if (IsPointingWithPalmDown(skeleton, true))
        //        //        UpdateHandInput(true, handName + " hand pointing", HandGestureState.Deselection);
        //        //    else if (IsPalmFlatDownThumbOut(skeleton, true))
        //        //        UpdateHandInput(true, handName + " palm is flat and thumb is out", HandGestureState.Selection);
        //        //    else UpdateHandInput(true, handName + " neutral", HandGestureState.Neutral);
        //        //}

        //        private void ActivateNewState(HandGestureState newState, bool right)
        //        {
        //            //if (newState == HandGestureState.Insert)
        //            //    Debug.Log("ActivateNewState - about to activate " + newState + " for " + ((right) ? "right " : "left ") + "hand.");
        //            if (right)
        //            {
        //                currentRightHandGestureState = newState;
        //                if (currentRightHandGestureState != previousRightState)
        //                {
        //                    RightStateChanged.I(currentRightHandGestureState, previousRightState);
        //                    Debug.Log("OpenXRHandInput: RightStateChange invoked - newState: " + newState + " previousState: " + previousRightState);
        //                }    
        //            }
        //            else {
        //                currentLeftHandGestureState = newState;
        //                if (currentLeftHandGestureState != previousLeftState)
        //                {
        //                    LeftStateChanged.I(currentLeftHandGestureState, previousLeftState);
        //                    Debug.Log("OpenXRHandInput: LeftStateChange invoked - newState: " + newState + " previousState: " + previousLeftState);
        //                }

        //            }
        //        }

        //        /// <summary>
        //        /// prevents this class from invoking a system-wide state change until
        //        /// the new state is confirmed to stick across a certain number of frames
        //        /// -- this stops single-frame hand-tracking noise (which is rampant on Quest)
        //        /// from causing strobe-flickering state changes
        //        /// </summary>
        //        private IEnumerator ClearFalsePositives(bool right)
        //        {
        //            int stickThreshold = 5;

        //            HandGestureState startingState = (right) ? currentRightHandGestureState : currentLeftHandGestureState;
        //            HandGestureState previousState = (right) ? previousRightState : previousLeftState;

        //            if (startingState == HandGestureState.DragSelection)
        //                stickThreshold *= 2;

        //            for (int i = 0; i < stickThreshold; i++)
        //            {
        //                yield return new WaitForEndOfFrame();
        //                if (startingState != ((right) ? currentRightHandGestureState : currentLeftHandGestureState))
        //                    yield break;
        //            }
        //            if (right) RightStateChanged.I(startingState, previousState);
        //            else LeftStateChanged.I(startingState, previousState);
        //        }

        //        ///// <summary>
        //        ///// supports functionality of the ClearFalsePositives method for right hand
        //        ///// </summary>
        //        //private IEnumerator ResetRight()
        //        //{
        //        //    yield return new WaitForSeconds(0.1f);
        //        //    _rightReady = true;
        //        //}

        //        ///// <summary>
        //        ///// supports functionality of the ClearFalsePositives method for left hand
        //        ///// </summary>
        //        //private IEnumerator ResetLeft()
        //        //{
        //        //    yield return new WaitForSeconds(0.1f);
        //        //    _leftReady = true;
        //        //}

        //#region state analysis
        //        private bool IsFistClosed(OVRSkeleton hand)
        //        {
        //            //float indexDistanceThreshold = 0.11f;
        //            //float middleDistanceThreshold = 0.11f;
        //            //float ringDistanceThreshold = 0.08f;
        //            //float pinkyDistanceThreshold = 0.08f;
        //            //float thumbDistanceThreshold = 0.05f;
        //            return
        //                (hand.Bones[(int)BoneId.Hand_IndexTip].Transform.position - hand.Bones[(int)BoneId.Hand_WristRoot].Transform.position).magnitude < indexDistanceThreshold &&
        //                (hand.Bones[(int)BoneId.Hand_MiddleTip].Transform.position - hand.Bones[(int)BoneId.Hand_WristRoot].Transform.position).magnitude < middleDistanceThreshold &&
        //                (hand.Bones[(int)BoneId.Hand_RingTip].Transform.position - hand.Bones[(int)BoneId.Hand_WristRoot].Transform.position).magnitude < ringDistanceThreshold &&
        //                (hand.Bones[(int)BoneId.Hand_PinkyTip].Transform.position - hand.Bones[(int)BoneId.Hand_WristRoot].Transform.position).magnitude < pinkyDistanceThreshold &&
        //                (hand.Bones[(int)BoneId.Hand_ThumbTip].Transform.position - hand.Bones[(int)BoneId.Hand_Index2].Transform.position).magnitude < thumbDistanceThreshold;
        //        }

        //        private bool IsFistClosedThumbOut(OVRSkeleton hand)
        //        {
        //            float indexDistanceThreshold = 0.11f;
        //            float middleDistanceThreshold = 0.11f;
        //            float ringDistanceThreshold = 0.08f;
        //            float pinkyDistanceThreshold = 0.08f;
        //            float thumbDistanceThreshold = 0.05f;
        //            return
        //                (hand.Bones[(int)BoneId.Hand_IndexTip].Transform.position - hand.Bones[(int)BoneId.Hand_WristRoot].Transform.position).magnitude < indexDistanceThreshold &&
        //                (hand.Bones[(int)BoneId.Hand_MiddleTip].Transform.position - hand.Bones[(int)BoneId.Hand_WristRoot].Transform.position).magnitude < middleDistanceThreshold &&
        //                (hand.Bones[(int)BoneId.Hand_RingTip].Transform.position - hand.Bones[(int)BoneId.Hand_WristRoot].Transform.position).magnitude < ringDistanceThreshold &&
        //                (hand.Bones[(int)BoneId.Hand_PinkyTip].Transform.position - hand.Bones[(int)BoneId.Hand_WristRoot].Transform.position).magnitude < pinkyDistanceThreshold &&
        //                (hand.Bones[(int)BoneId.Hand_ThumbTip].Transform.position - hand.Bones[(int)BoneId.Hand_Index2].Transform.position).magnitude > thumbDistanceThreshold;
        //        }

        //        private bool IsPointingWhileThumbIsUp(OVRSkeleton hand)
        //        {
        //            Vector3 thumbDirection = (hand.Bones[(int)BoneId.Hand_ThumbTip].Transform.position - hand.Bones[(int)BoneId.Hand_Thumb1].Transform.position).normalized;
        //            Vector3 indexDirection = (hand.Bones[(int)BoneId.Hand_IndexTip].Transform.position - hand.Bones[(int)BoneId.Hand_Index1].Transform.position).normalized;
        //            Vector3 middleDirection = (hand.Bones[(int)BoneId.Hand_MiddleTip].Transform.position - hand.Bones[(int)BoneId.Hand_Middle1].Transform.position).normalized;
        //            float thumbIndexComparison = Vector3.Dot(thumbDirection, indexDirection);
        //            float indexMiddleComparison = Vector3.Dot(indexDirection, middleDirection);
        //            return thumbIndexComparison > 0.1f &&
        //                thumbIndexComparison < 0.5f &&
        //                indexMiddleComparison < -0.3f &&
        //                Vector3.Dot(thumbDirection, Vector3.up) > 0.6f;
        //        }

        //        private bool IsPointingWithPalmDown(OVRSkeleton hand, bool right)
        //        {
        //            Vector3 palmDownTest = (hand.Bones[(int)BoneId.Hand_Index1].Transform.position - hand.Bones[(int)BoneId.Hand_Pinky1].Transform.position).normalized;
        //            Vector3 indexDirection = (hand.Bones[(int)BoneId.Hand_IndexTip].Transform.position - hand.Bones[(int)BoneId.Hand_Index1].Transform.position).normalized;
        //            Vector3 middleDirection = (hand.Bones[(int)BoneId.Hand_MiddleTip].Transform.position - hand.Bones[(int)BoneId.Hand_Middle1].Transform.position).normalized;
        //            float indexMiddleComparison = Vector3.Dot(indexDirection, middleDirection);
        //            float palmDownTestComparison = Vector3.Distance(palmDownTest, Vector3.up);
        //            return 
        //                indexMiddleComparison < 0 &&
        //                (palmDownTestComparison > 0.5f || palmDownTestComparison < -0.5f);
        //        }

        //        private bool IsPointingWholeHandUpward(OVRSkeleton hand, bool right, Vector3 palmNormal)
        //        {
        //            Vector3 middleDirection = (hand.Bones[(int)BoneId.Hand_MiddleTip].Transform.position - hand.Bones[(int)BoneId.Hand_Middle1].Transform.position).normalized;
        //            Vector3 ringDirection = (hand.Bones[(int)BoneId.Hand_RingTip].Transform.position - hand.Bones[(int)BoneId.Hand_Ring1].Transform.position).normalized;
        //            return 
        //                Vector3.Dot(Vector3.up, middleDirection) > 0.7f && // middle finger up
        //                Vector3.Dot(Vector3.up, ringDirection) > 0.7f && //ring finger up
        //                Vector3.Dot(palmNormal, _cubeControllerTransform.forward) > 0.7f; // palm forward
        //        }

        //        private bool IsPointingUpward(OVRSkeleton hand, bool right)
        //        {
        //            Vector3 indexDirection = (hand.Bones[(int)BoneId.Hand_IndexTip].Transform.position - hand.Bones[(int)BoneId.Hand_Index1].Transform.position).normalized;
        //            Vector3 middleDirection = (hand.Bones[(int)BoneId.Hand_MiddleTip].Transform.position - hand.Bones[(int)BoneId.Hand_Middle1].Transform.position).normalized;
        //            float indexMiddleComparison = Vector3.Dot(indexDirection, middleDirection);

        //            return indexMiddleComparison < 0 && Vector3.Dot(indexDirection,Vector3.up) > 0.7f;
        //        }

        //        private bool IsPalmFlatWhileThumbIsUp(OVRSkeleton hand)
        //        {
        //            Vector3 thumbDirection = (hand.Bones[(int)BoneId.Hand_ThumbTip].Transform.position - hand.Bones[(int)BoneId.Hand_Thumb1].Transform.position).normalized;
        //            Vector3 indexDirection = (hand.Bones[(int)BoneId.Hand_IndexTip].Transform.position - hand.Bones[(int)BoneId.Hand_Index1].Transform.position).normalized;
        //            Vector3 middleDirection = (hand.Bones[(int)BoneId.Hand_MiddleTip].Transform.position - hand.Bones[(int)BoneId.Hand_Middle1].Transform.position).normalized;
        //            float thumbIndexComparison = Vector3.Dot(thumbDirection, indexDirection);
        //            float indexMiddleComparison = Vector3.Dot(indexDirection, middleDirection);
        //            return thumbIndexComparison > 0.1f && 
        //                thumbIndexComparison < 0.5f &&
        //                indexMiddleComparison > 0.7f &&
        //                Vector3.Dot(thumbDirection, Vector3.up) > 0.6f;
        //        }

        //        private bool IsPalmFlatDownThumbOut(OVRSkeleton hand, bool right)
        //        {
        //            Vector3 palmDownTest = (hand.Bones[(int)BoneId.Hand_Index1].Transform.position - hand.Bones[(int)BoneId.Hand_Pinky1].Transform.position).normalized;
        //            Vector3 indexDirection = (hand.Bones[(int)BoneId.Hand_IndexTip].Transform.position - hand.Bones[(int)BoneId.Hand_Index1].Transform.position).normalized;
        //            Vector3 middleDirection = (hand.Bones[(int)BoneId.Hand_MiddleTip].Transform.position - hand.Bones[(int)BoneId.Hand_Middle1].Transform.position).normalized;
        //            float indexMiddleComparison = Vector3.Dot(indexDirection, middleDirection);
        //            float palmDownTestComparison = Vector3.Distance(palmDownTest, Vector3.up);
        //            return 
        //                indexMiddleComparison > 0.7f &&
        //                (palmDownTestComparison > 0.5f || palmDownTestComparison < -0.5f);
        //        }


        //        private bool IsHandPerpendicular(Vector3 palmNormal, OVRSkeleton hand, bool right)
        //        {
        //            bool palmIsFacingStomach = Vector3.Dot(_cubeControllerTransform.forward, palmNormal) < -0.33f;
        //            Vector3 palmRootToTopDirection = (hand.Bones[(int)BoneId.Hand_Middle1].Transform.position - hand.Bones[(int)BoneId.Hand_WristRoot].Transform.position).normalized;
        //            float uiPerpendicularDirection = (right) ? -1 : 1;
        //            bool wristAngledInward = Vector3.Dot(uiPerpendicularDirection * _cubeControllerTransform.right, palmRootToTopDirection) > 0.33f;
        //            //Debug.Log("IsHandPerpendicular? -" +
        //            //    "is palm facing stomach reading less than 0.5? " + Vector3.Dot(_cubeControllerTransform.forward, palmNormal) +
        //            //    " is wrist angled inware reading greater than 0.5? " + Vector3.Dot(uiPerpendicularDirection * _cubeControllerTransform.right, palmRootToTopDirection));
        //            return palmIsFacingStomach && wristAngledInward;
        //        }

        //        private bool IsNeutral(OVRHand hand, OVRSkeleton handSkeleton)
        //        {
        //            return !hand.GetFingerIsPinching(OVRHand.HandFinger.Index) &&
        //                !hand.GetFingerIsPinching(OVRHand.HandFinger.Middle) &&
        //                !IsFistClosed(handSkeleton) &&
        //                !IsPalmFlatWhileThumbIsUp(handSkeleton);
        //        }

        //        /// <summary>
        //        /// Refresh all data references in preparation to check for pop gun gestures
        //        /// </summary>
        //        /// <param name="right"></param>
        //        /// <param name="sixGunPointerConfidence">float range 0.0 to 1.0 where 1.0 is highest confidence of pointer finger ready</param>
        //        /// <param name="middleClosed">if true, the middle finger is close enough to the base of the wrist to be considered closed</param>
        //        /// <param name="ringClosed">if true, the ring finger is close enough to the base of the wrist to be considered closed</param>
        //        private void PopGunPrep(bool right, ref float sixGunPointerConfidence,ref bool middleClosed, ref bool ringClosed)
        //        {
        //#if !UNITY_EDITOR && !UNITY_STANDALONE
        //            RefreshTestVectors(right);
        //            sixGunPointerConfidence = Vector3.Dot((currentHandSkeleton.Bones[(int)BoneId.Hand_IndexTip].Transform.position
        //                - currentHandSkeleton.Bones[(int)BoneId.Hand_Index1].Transform.position).normalized,
        //                (TutorialManager.Instance.transform.position - Lexting.head.transform.position).normalized);
        //            middleClosed = (currentHandSkeleton.Bones[(int)BoneId.Hand_MiddleTip].Transform.position
        //                - currentHandSkeleton.Bones[(int)BoneId.Hand_WristRoot].Transform.position).magnitude < middleDistanceThreshold;
        //            ringClosed = (currentHandSkeleton.Bones[(int)BoneId.Hand_RingTip].Transform.position
        //                - currentHandSkeleton.Bones[(int)BoneId.Hand_WristRoot].Transform.position).magnitude < ringDistanceThreshold;
        //#endif
        //        }

        //        private bool HandHasPopGunGesture(bool right)
        //        {


        //#if !UNITY_EDITOR && !UNITY_STANDALONE
        //            if (_isInputStream) return false;
        //            //if (!right)
        //            //{
        //            //    return true;
        //            //}
        //            //else return false;

        //            float sixGunPointerConfidence = 0;
        //            bool middleClosed = false, ringClosed = false;
        //            PopGunPrep(right, ref sixGunPointerConfidence, ref middleClosed, ref ringClosed);
        //            float thumbConfidence = Vector3.Dot((currentHandSkeleton.Bones[(int)BoneId.Hand_ThumbTip].Transform.position
        //                - currentHandSkeleton.Bones[(int)BoneId.Hand_Thumb0].Transform.position).normalized, Vector3.up);

        //            // for debugging only
        //            //if (!right)
        //            //    PrintPopgunDiagnostics(sixGunPointerConfidence, thumbConfidence);

        //            return sixGunPointerConfidence > 0.7f && thumbConfidence > 0.7f && middleClosed && ringClosed;
        //#else
        //            return false;
        //#endif
        //        }

        //        private bool HasPopGunGesturesThumbsIn()
        //        {
        //#if !UNITY_EDITOR && !UNITY_STANDALONE
        //            if (_isInputStream) return false;
        //            float sixGunPointerConfidence = 0;
        //            bool middleClosed = false, ringClosed = false;
        //            PopGunPrep(true, ref sixGunPointerConfidence, ref middleClosed, ref ringClosed);
        //            bool rightGunReady = sixGunPointerConfidence > 0.7f && middleClosed && ringClosed;
        //            PopGunPrep(false, ref sixGunPointerConfidence, ref middleClosed, ref ringClosed);
        //            bool leftGunReady = sixGunPointerConfidence > 0.7f && middleClosed && ringClosed;
        //            float thumbsConfidence = Vector3.Dot((_rightHandSkeleton.Bones[(int)BoneId.Hand_Thumb0].Transform.position
        //                - _rightHandSkeleton.Bones[(int)BoneId.Hand_ThumbTip].Transform.position).normalized,
        //                (_leftHandSkeleton.Bones[(int)BoneId.Hand_Thumb0].Transform.position
        //                - _leftHandSkeleton.Bones[(int)BoneId.Hand_ThumbTip].Transform.position).normalized);



        //            return rightGunReady && leftGunReady && thumbsConfidence < 0.3f;
        //#else
        //            return false;
        //#endif
        //        }

        //        private void RefreshTestVectors(bool right)
        //        {


        //#if !UNITY_EDITOR && !UNITY_STANDALONE
        //            inward = (right)
        //                        ? Vector3.Cross(Lexting.head.flatLookDirection, Vector3.up).normalized
        //                        : Vector3.Cross(Vector3.up, Lexting.head.flatLookDirection).normalized;
        //            //inward = ((right) ? -1 : 1) * (_rightHandSkeleton.transform.position - _leftHandSkeleton.transform.position).normalized;
        //            currentPalmNormal = (right) ? Lexting.rHand.palmNormal : Lexting.lHand.palmNormal;
        //            currentHandSkeleton = (right) ? _rightHandSkeleton : _leftHandSkeleton;
        //#endif
        //        }

        //        private void PrintPopgunDiagnostics(float sixGunPointerConfidence, float thumbConfidence)
        //        {
        //#if !UNITY_EDITOR && !UNITY_STANDALONE
        //            Debug.Log("Popgun Full Diagnostics: " +
        //                $"sixgunPointerConfidence: {sixGunPointerConfidence} needs to be above 0.7f, " +
        //                $"middleClosedValue: " +
        //                $"{(currentHandSkeleton.Bones[(int)BoneId.Hand_MiddleTip].Transform.position - currentHandSkeleton.Bones[(int)BoneId.Hand_WristRoot].Transform.position).magnitude}" +
        //                $" needs to be under threshold: {middleDistanceThreshold}, " +
        //                $"ringClosedValue: " +
        //                $"{(currentHandSkeleton.Bones[(int)BoneId.Hand_RingTip].Transform.position - currentHandSkeleton.Bones[(int)BoneId.Hand_WristRoot].Transform.position).magnitude}" +
        //                $" needs to be under thresdhold: {ringDistanceThreshold}, " +
        //                $"thumbConfidence: {thumbConfidence} needs to be above 0.7f");
        //#endif
        //        }
        //#endregion state analysis

        #endregion helper methods

        #region debugging

    public void UnitTest_RightStateChanged()
        {
            RightStateChanged?.I(HandGestureState.Insert, HandGestureState.Neutral);
        }

        public void UnitTest_Reposition()
        {
            IndexFingersAtEachOther?.I();
        }

        public void UnitTest_GrabVisualization()
        {
#if !UNITY_ANDROID && LEXTING_FOUND_OCULUS_SDK
            GameObject palmViz = CreateVisualizerObject("palmVisualizer", 0.02f);
            GameObject mTipViz = CreateVisualizerObject("middleFingerVisualizer", 0.02f);
            this.UpdateAsObservable().Subscribe(_=> {
                palmViz.transform.position = Lexting.input.leftHand.palm.position;
                mTipViz.transform.position = Lexting.input.leftHand.middleTip.position;
                palmViz.transform.rotation = Lexting.input.leftHand.palm.rotation;
                mTipViz.transform.rotation = Lexting.input.leftHand.middleTip.rotation;
            });
#endif
        }

        public GameObject CreateVisualizerObject(string name, float scale)
        {
            GameObject result = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(result.GetComponent<Collider>());
            result.transform.localScale = new Vector3(scale, scale, scale);
            result.name = name;
            return result;
        }

        //        public void ActivateInEditor()
        //        {
        //            UpdateHandInput(true, "User pressed space bar to simulate finger tap...", HandGestureState.Insert);
        //            this.WaitS(0.1f,()=>UpdateHandInput(true, "Space bar auto-release...", HandGestureState.Neutral));
        //        }

        //        private void LogAllBoneIds()
        //        {
        //            string boneList = "R: ";
        //            int count = 0;
        //            foreach (OVRBone bone in _rightHandSkeleton.Bones)
        //            {
        //                boneList += count + " " + bone.Id + ", ";
        //                count++;
        //            }
        //            ILog(boneList, true);
        //            boneList = "L: ";
        //            count = 0;
        //            foreach (OVRBone bone in _leftHandSkeleton.Bones)
        //            {
        //                boneList += count + " " + bone.Id + ", ";
        //                count++;
        //            }
        //            ILog(boneList, false);
        //        }

        //        /// <summary>
        //        /// Enables diagnostics of the distance between two hand point positions for several fingers,
        //        /// which allows unit tests to capture accurate thresholds for measuring whether the fist is closed.
        //        /// Potential threshold values are printed to the console
        //        /// which can be used to set the thresholds in the IsFistClosed method accordingly.
        //        /// </summary>
        //        private void CalibrateFistClosureThresholds()
        //        {
        //#if !UNITY_EDITOR && !UNITY_STANDALONE
        //            int firstBone = 0;
        //            int secondBone = 0;
        //            switch (debugIterator % 5)
        //            {
        //                case 0:
        //                    firstBone = (int)BoneId.Hand_IndexTip;
        //                    secondBone = (int)BoneId.Hand_WristRoot;
        //                    break;
        //                case 1:
        //                    firstBone = (int)BoneId.Hand_MiddleTip;
        //                    secondBone = (int)BoneId.Hand_WristRoot;
        //                    break;
        //                case 2:
        //                    firstBone = (int)BoneId.Hand_RingTip;
        //                    secondBone = (int)BoneId.Hand_WristRoot;
        //                    break;
        //                case 3:
        //                    firstBone = (int)BoneId.Hand_PinkyTip;
        //                    secondBone = (int)BoneId.Hand_WristRoot;
        //                    break;
        //                case 4:
        //                    firstBone = (int)BoneId.Hand_ThumbTip;
        //                    secondBone = (int)BoneId.Hand_Index2;
        //                    break;
        //            }

        //            string output = (debugIterator % 5).ToString() + " " +
        //                (_rightHandSkeleton.Bones[firstBone].Transform.position - _rightHandSkeleton.Bones[secondBone].Transform.position).magnitude.ToString();
        //            ILog(output,true);
        //#endif
        //        }

        //        /// <summary>
        //        /// Compare direction of index finger to direction of thumb
        //        /// </summary>
        //        private void LogFingerDirections()
        //        {
        //#if !UNITY_EDITOR && !UNITY_STANDALONE
        //            Vector3 rightIndexDirection = (_rightHandSkeleton.Bones[20].Transform.position - _rightHandSkeleton.Bones[6].Transform.position).normalized;
        //            Vector3 leftIndexDirection = (_leftHandSkeleton.Bones[20].Transform.position - _leftHandSkeleton.Bones[6].Transform.position).normalized;
        //            Vector3 rightThumbDirection = (_rightHandSkeleton.Bones[19].Transform.position - _rightHandSkeleton.Bones[3].Transform.position).normalized;
        //            Vector3 leftThumbDirection = (_leftHandSkeleton.Bones[19].Transform.position - _leftHandSkeleton.Bones[3].Transform.position).normalized;

        //            ILog("Right Hand " + Vector3.Dot(rightIndexDirection, rightThumbDirection) + " ", true);
        //            ILog("Left Hand " + Vector3.Dot(leftIndexDirection, leftThumbDirection),false);
        //#endif
        //        }

        //        /// <summary>
        //        /// handler for all console output for hand input
        //        /// </summary>
        //        private void ILog(string text, bool right)
        //        {
        //            if (_debuggingEnabled)
        //            {
        //                //if (right)
        //                //    rightDebugOutput.text = text;
        //                //else leftDebugOutput.text = text;
        //                //Debug.Log(text);
        //            }
        //        }

        //        /// <summary>
        //        /// If we're in text mode and the hand is currently using the cube,
        //        /// we need to prevent hand input from accidentally escaping drag selection (very common due to tracking noise)
        //        /// until the hand exits the cube boundary (see CubeBoundary.cs)
        //        /// </summary>
        //        private bool CheckForTextCubeDragLock(bool right)
        //        {
        //            if (!_isInputStream)
        //            {
        //                _textCubeActive = LextingController.Instance.textCubeActive;
        //                _rightHandInBounds = LextingController.Instance.rightHandInBounds;
        //                _leftHandInBounds = LextingController.Instance.leftHandInBounds;
        //            }
        //            else
        //            {
        //                _textCubeActive = true;
        //                _rightHandInBounds = true;
        //                _leftHandInBounds = true;
        //            }

        //            return _textCubeActive && ((right && _rightHandInBounds) || (!right && _leftHandInBounds));
        //        }
#endregion debugging
    }
}

