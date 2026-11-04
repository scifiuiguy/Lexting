//using SpatialGestureText.Input;
//using SpatialGestureText.Transformation;
//using SpatialGestureText.Tutorial;
//using SpatialGestureText.Utilities;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UniRx;
//using UniRx.Triggers;
//using UnityEngine;
//using UnityEngine.Events;
//using static OVRSkeleton;

//namespace SpatialGestureText.LetterCube
//{
//    /// <summary>
//    /// DEPCRECATED
//    /// Controls display and functionality of one hand cursor
//    /// </summary>
//    public class CubeHandTrackingVisualizer : MonoBehaviour
//    {
////        #region public vars
////        // cube scale 1 = yOffset 0.16f
////        public float yOffset;
////        public CubeController controller;
////        public static Vector3 hoverDirectionRight, hoverDirectionLeft;
////        public Vector3 hoverDirectionRightDisplay;

////        /// <summary>
////        /// A trigger that signals hover change where param1 is right=true/left=false and param2 is center=true/non-center=false
////        /// </summary>
////        public static SpatialGestureEvent<bool,bool> OnHoverUpdate = new SpatialGestureEvent<bool, bool>();

////        /// <summary>
////        /// A trigger that signals hover boundary entry/exit where param1 is right=true/left=false and param2 is inside=true/outside=false
////        /// </summary>
////        public static SpatialGestureEvent<bool, bool> OnHoverBoundaryUpdate = new SpatialGestureEvent<bool, bool>();

////        /// <summary>
////        /// the radius of the cube's center element's hover sensor
////        /// </summary>
////        [SerializeField] public float centerHoverDistanceThreshold;

////        /// <summary>
////        /// the outer hover sensing radius
////        /// </summary>
////        [SerializeField] public float outerHoverDistanceThreshold;

////        public static Vector3 rightPosition, leftPosition;

////        #endregion public vars

////        #region private vars
////        private SpatialGestureInput _input;
////        private bool _offsetEngaged;
////        private bool right;
////        //private HandGestureState _currentHandGestureState;
////        private MeshRenderer[] vRenderers;
////        private SkinnedMeshRenderer[] vSkinnedRenderers;
////        private OVRHand _hand;
////        private OVRSkeleton _skeleton;
////        private Vector3 _horizontalOffset;
////        private Transform _handMarker;

////        /// <summary>
////        /// cached value of hover direction upon update 
////        /// </summary>
////        private Vector3 _rightHandDirection, _leftHandDirection;


////        /// <summary>
////        /// cached value of the magnitude of hand to controller center upon update
////        /// </summary>
////        private float _rightHandDistance, _leftHandDistance;
////        private HandDistanceState _rightHandDistanceState, _leftHandDistanceState;

////        /// <summary>
////        /// cached value of _handDistance for the current hand from last frame
////        /// </summary>
////        private float _previousRightHandDistance, _previousLeftHandDistance;

////        private IDisposable handDirectionUpdate;
////        private IDisposable handDistanceUpdate;

////        /// <summary>
////        /// Direction thresholds, each ranging between -1.0f and 1.0f,
////        /// representing per-axis event triggers across which hover state for a given hand must change.
////        /// Hover state is represented by one dot product per axis, comparing current hand pos to cube controller's local pos.
////        /// </summary>
////        private readonly float[] _dotProductThresholds = new float[] { -0.75f, -0.25f, 0.25f, 0.75f };

////        private bool _rightHandInCenter, _leftHandInCenter;

////        private IDisposable _positionUpdateLoop;

////        /// <summary>
////        /// A cached reference to the position of the last dragged rail, in case tracking is lost/reacquired
////        /// </summary>
////        private Transform _lastRailDragTarget;
////        #endregion private vars

////        #region init
////        private void Awake()
////        {
////            _input = SpatialGestureInput.Instance;
////            right = transform.name.Contains("Right");
////            _hand = (right) ? Lexting.rHand.GetComponentInChildren<OVRHand>() : Lexting.lHand.GetComponentInChildren<OVRHand>();
////            _skeleton = (right) ? Lexting.rHand.GetComponentInChildren<OVRSkeleton>() : Lexting.lHand.GetComponentInChildren<OVRSkeleton>();
////            vRenderers = GetComponentsInChildren<MeshRenderer>();
////            vSkinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
////            _horizontalOffset = (right)
////                ? -1 * Lexting.head.transform.right * Lexting.rHand.horizontalOffset
////                : Lexting.head.transform.right * Lexting.lHand.horizontalOffset;

////            RegisterEvents();
////        }
////        #endregion init

////        private void Update()
////        {
////            hoverDirectionRightDisplay = _rightHandDirection;
////        }

