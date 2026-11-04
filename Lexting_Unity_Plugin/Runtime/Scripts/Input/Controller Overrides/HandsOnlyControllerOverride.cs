using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.Input
{
	public class HandsOnlyControllerOverride : MonoBehaviour
	{
		#region vars
		#region public vars
		public bool iWantToUseMyOwnController;
		public Transform leftHand;
		public Transform rightHand;
		public LextingInputModuleManager manager;
		#endregion public vars
		#endregion vars
	}
}
