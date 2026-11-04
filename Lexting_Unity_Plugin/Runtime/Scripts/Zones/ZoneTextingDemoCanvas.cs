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
        private Vector3 _horizontalForward;
        private Vector3 _targetDirection;
        private float _lookConfidence;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private bool _isMoving;
        private CanvasGroup _canvasGroup;
        //public UnityAction OnRefreshPosition;
        public SpatialGestureEvent OnRefreshPosition = new SpatialGestureEvent();
        #endregion vars

        private void Start()
        {
            this.Ch(ref _canvasGroup);
            if (Lexting.inputManager.isDesktop)
                _canvasGroup.alpha = 0;
        }

        #region loops
        private void Update()
        {
            if (!Lexting.inputManager.hasHead) return;
            _targetDirection = _motionRoot.transform.position - Lexting.head.transform.position;
            _lookConfidence = Vector3.Dot(Lexting.head.transform.forward, _targetDirection.normalized);            

            if (_lookConfidence < 0)
                _isMoving = true;

            if (_isMoving)
            {
                _horizontalForward = -1 * Vector3.Cross(Vector3.up, Vector3.Cross(Vector3.up, Lexting.head.transform.forward));
                _targetPosition = Lexting.head.transform.position + _horizontalForward * _cameraDepthDistance;
                _motionRoot.transform.position = Vector3.Lerp(_motionRoot.transform.position, _targetPosition, _rootMotionDampeningFactor);
                if (_lookConfidence > 0.95f)
                {
                    _isMoving = false;
                    OnRefreshPosition.I();
                }
                
            }
            _targetRotation = Quaternion.LookRotation(_targetDirection);
            _motionRoot.transform.rotation = Quaternion.Lerp(_motionRoot.transform.rotation, _targetRotation, _rootMotionDampeningFactor);
        }
        #endregion loops
    }
}
