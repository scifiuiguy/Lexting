using SpatialGestureText.TwoZoneCube;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.QWERTY_Demo
{
	public class DemoPointerReceiver : MonoBehaviour
	{
		#region vars
		#region public vars

		#endregion public vars
		#region private vars
		public BoxCollider collider;
		public TwoZoneTriggerHighlight highlight;
		#endregion private vars
		#endregion vars
		
		#region init
		private void Awake()
		{
			this.Ch(ref collider, ref highlight);
			if (transform.name.Equals("Right_Controller"))
				highlight = null;
		}
		#endregion init
	
		#region loops
		
		#endregion loops
		
		#region event handlers
		
		#endregion event handlers
		
		#region helper methods
		
		#endregion helper methods
		
		#region Unit Tests
		
		#endregion Unit Tests
	}
}
