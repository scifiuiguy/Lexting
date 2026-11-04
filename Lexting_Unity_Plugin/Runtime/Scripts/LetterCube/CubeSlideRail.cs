//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace SpatialGestureText.LetterCube
//{
//    /// VERY DEPRECATED
//    /// <summary>
//    /// Updates the YZ position of the interface to display the appropriate letter column
//    /// </summary>
//    public class CubeSlideRail : Singleton<CubeSlideRail>
//    {
//        #region vars
//        /// <summary>
//        /// top front to bottom back where x is forward/backward space, and y is column space
//        /// e.g. A is (0,0) and Q is (5,4)
//        /// </summary>
//        public CubeLetterGridIndex currentIndex;

//        /// <summary>
//        /// the difference between home position (E) and the position of the current highlight letter
//        /// </summary>
//        public Vector3 currentLetterOffset;

//        /// <summary>
//        /// the destination (i.e. the center of a selected letter volume) to which the slide rail is 
//        /// continuously attempting to move itself
//        /// </summary>
//        public Vector3 forceTarget;

//        /// <summary>
//        /// If true, the user has activated a non-Neutral gesture and is attempting to control the interface
//        /// </summary>
//        public bool isUnderControl;

//        /// <summary>
//        /// softens the speed and jitter with which the slide rail moves toward the forceTarget
//        /// </summary>
//        [SerializeField] private float _dampener;

//        /// <summary>
//        /// the component that allows the user to input gestures
//        /// </summary>
//        private CubeController _controller;

//        /// <summary>
//        /// the world space position of the hand when a gesture begins
//        /// </summary>
//        private Vector3 _startPosition;

//        /// <summary>
//        /// the depth interval between letter columns
//        /// </summary>
//        private float _zDepth = 1.75f;

//        /// <summary>
//        /// the height interval between letter rows
//        /// </summary>
//        private float _yHeight = 3f;
//        public float yHeight
//        {
//            get
//            {
//                return _yHeight;
//            }
//        }

//        /// <summary>
//        /// the depth of all letter columns added together
//        /// </summary>
//        private float _totalDepth
//        {
//            get
//            {
//                return _zDepth * 5;
//            }
//        }

//        /// <summary>
//        /// the height of all letter rows added together
//        /// </summary>
//        private float _totalHeight
//        {
//            get
//            {
//                return _yHeight * 4;
//            }
//        }
//        #endregion vars

//        #region init
//        private void Awake()
//        {
//            _controller = FindObjectOfType(typeof(CubeController)) as CubeController;
//            if (_controller == null)
//                Debug.LogError("SlideRail couldn't locate the CubeController. Is it instanced?");

//            _startPosition = new Vector3(transform.position.x, transform.position.y - _yHeight, transform.position.z);
//        }
//        #endregion init

//        #region loops
//        private void Update()
//        {
//            forceTarget = _startPosition + new Vector3(0, currentIndex.row * _totalHeight / ((currentIndex.column < 4) ? 4 : 5), -1 * currentIndex.column * _zDepth);
//            transform.position = Vector3.Lerp(transform.position, forceTarget, _dampener);
//        }
//        #endregion loops
//    }
//}