////        #region event handlers
////        private void RegisterEvents()
////        {
////            this.Wait(() => controller.Ready(), () => {
////                this.Subscribe(controller.ControllerActivated, OnControllerActivated);
////            });

////            this.Wait(() => TutorialManager.Instance.Ready(), () => {
////                this.Subscribe(TutorialManager.Instance.OnTutorialCompleted, () => {
////                    this.Wait(() => this.Ready(Lexting.rHand, Lexting.lHand, Lexting.rHand.triggerActivator, Lexting.lHand.triggerActivator),
////                        () =>
////                        {
////                            _handMarker = (right) ? Lexting.rHand.triggerActivator.transform : Lexting.lHand.triggerActivator.transform;
////                            ToggleVisibility(true);
////                            //Debug.Log((right) ? "Right " : "Left " + "CubeHandTrackingVisualizer completed init");
////                            HandlePositionUpdates();
////                        }
////                    );
////                });
////            });

////            transform.ObserveEveryValueChanged(x => x.position).Subscribe(x => {
////                if (right) rightPosition = x;
////                else leftPosition = x;
////            });

////            this.Subscribe(CubePositionController._.Instance.OnCubeDragging, (on, _right, target) => {
////                if (right != _right) return;
////                if (on) HandleDragPositionUpdates(target);
////                else HandlePositionUpdates();
////            });

////            this.Subscribe(_input.OnHandTrackingToggled, (_right, on) => HandleStateDuringTrackingLoss());
////            this.Subscribe(_input.OnHandConfidenceToggled, (_right, confidence) => HandleStateDuringTrackingLoss());
////        }

////        private void HandleStateDuringTrackingLoss()
////        {
////            _positionUpdateLoop?.Dispose();
////            //Debug.Log($"CubeHandTrackingVisualizer - tracking confidence toggle - " +
////            //    $"_input.inputActive? {_input.inputActive} - " +
////            //    $"was rail dragging? {CubePositionController._.Instance.isDragging} - " +
////            //    $"is _lastRailDragTarget null? {_lastRailDragTarget == null}");
////            if (_input.inputActive)
////            {
////                if (CubePositionController._.Instance.isDragging && _lastRailDragTarget != null)
////                    HandleDragPositionUpdates(_lastRailDragTarget);
////                else HandlePositionUpdates();
////            }
////        }

////        private void HandlePositionUpdates()
////        {
////            _positionUpdateLoop?.Dispose();
////            _positionUpdateLoop = _handMarker.ObserveEveryValueChanged(x => x.position)
////                .Subscribe(handPos => {
////                    //if (!right)
////                    //    Debug.Log($" x {handPos.x} y {handPos.y} z {handPos.z}");
////                    transform.position = Vector3.Lerp(    
////                        transform.position, handPos + new Vector3(0, yOffset, 0), CubeRightMarker.handNoiseDampeningFactor);
////#if !UNITY_EDITOR && !UNITY_STANDALONE
////                    transform.rotation = Quaternion.LookRotation(_hand.PointerPose.forward);
////#endif
////                });
////        }

////        private void HandleDragPositionUpdates(Transform target)
////        {
////            _positionUpdateLoop?.Dispose();
////            _lastRailDragTarget = target;
////            _positionUpdateLoop = target.ObserveEveryValueChanged(x => x.position).Subscribe(handPos => transform.position = handPos);
////        }
        
////        private void OnControllerActivated(bool on)
////        {
////            ToggleVisibility(on);
            

////            // non-collider hover state analysis:
////            if (on)
////            {
////                // init distance state
////                OnHoverBoundaryUpdate.I(true, true);
////                OnHoverBoundaryUpdate.I(false, true);

////                // set up distance check loop
////                handDistanceUpdate = this.UpdateAsObservable().Subscribe(_ =>
////                {
////                    if (right) UpdateHandDistance(ref _previousRightHandDistance, ref _rightHandDistance);
////                    else UpdateHandDistance(ref _previousLeftHandDistance, ref _leftHandDistance);
////                });
////            }
////            // turn off distance check loop
////            else if (handDistanceUpdate != null)
////                handDistanceUpdate.Dispose();

////            if (on)
////            {
////                // set up direction check loop
////                handDirectionUpdate = this.UpdateAsObservable().Subscribe(_ =>
////                {
////                    if (right && !_rightHandInCenter)
////                    {
////                        _rightHandDirection = (transform.position - controller.centerPosition).normalized;
////                        UpdateHandDirection(ref hoverDirectionRight);
////                    }
////                    else if (!right && !_leftHandInCenter)
////                    {
////                        _leftHandDirection = (transform.position - controller.centerPosition).normalized;
////                        UpdateHandDirection(ref hoverDirectionLeft);
////                    }
////                });
////            }
////            // turn off direction check loop
////            else if (handDirectionUpdate != null)
////                handDirectionUpdate.Dispose();
////        }

