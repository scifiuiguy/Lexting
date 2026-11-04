using Pixelplacement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.QWERTY_Demo
{
	public class LogoTopNodeLerp : MonoBehaviour
	{
		#region vars
		#region public vars
		
		#endregion public vars
		#region private vars
		
		#endregion private vars
		#endregion vars
		
		#region init
		private void Awake()
		{
			Activate();
		}
		#endregion init

		#region loops

		#endregion loops

		#region event handlers

		#endregion event handlers

		#region helper methods
		public void Activate()
		{
			Tween.LocalScale(transform, new Vector3(4.08125f, transform.localScale.y, transform.localScale.z), 1f, 4, Tween.EaseIn, Tween.LoopType.None,
				null, () => {
					//Tween.LocalPosition(transform, transform.localPosition + transform.right * 0.3f * ((forward) ? 1 : -1), 1, 0);
					Tween.Rotate(transform, new Vector3(0, 0, 20), Space.Self, 0.5f, 0.2f, Tween.EaseInOut, Tween.LoopType.None, null, () => {
						Tween.Rotate(transform, new Vector3(0, 45, 0), Space.Self, 0.5f, 0.2f, Tween.EaseInOut, Tween.LoopType.None, null, () => {

						});
					});
				});


		}
		#endregion helper methods

		#region Unit Tests

		#endregion Unit Tests
	}
}
