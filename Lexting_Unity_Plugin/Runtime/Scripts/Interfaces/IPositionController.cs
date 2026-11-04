using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.Interfaces
{
	/// <summary>
	/// Enables classes to access the position controller whether it is for a two-by-two-by-two or a three-by-three-by-three cube
	/// </summary>
	public interface IPositionController 
	{
		bool hasHover { get; }
	}
}
