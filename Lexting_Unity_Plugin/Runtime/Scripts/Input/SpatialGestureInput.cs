using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpatialGestureText.LetterCube;
using SpatialGestureText.Tutorial;
using SpatialGestureText.Zones;
using UnityEngine;
using UnityEngine.Events;
using UniRx;
using UniRx.Triggers;
using static EventUtils;
using SpatialGestureText.Utilities;
using SpatialGestureText.Debugging;

namespace SpatialGestureText.Input
{
    /// <summary>
    /// Consolidates all Lexting input modules into a single set of actions (an action map from before there were action maps)
    /// including input from Oculus Touch, Oculus Hands, ... (add others in future)
    /// </summary>
    public class SpatialGestureInput : Singleton<SpatialGestureInput>
    {
        #region vars
        public bool modeActive;
        public bool inputActive = true;
        public bool handsConfident = true;

        public LextingHand rightHand;
        public LextingHand leftHand;
        public HandMarker rightMarker;
        public HandMarker leftMarker;

        public LextingHand hand(bool right) => right ? rightHand : leftHand;

        [HideInInspector] public SpatialGestureEvent<bool> OnModeActiveStateChanged = new SpatialGestureEvent<bool>();
        [HideInInspector] public SpatialGestureEvent<HandGestureState, HandGestureState> OnRightStateChanged = new SpatialGestureEvent<HandGestureState, HandGestureState>();
        [HideInInspector] public SpatialGestureEvent<HandGestureState, HandGestureState> OnLeftStateChanged = new SpatialGestureEvent<HandGestureState, HandGestureState>();
        [HideInInspector] public SpatialGestureEvent<bool, CubeTouchStates.TouchStateIndex> OnHoldAction = new SpatialGestureEvent<bool, CubeTouchStates.TouchStateIndex>();
        [HideInInspector] public SpatialGestureEvent<CubeRotationState, CubeRotationState> OnLeftRotationStateChanged = new SpatialGestureEvent<CubeRotationState, CubeRotationState>();
        [HideInInspector] public SpatialGestureEvent<CubeRotationState, CubeRotationState> OnRightRotationStateChanged = new SpatialGestureEvent<CubeRotationState, CubeRotationState>();
        [HideInInspector] public SpatialGestureEvent<bool, bool> OnSinglePopGunActive = new SpatialGestureEvent<bool, bool>();
        [HideInInspector] public SpatialGestureEvent< bool> OnHandTrackingToggled = new SpatialGestureEvent<bool>();
        [HideInInspector] public SpatialGestureEvent<bool> OnHandConfidenceToggled = new SpatialGestureEvent<bool>();
        [HideInInspector] public SpatialGestureEvent<bool> OnThumbOut = new SpatialGestureEvent<bool>();
        [HideInInspector] public SpatialGestureEvent<bool> OnIndexPointing = new SpatialGestureEvent<bool>();
        [HideInInspector] public SpatialGestureEvent<bool> OnIndexThumbTouch = new SpatialGestureEvent<bool>();
        [HideInInspector] public SpatialGestureEvent<bool> OnOppositeMiddleToPalmTouch = new SpatialGestureEvent<bool>();
        [HideInInspector] public SpatialGestureEvent OnIndexFingersAtEachOther = new SpatialGestureEvent();

        [HideInInspector] public SpatialGestureEvent<bool> OnDualPopGunActive = new SpatialGestureEvent<bool>();
        [HideInInspector] public HandGestureState currentRightHandGestureState, currentLeftHandGestureState;
        [HideInInspector] public CubeRotationState currentRightRotationState, currentLeftRotationState;

        [HideInInspector] public HandGestureState currentState(bool right) => right ? currentRightHandGestureState : currentLeftHandGestureState;
        [HideInInspector] public CubeRotationState currentRotationState(bool right) => right ? currentRightRotationState : currentLeftRotationState;

