using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.Data
{
	/// <summary>
	/// A simple structure containing all the data pertaining to a grab/drag action with one hand
	/// </summary>
	public struct GrabData
	{
		/// <summary>
		/// the letter from the zone in which drag action began
		/// </summary>
		public string firstRight, secondRight, firstLeft, secondLeft;

		/// <summary>
		/// the first letter's sort index among the first 2, i.e. true if this hand began before the 
		/// other hand's first letter, false otherwise
		/// </summary>
		public GrabbedFirst firstPair, secondPair;
	}

	public enum GrabbedFirst
	{ 
		none,
		right,
		left
	}
}