////        private void OnIntroTutorialCompleted()
////        {
            
////        }
////#endregion event handlers

////#region helper methods
////        private void ToggleVisibility(bool on)
////        {
////            foreach (MeshRenderer renderer in vRenderers)
////                renderer.enabled = on;
////            foreach (SkinnedMeshRenderer sRenderer in vSkinnedRenderers)
////                sRenderer.enabled = on;
////        }

////        /// <summary>
////        /// Determine user hand position relative to center of cube controller
////        /// (needed for non-collider version of hover calculations)
////        /// </summary>
////        private void UpdateHandDirection(ref Vector3 direction)
////        {
////            Vector3 previous = direction;
////            direction = new Vector3(
////                Vector3.Dot((right)?_rightHandDirection:_leftHandDirection, controller.transform.right.normalized),
////                Vector3.Dot((right) ? _rightHandDirection : _leftHandDirection, Vector3.up),
////                Vector3.Dot((right) ? _rightHandDirection : _leftHandDirection, controller.transform.forward.normalized)
////            );
////            for (int axis = 0; axis < 3; axis++)
////                if (AxisCrossedThreshold(axis, GetAxisValue(axis, previous), GetAxisValue(axis, direction)))
////                    OnHoverUpdate.I(right, (right) ? _rightHandInCenter : _leftHandInCenter);
////        }
        
////        /// <summary>
////        /// Each value is an axis representing a dot product direction of hand pos relative to cube controller.
////        /// If the delta of previousValue vs newValue crossed over -0.75, -0.25, 0.25, or 0.75, return true, otherwise false
////        /// </summary>
////        /// <param name="axis">x is 0, y is 1, z is 2</param>
////        /// <param name="previousValue">the value of the dot product direction of the hand last frame for one axis</param>
////        /// <param name="newValue">the new value of the dot product direction of the hand THIS frame for one axis</param>
////        private bool AxisCrossedThreshold(int axis, float previousValue, float newValue)
////        {
////            foreach (var threshold in _dotProductThresholds)
////            {
////                if ((previousValue < threshold && newValue >= threshold) ||
////                    (previousValue > threshold && newValue <= threshold))
////                    return true;
////            }
////            return false;
////        }

////        private float GetAxisValue(int axis, Vector3 direction)
////        {
////            switch (axis)
////            {
////                case 0: return direction.x;
////                case 1: return direction.y;
////                case 2: return direction.z;
////                default: return 0;
////            }
////        }

////        private void UpdateHandDistance(ref float previous, ref float current)
////        {
////            previous = current;
////            current = (transform.position - controller.centerPosition).magnitude;

////            if (right)
////            {
////                _rightHandInCenter = _rightHandDistance < centerHoverDistanceThreshold;
////                _rightHandDistanceState = (_rightHandInCenter)
////                    ? HandDistanceState.center
////                    : (_rightHandDistance > outerHoverDistanceThreshold)
////                        ? HandDistanceState.outOfBounds : HandDistanceState.outer;
////            }
////            else
////            {
////                _leftHandInCenter = _leftHandDistance < centerHoverDistanceThreshold;
////                _leftHandDistanceState = (_leftHandInCenter)
////                    ? HandDistanceState.center
////                    : (_leftHandDistance > outerHoverDistanceThreshold)
////                        ? HandDistanceState.outOfBounds : HandDistanceState.outer;
////            }

////            if (DistanceCrossedThresshold(previous, current, centerHoverDistanceThreshold))
////                OnHoverUpdate.I(right, (right) ? _rightHandInCenter : _leftHandInCenter);

////            if (DistanceCrossedThresshold(previous, current, outerHoverDistanceThreshold))
////                OnHoverBoundaryUpdate.I(right, (right) ? _rightHandDistanceState != HandDistanceState.outOfBounds : _leftHandDistanceState != HandDistanceState.outOfBounds);
////        }

////        private bool DistanceCrossedThresshold(float previousDistance, float newDistance, float threshold)
////        {
////            return (previousDistance < threshold && newDistance >= threshold) ||
////                (previousDistance >= threshold && newDistance < threshold);
////        }

//        //private bool DistanceCrossedOuterThresshold(float previousDistance, float newDistance)
//        //{
//        //    return (previousDistance < outerHoverDistanceThreshold && newDistance >= outerHoverDistanceThreshold) ||
//        //        (previousDistance >= outerHoverDistanceThreshold && newDistance < outerHoverDistanceThreshold);
//        //}
////#endregion helper methods
//    }

//    public enum HandDistanceState
//    {
//        center,
//        outer,
//        outOfBounds
//    }
//}