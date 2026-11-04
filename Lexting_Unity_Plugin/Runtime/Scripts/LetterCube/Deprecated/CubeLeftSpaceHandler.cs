//using SpatialGestureText.Input;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//namespace SpatialGestureText.LetterCube
//{
//    /// <summary>
//    /// deprecated - all handled in CubeSpaceHandler now
//    /// </summary>
//    public class CubeLeftSpaceHandler : Singleton<CubeLeftSpaceHandler>
//    {
//        #region vars
//        private SpatialGestureInput _input;
//        private Transform _spaceTransform;
//        private Transform _backspaceTransform;
//        private bool _spaceHandlerActive = true;
//        private int _outputLength;
//        #endregion vars

//        #region init
//        private void Awake()
//        {
//            _input = SpatialGestureInput.Instance;
//            _spaceTransform = transform.parent.GetChild(transform.parent.childCount - 1).GetChild(0).GetChild(transform.GetSiblingIndex()).GetChild(0);
//            if (_spaceTransform == null)
//                Debug.LogError("Space Handler couldn't locate the space bar transform");
//            _backspaceTransform = transform.parent.GetChild(transform.parent.childCount - 1).GetChild(0).GetChild(transform.GetSiblingIndex()).GetChild(1);
//            if (_backspaceTransform == null)
//                Debug.LogError("Space Handler couldn't locate the backspace transform");

//            this.Subscribe(_input.OnLeftRotationStateChanged, OnLeftRotationStateChanged);
//        }
//        #endregion init

//        #region event handlers
//        private void OnLeftRotationStateChanged(CubeRotationState newState, CubeRotationState previousState)
//        {
//            if (newState == CubeRotationState.pronate)
//            {
//                _backspaceTransform.gameObject.SetActive(true);
//            }

//            if (newState == CubeRotationState.relax)
//            {
//                Debug.Log("Lowercase callback turning off object named " + _backspaceTransform.gameObject.name);

//                // if user has activated Backword() at least once during the current rotation, add a trailing space
//                // when they rotate back to lowercase
//                if (CubeSpaceHandler.resetTrailingSpace)
//                {
//                    CubeSpaceHandler.resetTrailingSpace = false;
//                    LextingOutputManager.Instance.AppendOutputs(" ", true);
//                }
//                _backspaceTransform.gameObject.SetActive(false);
//            }
//        }
//        #endregion event handlers

//        #region helpers
//        public void Insert()
//        {
//            if (!_spaceHandlerActive) return;

//            if (LextingOutputManager.Instance.isActive)
//            {
//                if (_input.currentLeftRotationState == CubeRotationState.relax)
//                {
//                    LextingOutputManager.Instance.AppendOutputs(" ", true);
//                }
//                else if (_input.currentLeftRotationState == CubeRotationState.pronate && LextingOutputManager.Instance.CurrentOutputValue.Length > 0)
//                {
//                    if (_input.currentRightHandGestureState == HandGestureState.Insert && _input.currentLeftHandGestureState == HandGestureState.Insert)
//                        LextingOutputManager.Instance.SubtractWord(false);
//                    else
//                    {
//                       // LextingOutputManager.Instance.ReplaceOutput(1, "", currentWord);
//                        //LextingOutputManager.Instance.UpdateOutputs(
//                        //    LextingOutputManager.Instance.CurrentOutputValue.Substring(0,
//                        //        LextingOutputManager.Instance.CurrentOutputValue.Length - 1
//                        //    )
//                        //);
//                    }
                    
//                }
//            }
//        }

//        // deprecated
//        public void Backword()
//        {
//            _outputLength = LextingOutputManager.Instance.CurrentOutputValue.Length;
//            if (_outputLength == -1)
//                LextingOutputManager.Instance.UpdateOutputs("");
//            else
//            {
//                int remainder = _outputLength - LextingOutputManager.Instance.CurrentOutputValue.LastIndexOf(' ');
//                if (remainder > 0)
//                {
//                    remainder--;
//                    LextingOutputManager.Instance.UpdateOutputs(LextingOutputManager.Instance.CurrentOutputValue.Substring(0, _outputLength - remainder));
//                }
//                else LextingOutputManager.Instance.UpdateOutputs("");
//            }
//            _spaceHandlerActive = false;
//            //CubeSpaceHandler.resetTrailingSpace = true;
//            this.WaitS(0.1f, () => _spaceHandlerActive = true);
//        }
//        #endregion helpers
//    }
//}


