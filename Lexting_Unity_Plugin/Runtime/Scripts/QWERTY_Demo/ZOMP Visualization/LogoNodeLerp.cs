using Pixelplacement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.QWERTY_Demo
{
	public class LogoNodeLerp : MonoBehaviour
	{
		#region vars
		#region public vars

		#endregion public vars
		#region private vars
		Vector3 startingLocalPos;
		[SerializeField] private float delay;
		#endregion private vars
		#endregion vars
		
		#region init
		private void Awake()
		{
			startingLocalPos = transform.localPosition;
			transform.localPosition = Vector3.zero;
			this.WaitS(3, Activate);
		}
		#endregion init

		#region loops

		#endregion loops

		#region event handlers

		#endregion event handlers

		#region helper methods
		public void Activate()
		{
			Tween.LocalPosition(transform, startingLocalPos, 0.5f, delay, Tween.EaseSpring, Tween.LoopType.None, null, () =>
			{

			});
		}
		#endregion helper methods

		#region Unit Tests

		#endregion Unit Tests
	}
}
