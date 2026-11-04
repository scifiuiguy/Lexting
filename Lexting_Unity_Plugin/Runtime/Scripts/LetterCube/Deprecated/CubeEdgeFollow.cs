//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace SpatialGestureText.LetterCube
//{
//    /// <summary>
//    /// DEPRECATED
//    /// Allows the edge graphic of the interface to follow the letter grid up and down
//    /// without moving forward/backward in local depth
//    /// </summary>
//    public class CubeEdgeFollow : MonoBehaviour
//    {
//        public Transform rail;
//        private float _offset;

//        private void Awake()
//        {
//            if (rail == null)
//                Debug.LogError("The EdgeFollow component is missing a reference to the rail object. Double check that it is assigned in Inspector.");
//            _offset = rail.GetComponent<CubeSlideRail>().yHeight;
//        }

//        private void Update()
//        {
//            if (rail != null)
//                transform.position = new Vector3(transform.position.x, rail.position.y - _offset, transform.position.z);
//        }
//    }
//}


