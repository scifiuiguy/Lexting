using Pixelplacement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.QWERTY_Demo
{
	public class MatrixLerp : MonoBehaviour
	{
		#region vars
		#region public vars

		#endregion public vars
		#region private vars
		[SerializeField] bool forward;
		[SerializeField] bool moveOnDepth;
		#endregion private vars
		#endregion vars
		#region init
		private void Awake()
		{
			this.WaitS(5, Activate);
		}
		#endregion init

		#region loops

		#endregion loops

		#region event handlers

		#endregion event handlers

		#region helper methods
		public void Activate()
		{
			Tween.LocalScale(transform, new Vector3(0.15f, transform.localScale.y, transform.localScale.z), 1, 0, Tween.EaseInOut,Tween.LoopType.None,
				null, ()=>{
					if (!moveOnDepth) return;
					Tween.LocalPosition(transform, transform.localPosition + transform.right * 0.3f * ((forward) ? 1 : -1), 1, 0);
				});
		}
		#endregion helper methods

		#region Unit Tests

		#endregion Unit Tests
	}
}
