using SpatialGestureText.Debugging;
using SpatialGestureText.Input;
using SpatialGestureText.Interfaces;
using SpatialGestureText.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.TwoZoneCube
{
	public class TwoZoneCubePositionController : Singleton<TwoZoneCubePositionController>, IPositionController
    {
        #region vars
        #region public vars
        public bool hasHover => false;

        /// <summary>Fired after <see cref="Calibrate"/> applies widget world position and rotation (head-ready).</summary>
        public SpatialGestureEvent OnWidgetRepositioned { get; } = new SpatialGestureEvent();
        #endregion public vars
        #region private vars
        private TwoZoneCubeControllers _controllers;

        private Vector3 _startingCubePosition;
        private Vector3 _lookTarget;
        private Vector3 _targetPosition;

        private LextingController _topController;

        /// <summary>
        /// default distance between the cube and the user's chest
        /// </summary>
        [SerializeField] private float _zDistance => _topController?.depthOffset ?? 0f;

        /// <summary>
        /// the interfaceOrigin's initial y offset from the height of the user's head
        /// </summary>
        [SerializeField] private float _headHeightOffset => _topController?.heightOffset ?? 0f;

        /// <summary>
        /// offset created by user dragging the X position handle
        /// </summary>
        private float _xOffset;


        /// <summary>
        /// offset created by user dragging the Y position handle
        /// </summary>
        private float _yOffset;

        /// <summary>
        /// offset created by user dragging the Z position handle
        /// </summary>
        private float _zOffset;

        /// <summary>
        /// Cached value of _xOffset from the last time y handle was dragged
        /// </summary>
        private float _previousXOffset;


        /// <summary>
        /// Cached value of _yOffset from the last time y handle was dragged
        /// </summary>
        private float _previousYOffset;

        /// <summary>
        /// Cached value of _zOffset from the last time z handle was dragged
        /// </summary>
        private float _previousZOffset;

        /// <summary>
        /// A game object representing the point at which the interface will snap position and orientation during calibration
        /// </summary>
        private GameObject _interfaceOrigin;
        #endregion private vars
        #endregion vars

        #region init
        private void Awake()
		{
            _topController = transform.GinG<LextingController>();
            _interfaceOrigin = new GameObject("TwoZoneInterfaceOrigin");
            _interfaceOrigin.transform.SetParent(transform);
            _interfaceOrigin.transform.position = Vector3.zero;
            _controllers = TwoZoneCubeControllers.Instance;
            _controllers.positionController = this;
            this.Subscribe(SpatialGestureInput.Instance.OnIndexFingersAtEachOther, () => Calibrate());

            
        }
        #endregion init

        #region helper methods
        public void Calibrate()
        {
            _interfaceOrigin.transform.SetParent(null);
            this.Wait(() => Lexting.head.Ready(), () => {
                _interfaceOrigin.transform.position = Lexting.head.transform.position + Lexting.head.flatLookDirection * _zDistance + Vector3.down * _headHeightOffset;
                transform.position = _interfaceOrigin.transform.position;
                transform.rotation = Quaternion.LookRotation(Lexting.head.flatLookDirection);
                Lexting.finalizer.UpdateTransform();
                OnWidgetRepositioned?.I();
            });
        }
		#endregion helper methods
		
	}
}
