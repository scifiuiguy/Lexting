//using SpatialGestureText.Input;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace SpatialGestureText.LetterCube
//{
//    /// <summary>
//    /// DEPRECATED
//    /// </summary>
//    public class CubeSpaceHandler : Singleton<CubeSpaceHandler>
//    {
//        #region vars
//        private SpatialGestureInput _input;
//        private Transform _spaceTransform;
//        private Transform _backspaceTransform;
//        private LextingOutputManager _outputManager;
//        private bool _spaceHandlerActive = true;
//        private int _outputLength;

//        /// <summary>
//        /// A cached reference to the rotation state of the current hand
//        /// </summary>
//        private CubeRotationState _currentRotationState;

//        /// <summary>
//        /// A cached reference to the rotation state of the non-current hand
//        /// </summary>
//        private CubeRotationState _otherRotationState;
//        public static bool resetTrailingSpace;
        
//        #endregion vars

//        #region init
//        private void Awake()
//        {
//            _outputManager = LextingOutputManager.Instance;
//            _input = SpatialGestureInput.Instance;
//            _spaceTransform = transform.parent.GetChild(transform.parent.childCount - 1).GetChild(0).GetChild(transform.GetSiblingIndex()).GetChild(0);
//            if (_spaceTransform == null)
//                Debug.LogError("Space Handler couldn't locate the space bar transform");
//            _backspaceTransform = transform.parent.GetChild(transform.parent.childCount - 1).GetChild(0).GetChild(transform.GetSiblingIndex()).GetChild(1);
//            if (_backspaceTransform == null)
//                Debug.LogError("Space Handler couldn't locate the backspace transform");

//            this.Subscribe(_input.OnRightRotationStateChanged, OnRightRotationStateChanged);
//            this.Subscribe(_input.OnLeftRotationStateChanged, OnLeftRotationStateChanged);
//        }
//        #endregion init

//        #region event handlers
//        private void OnRightRotationStateChanged(CubeRotationState newState, CubeRotationState previousState)
//        {
//            if (newState == CubeRotationState.pronate)
//            {
//                _backspaceTransform.gameObject.SetActive(true);
//            }

//            if (newState == CubeRotationState.relax)
//            {
//                //Debug.Log("Lowercase callback turning off object named " + _backspaceTransform.gameObject.name);
//                _backspaceTransform.gameObject.SetActive(false);
//            }
//        }

//        private void OnLeftRotationStateChanged(CubeRotationState newState, CubeRotationState previousState)
//        {
//            if (newState == CubeRotationState.pronate)
//            {
//                _backspaceTransform.gameObject.SetActive(true);
//            }

//            if (newState == CubeRotationState.relax)
//            {
//                //Debug.Log("Lowercase callback turning off object named " + _backspaceTransform.gameObject.name);

//                //if (resetTrailingSpace)
//                //{
//                //    resetTrailingSpace = false;
//                //    LextingOutputManager.Instance.AppendOutputs(" ");
//                //}

//                _backspaceTransform.gameObject.SetActive(false);
//            }
//        }
//        #endregion event handlers

//        #region helpers
//        public void Insert(bool right)
//        {
//            Debug.Log($"CubeSpaceHandler - " + ((right) ? "right -" : "left -") + $" begin Insert, _spaceHandlerActive? {_spaceHandlerActive}");
//            if (!_spaceHandlerActive) return;
//            _currentRotationState = (right) ? _input.currentRightRotationState : _input.currentLeftRotationState;
//            _otherRotationState = (right) ? _input.currentLeftRotationState : _input.currentRightRotationState;


//            Debug.Log($"CubeSpaceHandler - cube rotation state: {_currentRotationState} " +
//                    $"currentOutputValue.Length: {_outputManager.CurrentOutputValue.Length}");
           
//            if (_currentRotationState == CubeRotationState.relax)
//            {
//                _outputManager.AppendOutputs(" ", true);
//                Debug.Log($"CubeSpaceHandler - just appended space to outputs.");
//            }
//            else if (_currentRotationState == CubeRotationState.pronate)
//            {
//                if (!CubeController.Instance.isInputStreamReceiver && LextingOutputManager.Instance.CurrentOutputValue.Length == 0) return;
//                if (_input.currentRightHandGestureState == HandGestureState.Insert && _input.currentLeftHandGestureState == HandGestureState.Insert)
//                    _outputManager.SubtractWord(false);
//                else
//                {
//                    _outputManager.ReplaceOutput(1, "", "");
//                    //_outputManager.UpdateOutputs(
//                    //    _outputManager.CurrentOutputValue.Substring(0,
//                    //        _outputManager.CurrentOutputValue.Length - 1
//                    //    )
//                    //);
//                }
                   
//            }
//        }

//        // deprecated - find this functionality in OutputManager
//        public void Backword()
//        {
//            if (!_outputManager.Ready()) return;
//            _outputLength = _outputManager.CurrentOutputValue.Length;
            
//            if (_outputLength == -1)
//                _outputManager.UpdateOutputs("");
//            else {
//                int remainder = _outputLength - 
//                    Mathf.Clamp(_outputManager.CurrentOutputValue.LastIndexOf(' '), 0, _outputManager.CurrentOutputValue.Length);

//                if (remainder > 0)
//                {
//                    remainder--;
//                    _outputManager.UpdateOutputs(_outputManager.CurrentOutputValue.Substring(0, _outputLength - remainder));
//                }
//                else _outputManager.UpdateOutputs("");
//            }
//            _spaceHandlerActive = false;
//            //resetTrailingSpace = true;
//            this.WaitS(0.1f, () => _spaceHandlerActive = true);
//        }
//        #endregion helpers

//        #region Unit Tests
//        public void UnitTest_Backword()
//        {
//            //Backword();
//            _outputManager.SubtractWord(true);
//        }
//        #endregion Unit Tests
//    }
//}