        /// <summary>
        /// final draft input senders of type where right is true, left is false, 1st hand state is current, 2nd is previous 
        /// </summary>
        [HideInInspector] public SpatialGestureEvent<bool, HandGestureState, HandGestureState> OnStateChanged = new SpatialGestureEvent<bool, HandGestureState, HandGestureState>();
        [HideInInspector] public SpatialGestureEvent<bool, CubeRotationState, CubeRotationState> OnRotationStateChanged = new SpatialGestureEvent<bool, CubeRotationState, CubeRotationState>();

        /// <summary>
        /// Touch Input Hold States - each array has 3 values, one for each of the 3 primary fingers
        /// </summary>
        public bool[] rightHeldLong, leftHeldLong;

        public bool singleRightPopGunActivated, singleLeftPopGunActivated;

        private bool tutorialComplete;

        [SerializeField] private bool _isInputStream;
        #endregion vars

        #region init
        private void Awake()
        {
            //this.WaitS(3,()=>PrintHandStatus(0.1f));

            

            //this.Wait(() => OculusTouchInputTest.Instance.Ready(), () => {
            //    this.Subscribe(
            //        OculusTouchInputTest.Instance.RightStateChanged, RightStateChangedFromTouchInput,
            //        OculusTouchInputTest.Instance.LeftStateChanged, LeftStateChangedFromTouchInput
            //    );
            //    this.Subscribe(OculusTouchInputTest.Instance.RightRotationStateChanged, RightRotationStateChangedFromHandInput);
            //    this.Subscribe(OculusTouchInputTest.Instance.LeftRotationStateChanged, LeftRotationStateChangedFromHandInput);
            //    this.Subscribe(OculusTouchInputTest.Instance.HoldAction, HoldAction);
            //    this.Subscribe(OculusTouchInputTest.Instance.RefreshHeldLongState, RefreshHeldLongState);
            //});

#if LEXTING_FOUND_LEAP_SDK
            if (Lexting.inputManager.isAnyLeapMode)
            {
                this.WaitF(1, () => {
                    this.Wait(() => Lexting.inputManager.leftMarker.Ready(), () => {
                        this.Subscribe(LeapHandInput.Instance.RightStateChanged, RightStateChangedFromHandInput,
                                        LeapHandInput.Instance.LeftStateChanged, LeftStateChangedFromHandInput);
                        this.Subscribe(LeapHandInput.Instance.RightRotationStateChanged, RightRotationStateChangedFromHandInput);
                        this.Subscribe(LeapHandInput.Instance.LeftRotationStateChanged, LeftRotationStateChangedFromHandInput);
                        this.Subscribe(LeapHandInput.Instance.ThumbOut, ThumbOut);
                        this.Subscribe(LeapHandInput.Instance.IndexPointing, IndexPointing);
                        this.Subscribe(LeapHandInput.Instance.IndexThumbTouch, IndexThumbTouch);
                        this.Subscribe(LeapHandInput.Instance.OppositeMiddleToPalmTouch, OppositeMiddleToPalmTouch);

                        LeapHandInput.Instance.ObserveEveryValueChanged(x => x.inputActive).Subscribe(x => inputActive = x);
                    });
                });
            }
#endif

            if (!_isInputStream)
                this.Wait(() => TutorialManager.Instance.Ready(), () => {
                    this.Subscribe(TutorialManager.Instance.OnTutorialCompleted, OnTutorialCompleted);
                });

            //this.Wait(() => HighlightableZoneController.Instance.Ready(), () => {
            //    this.Subscribe(HighlightableZoneController.Instance.OnActivateFreestyle, 
            //        ()=> {
            //            Debug.Log("SpatialGestureInput received OnActivateFreestyle event trigger.");
            //            OnRightStateChanged.I(HandGestureState.DragSelection, currentRightHandGestureState);
            //        });
            //});
            
            this.WaitS(1, () => {
                if (_isInputStream) return;
                this.SC(UnitTest_Trigger_Right_PopGun(true));
                this.SC(UnitTest_Trigger_Left_PopGun(true));
                this.WaitS(1, () => this.SC(UnitTest_Trigger_DualPopGun(true)));
            });
        }

