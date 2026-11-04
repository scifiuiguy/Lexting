#if LEXTING_FOUND_LEAP_SDK
using SpatialGestureText.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using SpatialGestureText.Interfaces;

namespace SpatialGestureText.Input.Leap
{
	public class LextingLeapFingertipMarker : MonoBehaviour, IHandPointNode
	{
		#region vars
		#region public vars
		public bool right { get; set; }
		//public SphereCollider collider;
		#endregion public vars
		#region private vars
		private Transform _palm => Lexting.input.hand(right).palm;

        private float _palm2MiddleTipDistance => Mathf.Abs((transform.position - _palm.position).magnitude);

        private bool _palmMiddleLess => _palm2MiddleTipDistance < _magnitudeThreshold;

        private bool _grabConditions => _palmMiddleLess;

        private bool _releaseConditions => !_palmMiddleLess;

        private float _magnitudeThreshold = 0.05f;
        #endregion private vars
        #endregion vars

        #region init
  //      private void Awake()
		//{
		//	this.Ch(ref collider);
		//	this.HandleTriggerWithTag((right) ? "right_leap_palm" : "left_leap_palm",
		//		(otherObject) => LeapHandInput.Instance.IndexStateChanged(true, right), //entry
		//		(otherObject) => LeapHandInput.Instance.IndexStateChanged(false, right) //exit
		//	);
		//}

        private void Start()
        {
            this.Wait(() => Lexting.inputManager.Ready(), ()=> {
                this.Wait(() => !_palmMiddleLess, () => this.Wait(() => _grabConditions, TriggerGrab));
            });
        }
        #endregion init

        #region event handlers
        private void TriggerGrab()
        {
            Debug.Log($"Leap fingertip tracker triggered {((right)?"right":"left")} hand grab.");
            LeapHandInput.Instance.IndexStateChanged(true, right);
            this.Wait(()=>_releaseConditions, TriggerRelease);
        }

        private void TriggerRelease()
        {
            Debug.Log($"Leap fingertip tracker triggered {((right) ? "right" : "left")} hand release");
            LeapHandInput.Instance.IndexStateChanged(false, right);
            this.Wait(()=>_grabConditions, TriggerGrab);
        }
        #endregion event handlers
    }
}
#endif
