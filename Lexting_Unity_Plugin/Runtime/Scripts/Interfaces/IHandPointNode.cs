using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.Interfaces
{
	public interface IHandPointNode 
	{
		/// <summary>
		/// guarantees that any script interacting w/ a hand point node will have knowledge of the hand's chirality,
		/// where true means right hand and false means left
		/// </summary>
		bool right { get; set; }
	}
}
