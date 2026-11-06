using SpatialGestureText.Input;
using SpatialGestureText.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SpatialGestureText.Zones
{
	public class ZoneTextingDemoCanvas : Singleton<ZoneTextingDemoCanvas>
	{
        #region vars
        [SerializeField] private Transform _motionRoot;
        [SerializeField] private float _cameraDepthDistance;
        [Range(0, 1)]
        [SerializeField] private float _rootMotionDampeningFactor;
        private Vector3 _targetDirection;
        private float _lookConfidence;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private bool _isMoving;
        private bool _hasAppliedInitialDepthSnap;
        private CanvasGroup _canvasGroup;

        private const float DepthArrivalSlack = 1.5f;
        //public UnityAction OnRefreshPosition;
        public SpatialGestureEvent OnRefreshPosition = new SpatialGestureEvent();
        #endregion vars

        private void OnEnable()
        {
            if (CanSnapToCameraDepth())
                TryApplyInitialWorldDepth();
        }

        private void Start()
        {
            this.Ch(ref _canvasGroup);
            if (Lexting.inputManager.isDesktop)
                _canvasGroup.alpha = 0;

            this.Wait(() => CanSnapToCameraDepth(), TryApplyInitialWorldDepth);
        }

        /// <summary>World-place output UI at head + horizontal forward × depth (same target as look-away reposition).</summary>
        public void SnapToCameraDepthNow()
        {
            EnsureInitialWorldDepth();
        }

        /// <summary>One-time snap when head tracking is ready (tutorial intro and cold start).</summary>
        public void EnsureInitialWorldDepth()
        {
            if (!CanSnapToCameraDepth())
                return;
            SnapMotionRootToCameraDepth();
            _hasAppliedInitialDepthSnap = true;
        }

        #region loops
        private void Update()
        {
            if (!Lexting.inputManager.hasHead || _motionRoot == null)
                return;

            if (!_hasAppliedInitialDepthSnap)
            {
                if (CanSnapToCameraDepth())
                    TryApplyInitialWorldDepth();
                return;
            }

            _targetDirection = _motionRoot.transform.position - Lexting.head.transform.position;
            _lookConfidence = Vector3.Dot(Lexting.head.transform.forward, _targetDirection.normalized);

            if (_lookConfidence < 0)
                _isMoving = true;

            if (_isMoving)
            {
                _targetPosition = GetCameraDepthWorldPosition();
                _motionRoot.transform.position = Vector3.Lerp(_motionRoot.transform.position, _targetPosition, _rootMotionDampeningFactor);

                // Do not stop while the user looks at the panel mid-lerp — wait until we reach camera depth.
                if (_lookConfidence > 0.95f && IsAtExpectedCameraDepth())
                {
                    _isMoving = false;
                    OnRefreshPosition.I();
                }
            }

            _targetRotation = Quaternion.LookRotation(_targetDirection);
            _motionRoot.transform.rotation = Quaternion.Lerp(_motionRoot.transform.rotation, _targetRotation, _rootMotionDampeningFactor);
        }
        #endregion loops

        private Vector3 GetHorizontalForward()
        {
            return -1 * Vector3.Cross(Vector3.up, Vector3.Cross(Vector3.up, Lexting.head.transform.forward));
        }

        private Vector3 GetCameraDepthWorldPosition()
        {
            return Lexting.head.transform.position + GetHorizontalForward() * _cameraDepthDistance;
        }

        /// <summary>
        /// Same world target as the _isMoving reposition path. Scene/local RectTransform Z does not set this distance.
        /// </summary>
        private void SnapMotionRootToCameraDepth()
        {
            _motionRoot.transform.position = GetCameraDepthWorldPosition();
            _targetDirection = _motionRoot.transform.position - Lexting.head.transform.position;
            _motionRoot.transform.rotation = Quaternion.LookRotation(_targetDirection);
            _isMoving = false;
        }

        private bool CanSnapToCameraDepth()
        {
            return _motionRoot != null
                   && Lexting.inputManager != null
                   && Lexting.inputManager.hasHead
                   && Lexting.head.Ready();
        }

        private void TryApplyInitialWorldDepth()
        {
            if (_hasAppliedInitialDepthSnap)
                return;
            EnsureInitialWorldDepth();
        }

        private bool IsAtExpectedCameraDepth()
        {
            var dist = Vector3.Distance(Lexting.head.transform.position, _motionRoot.transform.position);
            return dist >= _cameraDepthDistance - DepthArrivalSlack;
        }
    }
}