        public void InitXrHandEventBindings()
        {
            if (!Lexting.inputManager.usesOpenXrHandAdapter)
                return;

            this.Subscribe(
                OpenXRHandInput.Instance.RightStateChanged, RightStateChangedFromHandInput,
                OpenXRHandInput.Instance.LeftStateChanged, LeftStateChangedFromHandInput);
            this.Subscribe(OpenXRHandInput.Instance.RightRotationStateChanged, RightRotationStateChangedFromHandInput);
            this.Subscribe(OpenXRHandInput.Instance.LeftRotationStateChanged, LeftRotationStateChangedFromHandInput);
            this.Subscribe(OpenXRHandInput.Instance.HoldAction, HoldAction);
            this.Subscribe(OpenXRHandInput.Instance.OnHandTrackingToggled, (on) =>
            {
                inputActive = on;
                OnHandTrackingToggled?.I(on);
            });
            this.Subscribe(OpenXRHandInput.Instance.OnHandConfidenceToggled, (on) =>
            {
                handsConfident = true;
                OnHandConfidenceToggled?.I(on);
            });
            this.Subscribe(OpenXRHandInput.Instance.OppositeMiddleToPalmTouch, (right) => OnOppositeMiddleToPalmTouch?.I(right));
            this.Subscribe(OpenXRHandInput.Instance.IndexPointing, (right) => OnIndexPointing?.I(right));
            this.Subscribe(OpenXRHandInput.Instance.IndexFingersAtEachOther, () => OnIndexFingersAtEachOther?.I());
            OpenXRHandInput.Instance.ObserveEveryValueChanged(x => x.inputActive).Subscribe(x => inputActive = x);
        }

        [System.Obsolete("Use InitXrHandEventBindings")]
        public void InitOculusEventBindings() => InitXrHandEventBindings();

        string _logOutput = "";
        private void PrintHandStatus(float repeatInterval)
        {
            _logOutput = $"Right state: {Lexting.rhState}\n" +
                         $"Left state: {Lexting.lhState}";
            //LogOutputReceiver.Instance.Log(_logOutput);
            this.WaitS(repeatInterval, () => PrintHandStatus(repeatInterval));
        }
        #endregion init

        #region event handlers
        #region RX extensions
#if UNITY_EDITOR
        public static IObservable<Unit> GetKeyDown(KeyCode key)
        {
            return Observable.EveryUpdate().Where(_ => UnityEngine.Input.GetKeyDown(key)).AsUnitObservable();
        }

        public static IObservable<Unit> GetKeyUp(KeyCode key)
        {
            return Observable.EveryUpdate().Where(_ => UnityEngine.Input.GetKeyUp(key)).AsUnitObservable();
        }
#endif
        #endregion RX extensions
        
        #region Oculus Touch event handlers
        private void LeftStateChangedFromTouchInput(HandGestureState current, HandGestureState previous)
        {
            currentLeftHandGestureState = current;
            OnLeftStateChanged.I(current, previous);
            OnStateChanged.I(false, current, previous);
        }

        private void RightStateChangedFromTouchInput(HandGestureState current, HandGestureState previous)
        {
            currentRightHandGestureState = current;
            OnRightStateChanged.I(current, previous);
            OnStateChanged.I(true, current, previous);
        }
        #endregion Oculus Touch event handlers

        #region Oculus hand tracking event handlers
        private void LeftStateChangedFromHandInput(HandGestureState current, HandGestureState previous)
        {
            currentLeftHandGestureState = current;
            OnLeftStateChanged.I(current, previous);
            //OnStateChanged.I(false, current, previous);
            OnStateChanged?.Invoke(false, current, previous);
        }
        
        private void RightStateChangedFromHandInput(HandGestureState current, HandGestureState previous)
        {
            Debug.Log("SpatialGestureInput RightStateChangedFromHandInput - current: " + current + " previous: " + previous);
            currentRightHandGestureState = current;
            OnRightStateChanged.I(current, previous);
            OnStateChanged.I(true, current, previous);
        }

        private void LeftRotationStateChangedFromHandInput(CubeRotationState current, CubeRotationState previous)
        {
            //Debug.Log("SpatialGestureInput LeftStateChangedFromHandInput - current: " + current + " previous: " + previous);
            currentLeftRotationState = current;
            OnLeftRotationStateChanged.I(current, previous);
            OnRotationStateChanged.I(false, current, previous);
        }

