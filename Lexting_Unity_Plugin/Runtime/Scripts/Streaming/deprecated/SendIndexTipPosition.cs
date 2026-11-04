using SpatialGestureText.Input;
using SpatialGestureText.LetterCube;
using SpatialGestureText.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.Streaming
{
	public class SendIndexTipPosition : MonoBehaviour
	{
        [SerializeField] private int numMessagesPerSecond;

        /// <summary>
        /// if true, right hand, otherwise left
        /// </summary>
        [SerializeField] private bool right;

        private void Awake()
        {
            StartCoroutine(HandUpdateLoop());
        }
        
        private IEnumerator HandUpdateLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(1/numMessagesPerSecond);

                ClientSend.SendIndexTipPosition(right, (right) 
                    ? Lexting.head.refFrameOrigin.transform.InverseTransformPoint(
                        Lexting.rHandMarker.averageHandPos + Lexting.head.userRightDirection*-0.15f
                        /*+ ((!HeadMarker.Instance.RightIsNearest()) ? new Vector3(0, -0.05f, 0) : Vector3.zero)*/) 
                    : Lexting.head.refFrameOrigin.transform.InverseTransformPoint(
                        Lexting.lHandMarker.averageHandPos + Lexting.head.userRightDirection * 0.15f 
                        /*+ ((HeadMarker.Instance.RightIsNearest()) ? new Vector3(0,-0.05f,0):Vector3.zero)*/));
            }
        }
    }
}
