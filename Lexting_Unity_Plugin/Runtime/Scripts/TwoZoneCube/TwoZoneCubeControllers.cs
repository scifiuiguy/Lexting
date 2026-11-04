using SpatialGestureText.Input;
using SpatialGestureText.Interfaces;
using SpatialGestureText.LetterCube;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using SpatialGestureText.Utilities;

namespace SpatialGestureText.TwoZoneCube
{
    /// <summary>
    /// A hierarchy parent of both left and right TwoZoneCubeController, used for singleton access to both
    /// </summary>
	public class TwoZoneCubeControllers : Singleton<TwoZoneCubeControllers>, IController
	{

        #region IController interface implementation

        /// <summary>
        /// cached ref of either right or left current trigger updated any time any trigger executes
        /// </summary>
        public ITrigger currentTrigger;

        /// <summary>
        /// Even if the other hand now supercedes this hand, it's still helpfult to ref the last current for this hand
        /// </summary>
        public ITrigger lastCurrentTrigger(bool right) => right ? currentRightTrigger : currentLeftTrigger;

        public ITrigger currentRightTrigger
        {
            get
            {
                return TwoZoneCubeController.currentRightTrigger;
            }
            set
            {
                TwoZoneCubeController.currentRightTrigger = value;
            }
        }

        public ITrigger currentLeftTrigger
        {
            get
            {
                return TwoZoneCubeController.currentLeftTrigger;
            }
            set
            {
                TwoZoneCubeController.currentLeftTrigger = value;
            }
        }
        private static ITrigger _currentLeftTrigger;

        public bool textCubeActive { get; set; }

        public bool rightHandInBounds {
            get {
                return right.inBounds;
            }
            set {
                right.inBounds = value;
            }
        }

        public bool leftHandInBounds
        {
            get
            {
                return left.inBounds;
            }
            set
            {
                left.inBounds = value;
            }
        }

        // if true, this is the PC client receiving a local wireless input stream via UDP - set this true in Inspector for the PC receiver client only
        public bool isInputStreamReceiver
        {
            get
            {
                return _isInputStreamReceiver;
            }
            set
            {
                _isInputStreamReceiver = value;
            }
        }

        [SerializeField] private bool _isInputStreamReceiver;

        public Vector3 centerPosition {
            get {
                return transform.position;
            }
            set {
                transform.position = value;
            }
        }

        public SpatialGestureEvent<bool> ControllerActivated { get; set; }
        public SpatialGestureEvent<bool, string> OutputBegan { get; set; }
        public CubeDragLineVisualizer rightDragLineHandler { get; set; }
        public CubeDragLineVisualizer leftDragLineHandler { get; set; }
        public bool IsReady() => this.Ready();
        #endregion IController interface implementation
        
        [HideInInspector]
        public TwoZoneCubeController[] controllers;
        public TwoZoneCubeController right => controllers?[0];
        public TwoZoneCubeController left => controllers?[1];

        [HideInInspector]
        public TwoZoneCubePositionController positionController;

        private void Awake()
        {
            ControllerActivated = new SpatialGestureEvent<bool>();
            OutputBegan = new SpatialGestureEvent<bool, string>();
            rightDragLineHandler = RightDragMarker.Instance.GetComponent<CubeDragLineVisualizer>();
            leftDragLineHandler = LeftDragMarker.Instance.GetComponent<CubeDragLineVisualizer>();
            this.Chs(ref controllers);
            RegisterAllEventsAndActions();
        }

        #region event handlers
        private void RegisterAllEventsAndActions()
        {
            this.GetGestures(
                (right, current, previous) => { },
                (right, current, previous) =>
                {
                    if (right)
                    {
                        //if (Lexting.rhRotState == CubeRotationState.other && Lexting.lhRotState == CubeRotationState.other &&
                        //    previous != CubeRotationState.other)
                        //{
                        //    if (!Lexting.inputManager.isDesktop)
                        //        CalibratePosition();
                        //    ControllerActivated.I(true);
                        //}
                    }
                }
            );

            //this.Wait(() => Lexting.finalizer.Ready(), () => {
            //    Lexting.finalizer.ObserveEveryValueChanged(x => x.isSpatialListActive)
            //        .Where(active => active).Subscribe(active => {
            //            ResetAllTriggers();
            //        });
            //});

            this.Wait(() => Lexting.input.Ready(), () =>
                this.Subscribe(Lexting.input.OnIndexPointing, (right)=>ResetAllTriggers()));

            this.Subscribe(SpatialGestureInput.Instance.OnIndexFingersAtEachOther, () => ControllerActivated?.I(true));
        }
        #endregion event handlers

        #region helpers

        public void ToggleCharDisplay(bool on)
        {
            foreach (var controller in controllers)
                controller.charDisplayHandler.ToggleVisibility(on);
        }

        public void ResetAllTriggers()
        {
            foreach (var controller in controllers)
                controller.ResetAllTriggers();
        }

        public void ResetAllTriggers(bool right)
        {
            controllers[(right)?0:1].ResetAllTriggers();
        }

        public void FlashCubeHighlight(bool correct)
        {
            // not yet implemented
        }

        public Vector3 GetLetterPosition(char letter, bool right)
        {
            // not yet implemented
            return Vector3.zero;
        }
        #endregion helpers
    }
}
