//using SpatialGestureText.Input;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace SpatialGestureText.LetterCube
//{
//    /// <summary>
//    /// DEPRECATED
//    /// Toggles the letter grid visualization cubes on/off as the user transtions gestures from/to neutral position.
//    /// </summary>
//    public class CubeDisplayToggler : MonoBehaviour
//    {
//        private SpatialGestureInput _input;
//        private CubeRightMarker _rightMarker;
//        private CubeLeftMarker _leftMarker;
//        private bool _childrenOn;

//        private void Awake()
//        {
//            _input = SpatialGestureInput.Instance;
//            EventUtils.GetGestures(this, OnGestureChanged);
//        }


//        //private void OnEnable()
//        //{
//        //    _input.OnRightStateChanged += OnRightGestureChanged;
//        //    _input.OnLeftStateChanged += OnLeftGestureChanged;
//        //}

//        //private void OnDisable()
//        //{
//        //    _input.OnRightStateChanged -= OnRightGestureChanged;
//        //    _input.OnLeftStateChanged -= OnLeftGestureChanged;
//        //}

//        void OnRightGestureChanged(HandGestureState newState, HandGestureState previousState)
//        {
//            switch (newState)
//            {
//                case HandGestureState.Insert:
//                    _childrenOn = true;
//                    break;
//                case HandGestureState.Selection:
//                    _childrenOn = true;
//                    break;
//                case HandGestureState.DragSelection:
//                    _childrenOn = true;
//                    break;
//                case HandGestureState.Neutral:
//                    _childrenOn = false;
//                    break;
//            }
//            ToggleChildren(_childrenOn);
//        }

//        void OnLeftGestureChanged(HandGestureState newState, HandGestureState previousState)
//        {
//            switch (newState)
//            {
//                case HandGestureState.Insert:
//                    _childrenOn = true;
//                    break;
//                case HandGestureState.Selection:
//                    _childrenOn = true;
//                    break;
//                case HandGestureState.DragSelection:
//                    _childrenOn = true;
//                    break;
//                case HandGestureState.Neutral:
//                    _childrenOn = false;
//                    break;
//            }
//            ToggleChildren(_childrenOn);
//        }

//        private void OnGestureChanged(bool right, HandGestureState current, HandGestureState previous)
//        {
//            switch (current)
//            {
//                case HandGestureState.Insert:
//                    _childrenOn = true;
//                    break;
//                case HandGestureState.Selection:
//                    _childrenOn = true;
//                    break;
//                case HandGestureState.DragSelection:
//                    _childrenOn = true;
//                    break;
//                case HandGestureState.Neutral:
//                    _childrenOn = false;
//                    break;
//            }
//            ToggleChildren(_childrenOn);
//        }


//        void ToggleChildren(bool on)
//        {
//            foreach (Transform child in transform)
//                child.gameObject.SetActive(on);
//        }
//    }
//}

