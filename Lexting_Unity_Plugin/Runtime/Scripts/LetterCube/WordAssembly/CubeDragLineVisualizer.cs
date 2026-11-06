using SpatialGestureText.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UniRx;
using UniRx.Triggers;
using SpatialGestureText.TwoZoneCube;
using SpatialGestureText.Interfaces;
using SpatialGestureText.Utilities;

namespace SpatialGestureText.LetterCube
{
    /// <summary>
    /// Controls display of a line which connects a letter where drag begins to a letter where drag completes.
    /// </summary>
    public class CubeDragLineVisualizer : MonoBehaviour
    {
        private bool _right;
        [SerializeField] private CubeDragLineVisualizer _otherHand;
        [SerializeField] private bool _isUsingTwoZone;
        private LineRenderer _lineRenderer;
        private Vector3 _currentLineStartPos;
        private bool isListeningForDrag;
        private string _currentLetter;
        
        public int[] currentGridIndex;

        public int currentTwoZoneIndex;

        [SerializeField] private Transform _handPosVisualizerTransform;
        
        public bool isLineTracingActive;
        public static bool isEitherHandTracing;

        public SpatialGestureEvent<bool> LineTracingActivated = new SpatialGestureEvent<bool>();
        
        private void Awake()
        {
            if (transform.name.Contains("Right"))
                _right = true;

            RegisterAllEvents();

            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.enabled = false;

            //this.WaitS(2, () => ToggleLineTracing(HandGestureState.Insert, HandGestureState.Neutral));
        }

        #region loops and timers
        private void Update()
        {
            if (isLineTracingActive && !Lexting.assembler.isWordScrollActive)
                UpdateLinePosition();
        }
        #endregion loops and timers

        #region event handlers
        private void RegisterAllEvents()
        {
            this.GetGestures((right, current, previous) => {
                if (right != _right || !isListeningForDrag || !Lexting.input.inputActive) return;
                if (!_isUsingTwoZone && LextingPositionController.Instance != null && LextingPositionController.Instance.hasHover) return;
                ToggleLineTracing(current, previous);
            });
           
            this.Wait(() => LextingController.Instance.IsReady(), () => {
                this.Subscribe(LextingController.Instance.ControllerActivated, (on) => this.WaitS(1, () => {
                        //Debug.Log($"CubeDragLineViz isListeningForDrag? {isListeningForDrag}");
                        isListeningForDrag = on;
                        _otherHand = (_right) ? LextingController.Instance.leftDragLineHandler : LextingController.Instance.rightDragLineHandler;
                    })
                );

                this.Subscribe(Lexting.input.OnIndexPointing, (right) => ResetStateToIdle(true));
            });
        }
        #endregion event handlers

        #region helper methods
        private void ToggleLineTracing(HandGestureState current, HandGestureState previous)
        {
            try {
        //        Debug.Log("ToggleLineTracing - " + ((_right) ? " right " : " left ") + " = " +
        //"current: " + current + " previous: " + previous);
        //        Debug.Log(
        //            $"is current right trigger null? {LextingController.Instance.currentRightTrigger == null} - " +
        //            $"is current left trigger null? {LextingController.Instance.currentLeftTrigger == null} - ");
        //        Debug.Log(
        //            $"is current right trigger space? {LextingController.Instance.currentRightTrigger.IsSpace} - " +
        //            $"is current left trigger space? {LextingController.Instance.currentLeftTrigger.IsSpace} - ");
        //        Debug.Log(
        //            $"current right rot state {Lexting.rhRotState} - current right rot state {Lexting.lhRotState}");
                if (current == HandGestureState.Insert && previous != HandGestureState.Insert)
                {
                    // Do not engage tracing on backspace press
                    if (_right && LextingController.Instance.currentRightTrigger == null) return;
                    if (!_right && LextingController.Instance.currentLeftTrigger == null) return;
                    if (_right && LextingController.Instance.currentRightTrigger.IsHotkey && Lexting.rhRotState == CubeRotationState.pronate) return;
                    else if (!_right && LextingController.Instance.currentLeftTrigger.IsHotkey && Lexting.lhRotState == CubeRotationState.pronate) return;


                    SetLineTracingActive();
                    //Debug.Log(((_right) ? "Right " : "Left ") + "just set line tracing active.");
                }
                else if (current != HandGestureState.Insert && previous == HandGestureState.Insert)
                {
                    ResetStateToIdle(false);
                    //Debug.Log(((_right) ? "Right " : "Left ") + "just turned line tracing off.");
                }
            }
            catch (Exception e)
            {
                //Debug.Log(e.Message);
            }

        }

        private void UpdateLinePosition()
        {
            _lineRenderer.SetPositions(new Vector3[] { _currentLineStartPos, _handPosVisualizerTransform.position });
        }

        private void SetLineTracingActive()
        {
            isLineTracingActive = true;
            isEitherHandTracing = true;
            _currentLineStartPos = _handPosVisualizerTransform.position;
            _lineRenderer.enabled = true;
            _currentLetter = (_right) ? TwoZoneTrigger.currentRightHighlightedLetter : TwoZoneTrigger.currentLeftHighlightedLetter;
            //Debug.Log(((_right) ? "Right " : "Left ") + "line tracing now active");
            LineTracingActivated.I(true);
        }

        /// <summary>
        /// Resets state variables so that line tracing becomes idle for this hand
        /// </summary>
        /// <param name="refreshExternalState">if true, tell word assembler and output manager to refresh their state vars too</param>
        public void ResetStateToIdle(bool refreshExternalState)
        {
            isLineTracingActive = false;
            if (_otherHand == null || !_otherHand.isLineTracingActive)
                isEitherHandTracing = false;
            if (_lineRenderer != null)
            {
                _lineRenderer.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
                _lineRenderer.enabled = false;
            }

            // update state for single-char entry only -> bypass this during two-handed word assembly
            if (refreshExternalState && !Lexting.assembler.isWordAssemblyActive &&
                    _currentLetter != null && 
                    _currentLetter.Equals((_right) 
                                            ? TwoZoneTrigger.currentRightHighlightedLetter 
                                            : TwoZoneTrigger.currentLeftHighlightedLetter))
            {
                // enable output manager to properly refresh its prefix:
                //Lexting.printer.ConfirmSingleLetterEntry(_right);
                // reset all lists that were mistakenly attempting to assemble entries for word construction:
                Lexting.assembler.DeactivateWordAssembler();
            }
            
            LineTracingActivated.I(false);
        }
        #endregion helper methods
    }
}
