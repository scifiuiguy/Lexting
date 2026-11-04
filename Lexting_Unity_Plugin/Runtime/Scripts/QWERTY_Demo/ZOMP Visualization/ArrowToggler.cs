using Pixelplacement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.QWERTY_Demo
{
	public class ArrowToggler : Singleton<ArrowToggler>
	{
		private CanvasGroup current;

		#region helper methods
		public void Toggle(int index, bool on)
		{
			current = transform.GetChild(index).GetComponent<CanvasGroup>();
			Tween.CanvasGroupAlpha(current, (on) ? 1 : 0, 0.2f, 0);
		}
		#endregion helper methods
		

	}
}
