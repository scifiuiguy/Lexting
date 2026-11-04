using SpatialGestureText.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SpatialGestureText.QWERTY_Demo
{
	public class QWERTYLerp : MonoBehaviour
	{
		#region vars
		#region public vars

		#endregion public vars
		#region private vars
		private Vector3 _startPos;
		#endregion private vars
		#endregion vars
		
		#region init
		private void Awake()
		{
			_startPos = transform.localPosition;
		}
		#endregion init

		#region event handlers

		#endregion event handlers

		#region helper methods
		public void MoveZ(float distance, UnityAction callback)
		{
			this.LerpForward(distance, 0.5f, callback);
		}

		public void MoveX(float distance, UnityAction callback)
		{
			
			this.LerpToPos(transform.position + transform.right*distance, 0.5f, callback);
		}

		private float _segmentDistance = 0.249f;
		public void MoveLeftSegment()
		{
			MoveZ(-1 * _segmentDistance *0.25f, () => MoveX(_segmentDistance, null));
		}

        public void MoveRightSegment()
        {
			MoveZ(_segmentDistance * 0.25f, () => MoveX(-1 * _segmentDistance, null));
        }

        public void ResetStartPos()
        {
			transform.localPosition = _startPos;
        }
		#endregion helper methods

		#region Unit Tests
		public void UnitTest_Left()
		{
			MoveLeftSegment();
		}

		public void UnitTest_Right()
		{
			MoveRightSegment();
		}


		#endregion Unit Tests
	}
}
