//using SpatialGestureText.Input;
//using SpatialGestureText.Transformation;
//using SpatialGestureText.Utilities;
//using SpatialGestureText.Zones;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//namespace SpatialGestureText.LetterCube
//{
//    /// <summary>
//    /// DEPRECATED
//    /// </summary>
//    public class CubeCharDisplayController : MonoBehaviour
//    {
//        #region vars
//        [HideInInspector] public CubeController controller;
//        private SpatialGestureInput _input;
//        private CanvasGroup[] _canvasGroups;
//        private Text[][] _textDisplays;
//        private CubeLetterTransition[][] _textTransitions;
//        private LetterScaleHandler[] _alphaScaleHandlers, _numericScaleHandlers, _specialScaleHandlers;
//        [SerializeField] private bool rightHand;
//        private bool _isInitialized;
//        #endregion vars

//        #region init
//        private void Awake()
//        {
//            if (!_isInitialized)
//                InitializeCharDisplayController();

//            InitMarker<AlphaDisplayMarker>(ref _alphaScaleHandlers);
//            InitMarker<NumericDisplayMarker>(ref _numericScaleHandlers);
//            InitMarker<SpecialCharDisplayMarker>(ref _specialScaleHandlers);
//            this.Subscribe(_input?.OnRightRotationStateChanged, OnRightRotationStateChanged);
//            this.Subscribe(_input?.OnLeftRotationStateChanged, OnLeftRotationStateChanged);
//        }

//        private void InitMarker<T>(ref LetterScaleHandler[] var0) where T : MonoBehaviour
//        {
//            T marker = this.Ch<T>();
//            marker.enabled = true;
//            var0 = marker.transform.GetComponentsInChildren<LetterScaleHandler>(true);
//            marker.enabled = false;
//        }

//        private void InitializeCharDisplayController()
//        {
//            this.Chs(ref _canvasGroups);
//            _input = SpatialGestureInput.Instance;

//            // 3 instances of this class per CubeController * 3 types (letter/num/special) * 9 elements per type 
//            // each CubeCharDisplayController handles 1 layer of the cube
//            _textDisplays = new Text[3][] {
//                                    transform.GetChild(0).GetComponentsInChildren<Text>(), // letters
//                                    transform.GetChild(1).GetComponentsInChildren<Text>(), // numbers
//                                    transform.GetChild(2).GetComponentsInChildren<Text>(), // special chars
//            };
//            _textTransitions = new CubeLetterTransition[3][] {
//                                    transform.GetChild(0).GetComponentsInChildren<CubeLetterTransition>(), // leters
//                                    transform.GetChild(1).GetComponentsInChildren<CubeLetterTransition>(), // numbers
//                                    transform.GetChild(2).GetComponentsInChildren<CubeLetterTransition>() // special chars
//            };
//            _isInitialized = true;
//        }
//        #endregion init

//        #region event handlers
//        private void OnRightRotationStateChanged(CubeRotationState newState, CubeRotationState previousState)
//        {
//            if (Lexting.assembler.isWordAssemblyActive || newState == previousState) return;
//            HandleFullCubeCaseToggling(true, newState);
//            ResetState(newState);
//        }

//        private void OnLeftRotationStateChanged(CubeRotationState newState, CubeRotationState previousState)
//        {
//            if (Lexting.assembler.isWordAssemblyActive || newState == previousState) return;
//            HandleFullCubeCaseToggling(false, newState);
//            ResetState(newState);
//        }
//        #endregion event handlers

//        #region helpers
//        public void Initialize()
//        {
//            ResetState((rightHand) ? Lexting.rhRotState : Lexting.lhRotState);
//        }

//        private void ToggleChild(bool on, int index)
//        {
//            if (!_isInitialized)
//                InitializeCharDisplayController();
//            //Debug.Log($"CubeCharDisplay index: {index} canvasGroups.Length: {_canvasGroups.Length}");
//            _canvasGroups[index].alpha = (on) ? 1 : 0;
//            transform.GetChild(index).gameObject.SetActive(on);
//        }

//        public void ToggleAllChildren(bool on)
//        {
//            for (int i = 0; i < transform.childCount; i++)
//                ToggleChild(on, i);
//        }

//        private void ResetState(CubeRotationState newState)
//        {
//            ToggleAllChildren(false);
//            switch (newState)
//            {
//                case CubeRotationState.relax:
//                    ToggleChild(true, 0);
//                    break;
//                case CubeRotationState.pronate:
//                    ToggleChild(true, 0);
//                    break;
//                case CubeRotationState.forward:
//                    ToggleChild(true, 1);
//                    break;
//                case CubeRotationState.supinate:
//                    ToggleChild(true, 2);
//                    break;
//            }
//        }
        
//        public void ToggleCaseForAllTriggers(bool upper)
//        {
//            foreach (Text display in _textDisplays[0])
//                display.text = (upper) ? display.text.ToUpper() : display.text.ToLower();
//        }

//        /// <summary>
//        /// Causes all letters to display uppercase only if
//        /// both hands are in uppercase mode, while also ensuring all letters display lowercase
//        /// if even one hand is in lowercase mode.
//        /// </summary>
//        /// <param name="right">If true, right hand caused the new state, otherwise left hand did.</param>
//        /// <param name="newState">the new state the hand just changed to</param>
//        private void HandleFullCubeCaseToggling(bool right, CubeRotationState newState)
//        {
//            CubeRotationState otherHandState = right ? _input.currentLeftRotationState : _input.currentRightRotationState;
//            //Debug.Log($"CubeCharDisplayController - HandleFullCubeCaseToggline - hand {right} - newState {newState} - otherState {otherHandState}");
//            if (newState == CubeRotationState.pronate && otherHandState == CubeRotationState.pronate)    
//                ToggleCaseForAllTriggers(true);
//            else if (newState == CubeRotationState.relax || otherHandState == CubeRotationState.relax)
//                ToggleCaseForAllTriggers(false);
//        }

//        public void ToggleVisibility(bool on)
//        {
//            foreach (CanvasGroup group in _canvasGroups)
//                group.alpha = (on) ? 1 : 0;
//        }

//        //public void EnlargeCurrent(int typeIndex, int elementIndex)
//        //{
//        //    _textTransitions[typeIndex][elementIndex].ChangeSize(LetterSize.hovered);
//        //}

//        public void Hover(bool on, int typeIndex, int elementIndex)
//        {
//            switch (typeIndex)
//            {
//                case 0:
//                    _alphaScaleHandlers[elementIndex].Hover(on, null);
//                    break;
//                case 1:
//                    _numericScaleHandlers[elementIndex].Hover(on, null);
//                    break;
//                case 2:
//                    _specialScaleHandlers[elementIndex].Hover(on, null);
//                    break;
//            }
//        }

//        public void Bump(int typeIndex, int elementIndex)
//        {
//            switch (typeIndex)
//            {
//                case 0:
//                    _alphaScaleHandlers[elementIndex].Bump(null);
//                    break;
//                case 1:
//                    _numericScaleHandlers[elementIndex].Bump(null);
//                    break;
//                case 2:
//                    _specialScaleHandlers[elementIndex].Bump(null);
//                    break;
//            }
//        }
//        #endregion helpers
//    }
//}