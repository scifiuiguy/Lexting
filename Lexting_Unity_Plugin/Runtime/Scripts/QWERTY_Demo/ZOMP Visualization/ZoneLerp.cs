using Pixelplacement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.QWERTY_Demo
{
	public class ZoneLerp : MonoBehaviour
	{
		#region vars
		#region public vars

		#endregion public vars
		#region private vars
		[SerializeField] bool forward;
		#endregion private vars
		#endregion vars

		#region init
		private void Awake()
		{
			this.WaitS(8, Activate);
		}
		#endregion init

		#region loops

		#endregion loops

		#region event handlers

		#endregion event handlers

		#region helper methods
		public void Activate()
		{
			Tween.LocalScale(transform, new Vector3(transform.localScale.x, transform.localScale.y, 7f), 1, 0, Tween.EaseInOut, Tween.LoopType.None,
				null, () => {
					Tween.LocalPosition(transform, transform.position + transform.forward * 0.2f * ((forward) ? 1:-1), 1, 0);
				});
		}
		#endregion helper methods

		#region Unit Tests

		#endregion Unit Tests
	}
}
