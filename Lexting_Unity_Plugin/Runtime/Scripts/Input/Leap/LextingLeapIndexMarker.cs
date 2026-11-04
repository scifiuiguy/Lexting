#if LEXTING_FOUND_LEAP_SDK
using Leap.Unity.Attachments;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using SpatialGestureText.Debugging;
using SpatialGestureText.LetterCube;
using SpatialGestureText.Interfaces;

namespace SpatialGestureText.Input.Leap
{
	[System.Obsolete("Leap reference template only; not a shipping path.")]
	public class LextingLeapIndexMarker : MonoBehaviour, IHandPointNode
	{
		#region vars
		#region private vars

		/// <summary>
		/// The spatial node of the palm matching the same hand as this thumb
		/// </summary>
		private Transform _leapIndexBase, _leapMiddleTip;

		/// <summary>
		/// The threshold beyond which the thumb's distance from the palm will trigger a thumb-open event
		/// </summary>
		[SerializeField] private float _magnitudeThreshold = 0.07f;

		/// <summary>
		/// The time interval that must elapse after the ThumbOut even triggers before it can be triggered again
		/// </summary>
		[SerializeField] private float _triggerResetInterval = 1f;

		private bool _triggerReady => _baseIndexLess && _baseMiddleLess;

		private float _baseIndexDistance => 
			(_leapIndexBase == null) ? 0 : (transform.position - _leapIndexBase.position).magnitude;
		private bool _baseIndexGreater => _baseIndexDistance > _magnitudeThreshold;

		/// <summary>
		/// if true, index finger just retracted below the threshold to reset
		/// </summary>
		private bool _baseIndexLess => _baseIndexDistance < _magnitudeThreshold;

		private bool _triggerConditions => _baseIndexGreater && _baseMiddleLess;

		private float _baseMiddleDistance =>
			(_leapIndexBase == null) ? 0 : (_leapMiddleTip.position - _leapIndexBase.position).magnitude;
		private bool _baseMiddleGreater => _baseMiddleDistance > _magnitudeThreshold;
		private bool _baseMiddleLess => _baseMiddleDistance < _magnitudeThreshold;

		private string _logOutput = "";
		#endregion prviate vars
		#region public vars
		/// <summary>
		/// if true, this is the right hand's thumbe, otherwise left
		/// </summary>
		public bool right { get; set; }
		#endregion public vars
		#endregion vars

		#region init
		private void Awake()
		{
			FindIndexBase();
			FindMiddleTip();


		}

        private void Start()
        {
			this.Wait(() => _triggerReady, () => CheckForTrigger());

			//LogIndexAndMiddleDistanceContinuous();
		}
		#endregion init

		#region event handlers
		private void CheckForTrigger()
		{
			this.Wait(() => _triggerConditions, () => IndexPointing());
		}
        private void IndexPointing()
		{
			Debug.Log($"Index point triggered @ time: {Time.time}");
			LeapHandInput.Instance.IndexPointing?.I(right);
			this.WaitS(_triggerResetInterval, () => this.Wait(() => _triggerReady, ()=> CheckForTrigger()));
		}
		#endregion event handlers

		#region helper methods
		private void FindIndexBase()
		{
			_leapIndexBase = transform.GG<AttachmentHand>().GetComponentInChildren<LextingLeapIndexBaseMarker>().transform;
		}

		private void FindMiddleTip()
		{
			_leapMiddleTip = transform.GG<AttachmentHand>().GetComponentInChildren<LextingLeapFingertipMarker>().transform;
		}
		#endregion helper methods

		#region Debugging
		private void LogIndexAndMiddleDistanceContinuous()
		{
			this.UpdateAsObservable().Subscribe(_ => {
				if (right) return;
				_logOutput = ((right) ? "Right Hand " : "Left Hand ") + "Index Pointing Trigger Check: " +
					$"Index finger to base: {_baseIndexDistance}\n" +
					$"Middle to base: {_baseMiddleDistance} ";

				LogOutputReceiver.Instance.Log(_logOutput);
			});
		}
        #endregion Debugging
    }
}
#endif
