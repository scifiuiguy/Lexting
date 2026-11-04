using SpatialGestureText.Debugging;
using SpatialGestureText.Input;
using SpatialGestureText.Interfaces;
using SpatialGestureText.LetterCube;
using SpatialGestureText.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace SpatialGestureText.TwoZoneCube
{
    /// <summary>
    /// Controls display and functionality of one hand cursor (two-zone cube)
    /// </summary>
	public class HandCursor : MonoBehaviour
	{
        #region vars
        #region public vars
        public static Vector3 hoverDirectionRight, hoverDirectionLeft;

        public static bool rightInBounds;
        public static bool leftInBounds;
        public static bool inBounds(bool right) => right ? rightInBounds : leftInBounds;

        public bool handCursorActive = true;

        /// <summary>
        /// A trigger that signals hover change where param1 is right=true/left=false and param2 is center=true/non-center=false
        /// </summary>
        public static SpatialGestureEvent<bool, bool> OnHoverUpdate = new SpatialGestureEvent<bool, bool>();

        /// <summary>
        /// A trigger that signals hover boundary entry/exit where param1 is right=true/left=false and param2 is inside=true/outside=false
        /// </summary>
        public static SpatialGestureEvent<bool, bool> OnHoverBoundaryUpdate = new SpatialGestureEvent<bool, bool>();

        /// <summary>
        /// the radius of the cube's center element's hover sensor
        /// </summary>
        [SerializeField] public float centerHoverDistanceThreshold;

        /// <summary>
        /// the outer hover sensor radius
        /// </summary>
        [SerializeField] public float outerHoverDistanceThreshold;

        public static Vector3 rightPosition, leftPosition;
        #endregion public vars
        #region private vars
        [SerializeField] private bool _right;

        private LextingController _topController;

        /// <summary>
        /// the position offset of the hand cursor relative to the position of the actual hand
        /// These were aligned well for Leap Motion w/
        /// RIGHT: 0.03, 0.2, 0.05
        /// LEFT: -0.03, 0.2, 0.05
        /// </summary>
        /*[SerializeField]*/
        private Vector3 _handMarkerOffset => _topController?.cursorOffset ?? Vector3.zero;

        private Vector3 _UIAlignedPalmNormal => _right ? -1 * Lexting.controllers.transform.right : Lexting.controllers.transform.right;

        private Vector3 _headLookDirection;

        private IController _controllers;
        private TwoZoneCubeController controller;

        /// <summary>
        /// cached value of hover direction upon update 
        /// </summary>
        private Vector3 _rightHandDirection, _leftHandDirection;


        /// <summary>
        /// cached value of the magnitude of hand to controller center upon update
        /// </summary>
        private float _rightHandDistance, _leftHandDistance;
        private HandDistanceState _rightHandDistanceState, _leftHandDistanceState;

        /// <summary>
        /// cached value of _handDistance for the current hand from last frame
        /// </summary>
        private float _previousRightHandDistance, _previousLeftHandDistance;

        private IDisposable handDirectionUpdate;
        private IDisposable handDistanceUpdate;

        private MeshRenderer[] vRenderers;
        private SkinnedMeshRenderer[] vSkinnedRenderers;

        private bool _rightHandInCenter, _leftHandInCenter;

        private bool _inCenter(bool right) => right ? _rightHandInCenter : _leftHandInCenter;

        /// <summary>
        /// Direction thresholds, each ranging between -1.0f and 1.0f,
        /// representing per-axis event triggers across which hover state for a given hand must change.
        /// Hover state is represented by one dot product per axis, comparing current hand pos to cube controller's local pos.
        /// </summary>
        private readonly float[] _dotProductThresholds = new float[] { 0 }; // w/ 2x2x2, there is only one threshold per axis, across 0

        /// <summary>
        /// the current position to place the cursor unless it is out-of-bounds
        /// </summary>
        private Vector3 _targetPosition;
        private Vector3 _targetDirection => (_targetPosition - (controller?.transform.position ?? Vector3.zero)).normalized;
        #endregion private vars
        #endregion vars

        #region init
        private void Awake()
		{
            _topController = transform.GinG<LextingController>();
            _controllers = LextingController.Instance;
            InitHandRef();
            vRenderers = GetComponentsInChildren<MeshRenderer>();
            vSkinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            RegisterEvents();

            //if (Lexting.inputManager.isDesktop)
            //    _handMarkerOffset = new Vector3((_right)?-0.07f:0.07f, _handMarkerOffset.y, _handMarkerOffset.z);
        }

        /// <summary>
        /// Get a reference to the right hand if this is the right cursor or left hand if this is the left cursor
        /// </summary>
        private void InitHandRef()
        {
            this.Wait(() => (_controllers as TwoZoneCubeControllers).controllers.Length > 0, () => {
                controller = (_right) ? (_controllers as TwoZoneCubeControllers).right
                                      : (_controllers as TwoZoneCubeControllers).left;
             });
        }
        #endregion init

        #region event handlers
        int frameCounter = 0;
        private void RegisterEvents()
        {
            this.UpdateAsObservable().Subscribe(_ => {
                //this.Wait(()=> Lexting.input.inputActive,()=> {
                if (Lexting.inputManager.hasHead && Lexting.head.Ready())
                {
                    if (TryGetHandTrackingWorldPoint(_right, out var handPoint))
                    {
                        _targetPosition = handPoint
                            + Lexting.controllers.transform.forward * _handMarkerOffset.z
                            + Vector3.up * _handMarkerOffset.y
                            + _UIAlignedPalmNormal * _handMarkerOffset.x;
                        if (inBounds(_right))
                            transform.position = _targetPosition;
                        else
                            transform.position = Vector3.Lerp(
                                transform.position,
                                (controller != null ? controller.transform.position : Vector3.zero)
                                    + _targetDirection * outerHoverDistanceThreshold * 1.01f,
                                0.1f);
                        transform.rotation = Quaternion.LookRotation(Lexting.head.flatLookDirection);
                    }
                    else if (!Lexting.input.inputActive && controller != null)
                    {
                        transform.position = Vector3.Lerp(transform.position, controller.transform.position, 0.1f);
                    }
                }
               // });
            });
            
            this.Wait(() => _controllers.IsReady(), () =>
            {
                this.Subscribe(_controllers.ControllerActivated, (on) =>
                {
                    ToggleVisibility(on);

                    // non-collider hover state analysis:
                    if (on)
                    {
                        // Widget is summoned in front of the user; static flags must match the
                        // boundary events (defaults are false while the rig sits at world origin).
                        rightInBounds = true;
                        leftInBounds = true;
                        OnHoverBoundaryUpdate.I(true, true);
                        OnHoverBoundaryUpdate.I(false, true);

                        // set up distance check loop
                        handDistanceUpdate = this.UpdateAsObservable().Subscribe(_ =>
                        {
                            if (_right) UpdateHandDistance(ref _previousRightHandDistance, ref _rightHandDistance);
                            else UpdateHandDistance(ref _previousLeftHandDistance, ref _leftHandDistance);
                        });
                    }
                    // turn off distance check loop
                    else if (handDistanceUpdate != null)
                    {
                        handDistanceUpdate.Dispose();
                        if (_right)
                            rightInBounds = false;
                        else
                            leftInBounds = false;
                        OnHoverBoundaryUpdate.I(_right, false);
                    }

                    if (on)
                    {
                        // set up direction check loop
                        handDirectionUpdate = this.UpdateAsObservable().Subscribe(_ =>
                        {
                            if (_right && !_rightHandInCenter)
                            {
                                _rightHandDirection = _targetDirection;
                                UpdateHandDirection(ref hoverDirectionRight);
                            }
                            else if (!_right && !_leftHandInCenter)
                            {
                                _leftHandDirection = _targetDirection;
                                UpdateHandDirection(ref hoverDirectionLeft);
                            }
                        });
                    }
                    // turn off direction check loop
                    else if (handDirectionUpdate != null)
                        handDirectionUpdate.Dispose();
                });
            });

            transform.ObserveEveryValueChanged(x => x.position).Subscribe(x => {
                if (_right) rightPosition = x;
                else leftPosition = x;
            });
        }
        #endregion event handlers

        #region helper methods
        static bool TryGetHandTrackingWorldPoint(bool right, out Vector3 worldPoint)
        {
#if !UNITY_STANDALONE && UNITY_ANDROID
            if (QuestHandCursorPosition.HasTrackingPoint(right))
            {
                worldPoint = QuestHandCursorPosition.GetWorldPosition(right);
                return worldPoint.sqrMagnitude > 1e-6f;
            }
#endif
            worldPoint = default;
            if (!Lexting.input.inputActive || !Lexting.input.handsConfident)
                return false;

            var marker = right ? Lexting.rHandMarker : Lexting.lHandMarker;
            if (marker == null || marker.cursorAnchor == null)
                return false;

            worldPoint = marker.cursorAnchor.transform.position;
            return true;
        }

        private void ToggleVisibility(bool on)
        {
            foreach (MeshRenderer renderer in vRenderers)
                renderer.enabled = on;
            foreach (SkinnedMeshRenderer sRenderer in vSkinnedRenderers)
                sRenderer.enabled = on;
        }

        private void UpdateHandDistance(ref float previous, ref float current)
        {
            previous = current;
            current = Mathf.Abs((_targetPosition - controller.transform.position).magnitude);

            if (_right)
            {
                _rightHandInCenter = _rightHandDistance < centerHoverDistanceThreshold;
                _rightHandDistanceState = (_rightHandInCenter)
                    ? HandDistanceState.center
                    : (_rightHandDistance > outerHoverDistanceThreshold)
                        ? HandDistanceState.outOfBounds : HandDistanceState.outer;
                //LogOutputReceiver.Instance.Log($"Right hand distance: {_rightHandDistance}\n" +
                //    $"right hand distance state: {_rightHandDistanceState}");
            }
            else
            {
                _leftHandInCenter = _leftHandDistance < centerHoverDistanceThreshold;
                _leftHandDistanceState = (_leftHandInCenter)
                    ? HandDistanceState.center
                    : (_leftHandDistance > outerHoverDistanceThreshold)
                        ? HandDistanceState.outOfBounds : HandDistanceState.outer;
            }

            if (DistanceCrossedThresshold(previous, current, centerHoverDistanceThreshold))
            {
                // Fire on enter *and* exit. Old code only fired while still "in center", so leaving the
                // center ring never re-selected an outer node (center highlight stuck).
                if (!_inCenter(_right))
                    RefreshHoverDirectionAfterLeavingCenter();
                OnHoverUpdate.I(_right, _inCenter(_right));
            }

            if (_right)
            {
                var nowInBounds = _rightHandDistanceState != HandDistanceState.outOfBounds;
                if (nowInBounds != rightInBounds)
                {
                    rightInBounds = nowInBounds;
                    OnHoverBoundaryUpdate.I(true, rightInBounds);
                }
            }
            else
            {
                var nowInBounds = _leftHandDistanceState != HandDistanceState.outOfBounds;
                if (nowInBounds != leftInBounds)
                {
                    leftInBounds = nowInBounds;
                    OnHoverBoundaryUpdate.I(false, leftInBounds);
                }
            }
        }

        /// <summary>
        /// Determine user hand position relative to center of cube controller
        /// (needed for non-collider version of hover calculations)
        /// </summary>
        private void RefreshHoverDirectionAfterLeavingCenter()
        {
            if (controller == null)
                return;

            var handDir = _targetDirection;
            if (_right)
            {
                _rightHandDirection = handDir;
                hoverDirectionRight = ComputeHoverDirectionDots(handDir);
            }
            else
            {
                _leftHandDirection = handDir;
                hoverDirectionLeft = ComputeHoverDirectionDots(handDir);
            }
        }

        private Vector3 ComputeHoverDirectionDots(Vector3 handDirection)
        {
            return new Vector3(
                Vector3.Dot(handDirection, controller.transform.right.normalized),
                Vector3.Dot(handDirection, Vector3.up),
                Vector3.Dot(handDirection, controller.transform.forward.normalized));
        }

        private void UpdateHandDirection(ref Vector3 direction)
        {
            Vector3 previous = direction;
            direction = ComputeHoverDirectionDots((_right) ? _rightHandDirection : _leftHandDirection);
            for (int axis = 0; axis < 3; axis++)
                if (AxisCrossedThreshold(axis, GetAxisValue(axis, previous), GetAxisValue(axis, direction)) && !_inCenter(_right))
                    OnHoverUpdate.I(_right, _inCenter(_right));
        }

        /// <summary>
        /// Each value is an axis representing a dot product direction of hand pos relative to cube controller.
        /// If the delta of previousValue vs newValue crossed over a threshold, return true, otherwise false
        /// </summary>
        /// <param name="axis">x is 0, y is 1, z is 2</param>
        /// <param name="previousValue">the value of the dot product direction of the hand last frame for one axis</param>
        /// <param name="newValue">the new value of the dot product direction of the hand THIS frame for one axis</param>
        private bool AxisCrossedThreshold(int axis, float previousValue, float newValue)
        {
            foreach (var threshold in _dotProductThresholds)
            {
                if ((previousValue < threshold && newValue >= threshold) ||
                    (previousValue > threshold && newValue <= threshold))
                    return true;
            }
            return false;
        }

        private float GetAxisValue(int axis, Vector3 direction)
        {
            switch (axis)
            {
                case 0: return direction.x;
                case 1: return direction.y;
                case 2: return direction.z;
                default: return 0;
            }
        }

        private bool DistanceCrossedThresshold(float previousDistance, float newDistance, float threshold)
        {
            return (previousDistance < threshold && newDistance >= threshold) ||
                (previousDistance >= threshold && newDistance < threshold);
        }
        #endregion helper methods

        #region Unit Tests

        #endregion Unit Tests
    }

    public enum HandDistanceState
    {
        center,
        outer,
        outOfBounds
    }
}
