#if LEXTING_FOUND_LEAP_SDK
using SpatialGestureText.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.Input.Leap
{
	[System.Obsolete("Leap reference template only; not a shipping path.")]
	public class LextingLeapIndexBaseMarker : MonoBehaviour, IHandPointNode
	{
		public bool right { get; set; }
	}
}
#endif