        private void RightRotationStateChangedFromHandInput(CubeRotationState current, CubeRotationState previous)
        {
            currentRightRotationState = current;
            OnRightRotationStateChanged.I(current, previous);
            OnRotationStateChanged.I(true, current, previous);
            //LogOutputReceiver.Instance.Log($"SpatialGestureInput received a new currentRightRotationState to {current}");

        }

        private void SinglePopGunActive(bool on, bool right)
        {
            if (right && !singleRightPopGunActivated)
                singleRightPopGunActivated = true;
            else if (!right && !singleLeftPopGunActivated)
                singleLeftPopGunActivated = true;
            else return;

            OnSinglePopGunActive.I(on, right);
        }

        private void DualPopGunsActive(bool on)
        {
            OnDualPopGunActive.I(on);
        }
        #endregion Oculus hand tracking event handlers

        /// <summary>
        /// deprecated
        /// </summary>
        private void ModeActiveStateChanged(bool on)
        {
            modeActive = on;
            OnModeActiveStateChanged.I(modeActive);
        }

        /// <summary>
        /// deprecated
        /// </summary>
        private void HoldAction(bool right, CubeTouchStates.TouchStateIndex finger)
        {
            if (right) rightHeldLong[(int)finger] = true;
            OnHoldAction.I(right, finger);
        }

        /// <summary>
        /// deprecated
        /// </summary>
        private void RightRotationStateChanged(CubeRotationState current, CubeRotationState previous)
        {
            currentRightRotationState = current;
            OnRightRotationStateChanged.I(current, previous);
            OnRotationStateChanged.I(true, current, previous);
        }

        /// <summary>
        /// deprecated
        /// </summary>
        private void LeftRotationStateChanged(CubeRotationState current, CubeRotationState previous)
        {
            currentLeftRotationState = current;
            OnLeftRotationStateChanged.I(current, previous);
            OnRotationStateChanged.I(false, current, previous);
        }

        /// <summary>
        /// deprecated
        /// </summary>
        private void RefreshHeldLongState(bool right, bool[] newState)
        {
            if (right) rightHeldLong = newState;
            else leftHeldLong = newState;
        }

        private void OnTutorialCompleted()
        {
            tutorialComplete = true;
            //
        }

        private void OnActivateFreestyle()
        {
            OnRightStateChanged.I(HandGestureState.DragSelection, currentRightHandGestureState);
        }

        private void ThumbOut(bool right)
        {
            OnThumbOut?.I(right);
        }

        private void IndexPointing(bool right)
        {
            OnIndexPointing?.I(right);
        }

        private void IndexThumbTouch(bool right)
        {
            OnIndexThumbTouch?.I(right);
        }

        private void OppositeMiddleToPalmTouch(bool right) 
        {
            OnOppositeMiddleToPalmTouch?.I(right);
        }
        
        #endregion event handlers

        #region Unit Tests
        private IEnumerator UnitTest_Trigger_Right_PopGun(bool on)
        {
            //yield return new WaitForSeconds(3);
            OnSinglePopGunActive.I(on, true);
            yield break;
        }

        private IEnumerator UnitTest_Trigger_Left_PopGun(bool on)
        {
            //yield return new WaitForSeconds(3);
            OnSinglePopGunActive.I(on, false);
            yield break;
        }

        private IEnumerator UnitTest_Trigger_DualPopGun(bool on)
        {
            yield return new WaitForSeconds(5);
            OnDualPopGunActive.I(on);
            yield break;
        }
        #endregion Unit Tests
    }

    public struct LextingHand
    {
        public Transform wrist;
        /// <summary>XR/OpenXR wrist joint when bound; distinct from <see cref="wrist"/> HandMarker root.</summary>
        public Transform trackingWrist;
        public Transform palm;
        public Transform indexTip;
        public Transform indexBase;
        public Transform middleTip;
        public Transform middleBase;
        public Transform littleBase;
        public Transform thumbTip;
    }
}
