using Pixelplacement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.QWERTY_Demo
{
	public class WindowLerp : MonoBehaviour
	{
		#region vars
		#region public vars

		#endregion public vars
		#region private vars
		//private float zScale = 0;
		#endregion private vars
		#endregion vars
		
		#region init
		private void Awake()
		{
			this.WaitS(2, Activate);
		}
		#endregion init

		#region loops

		#endregion loops

		#region event handlers

		#endregion event handlers

		#region helper methods
		public void Activate()
		{
			Tween.LocalScale(transform, new Vector3(1, transform.localScale.y, transform.localScale.z), 1, 0);
		}
		#endregion helper methods
		
		#region Unit Tests
		
		#endregion Unit Tests
	}
}
