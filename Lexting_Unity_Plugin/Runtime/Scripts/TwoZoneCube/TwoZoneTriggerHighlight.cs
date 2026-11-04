using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.TwoZoneCube
{
	public class TwoZoneTriggerHighlight : MonoBehaviour
	{
        #region vars
        #region public vars

        #endregion public vars
        #region private vars
        private MeshRenderer _renderer;
		#endregion private vars
		#endregion vars
		
		#region init
		private void Awake()
		{
            _renderer = GetComponent<MeshRenderer>();
		}
        #endregion init
        
        #region helper methods
        public void Toggle(bool on)
        {
            _renderer.enabled = on;
        }
		#endregion helper methods
	}
}
