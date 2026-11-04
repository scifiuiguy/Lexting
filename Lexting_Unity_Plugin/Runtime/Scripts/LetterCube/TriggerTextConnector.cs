//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace SpatialGestureText.LetterCube
//{
//	/// <summary>
//	///// DEPRECATED
//	/// </summary>
//	public class TriggerTextConnector : MonoBehaviour
//	{
//        #region vars
//        #region public vars

//        #endregion public vars
//        #region private vars
//        private CubeTrigger[] _triggers;
//        private CubeTextZone[] _textZones;
//        private CubeCharDisplayController _charDisplayController;
//		#endregion private vars
//		#endregion vars
		
//		#region init
//		private void Awake()
//		{
//            _charDisplayController = GetComponentInChildren<CubeCharDisplayController>();
//            this.Chs(ref _triggers, ref _textZones);
//            this.FE(ref _triggers, trigger => {
//                this.FE(ref _textZones, textZone => {
//                    if (textZone.transform.name.Equals(trigger.transform.name.Substring(1, 3)))
//                    {
//                        textZone.trigger = trigger;
//                        trigger.textZone = textZone;
//                        trigger.charDisplayController = _charDisplayController;
//                    }
//                });
//            });
//		}
//		#endregion init
	
//		#region loops
		
//		#endregion loops
		
//		#region event handlers
		
//		#endregion event handlers
		
//		#region helper methods
		
//		#endregion helper methods
		
//		#region Unit Tests
		
//		#endregion Unit Tests
//	}
//}
