using SpatialGestureText.LetterCube;
using SpatialGestureText.Transformation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static SpatialGestureText.LetterCube.CubeTouchStates;
using SpatialGestureText.Tutorial;
using UnityEngine.Events;
using UniRx;
using UniRx.Triggers;

namespace SpatialGestureText.Assets.HandsFreeText.Scripts.Debugging
{
    class DummyHead : Singleton<MonoBehaviour>
    {
        public GameObject refFrameOrigin;
        private Vector3 _averageHandPos = Vector3.zero;

        //public bool unitTest_Calculate;

        private void Awake()
        {
            refFrameOrigin = new GameObject("RefFrameOrigin");
            //refFrameOrigin.transform.position = Vector3.zero;
            //refFrameOrigin.transform.SetParent(transform.parent);
        }

        //private void Update()
        //{
        //    if (unitTest_Calculate)
        //    {
        //        unitTest_Calculate = false;
        //        CalculateRefFrameOrigin();
        //    }
        //}

        /// <summary>
        /// Procedurally determine position of the proxy origin that should match the PC client such that the hands end up inside the cube
        /// when streamed to the PC.
        /// </summary>
        public void CalculateRefFrameOrigin()
        {
            // headset is mounted on a stand beside the user, angled 45 degrees toward user and down toward hands
            // the target reference frame is approximately the position of the user's naval
            // must be calculated procedurally because we don't know if user will set headset up to their right or left

            _averageHandPos = DummyLeft._.Instance.transform.position + (DummyRight._.Instance.transform.position - DummyLeft._.Instance.transform.position)/2;
            //refFrameOrigin.transform.position = _averageHandPos;

            // get head pos at hand height
            Vector3 _headPos = new Vector3(transform.position.x, _averageHandPos.y, transform.position.z);
            // get hypotenuse
            float magnitudeNearestHand2Head = Mathf.Abs((_headPos - _averageHandPos).magnitude);
            // get the opposite length of the right triangle, i.e. the component forward vector magnitude of the hand away from the body
            float refFrameDistance = Mathf.Sin(45) * magnitudeNearestHand2Head;
            // Find the direction of the user's body by yawing left if HMD is to user's left and right if HMD is to user's right
            float turnDirection = (RightIsNearest()) ? 1 : -1;
            // From the average hand position, turn 45 degrees in turnDirection to face the user
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(_headPos.x, _averageHandPos.y, _headPos.z) - _averageHandPos) * Quaternion.Euler(0, turnDirection * 45, 0);

            // the user's naval should be straight ahead of lookRotation, at a range of refFrameDistance - place the _refFrame origin there 
            //refFrameOrigin.transform.position = DummyLeft._.Instance.transform.position;
            refFrameOrigin.transform.position = _averageHandPos + (lookRotation * Vector3.forward).normalized * refFrameDistance - new Vector3(0,0.2f,0);
            refFrameOrigin.transform.rotation = lookRotation * Quaternion.Euler(0,180,0);
        }

        /// <summary>
        /// Returns true if right is closer to head, otherwise false
        /// </summary>
        private bool RightIsNearest()
        {
            return Mathf.Abs((DummyRight._.Instance.transform.position - transform.position).magnitude) <
                Mathf.Abs((DummyLeft._.Instance.transform.position - transform.position).magnitude);
        }
    }
}
