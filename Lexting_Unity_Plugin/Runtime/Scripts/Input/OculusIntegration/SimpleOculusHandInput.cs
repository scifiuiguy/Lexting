#if LEXTING_FOUND_OCULUS_SDK && (UNITY_EDITOR || UNITY_STANDALONE)
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OVRPlugin;

namespace SpatialGestureText.Input.OculusIntegration
{
	/// <summary>
	/// Legacy OVR skeleton debug helper. Deprecated; greenfield Quest builds omit <c>LEXTING_FOUND_OCULUS_SDK</c>.
	/// </summary>
	[System.Obsolete("OVR skeleton debug path; use OpenXRHandInput + Unity XR Hands.")]
	public class SimpleOculusHandInput : Singleton<SimpleOculusHandInput>
	{
		#region vars
		#region public vars
		public SpatialGestureEvent<bool> OppositeMiddleToPalmTouch = new SpatialGestureEvent<bool>();
		#endregion public vars
		#region private vars
		/// <summary>
		/// A shortcut reference to the palm
		/// </summary>
		private Transform _palm(bool right) => _skeleton(right).Bones[(int)BoneId.Hand_WristRoot].Transform;

		private Transform _mTip(bool right) => _skeleton(right).Bones[(int)BoneId.Hand_MiddleTip].Transform;

		/// These determine whether the user is performing the "BackWord" gesture (opposite middle finger touches palm)
		#region BackWord vars
		/// <summary>
		/// the float distance between this palm and the other hand's middle finger
		/// </summary>
		private float _palmOpMiddleDistance(bool right) =>
			Mathf.Abs((_palm(right).position - (_mTip(!right)?.position ?? Vector3.zero)).magnitude);

		/// <summary>
		/// above the threshold?
		/// </summary>
		private bool _palmOpMiddleGreater(bool right) => _palmOpMiddleDistance(right) > _backwordMagnitudeThreshold;

		/// <summary>
		/// below the theshold?
		/// </summary>
		private bool _palmOpMiddleLess(bool right) => _palmOpMiddleDistance(right) < _backwordMagnitudeThreshold;

		/// <summary>
		/// Is opposite middle finger close enough to the palm?
		/// </summary>
		private bool _backwordTriggerConditions(bool right) => _palmOpMiddleLess(right);

		/// <summary>
		/// Is the middle finger far away enough yet for us to start checking when it may touch again?
		/// </summary>
		private bool _backwordTriggerReady(bool right) => _palmOpMiddleGreater(right);


		/// <summary>
		/// The threshold beyond which the opposite middle finger's distance from the palm will trigger an event
		/// </summary>
		[SerializeField] private float _backwordMagnitudeThreshold = 0.05f;

		/// <summary>
		/// The time interval that must elapse after the palm touch event triggers before it can be triggered again
		/// </summary>
		[SerializeField] private float _backwordTriggerResetInterval = 0.01f;

		#endregion Backword vars

		private OVRCustomSkeleton _lSkeleton, _rSkeleton;

		private OVRCustomSkeleton _skeleton(bool right) => (right) ? _rSkeleton : _lSkeleton;
		#endregion private vars
		#endregion vars

		#region init
		private void Awake()
		{
			_rSkeleton = RightDebugHand.Instance.transform.GetComponentInChildren<OVRCustomSkeleton>();
			_lSkeleton = LeftDebugHand.Instance.transform.GetComponentInChildren<OVRCustomSkeleton>();
		}
		#endregion init

		#region loops

		#endregion loops

		#region event handlers

		#endregion event handlers

		#region helper methods
		private void BackwordTriggerCheck(bool right)
		{
			//if (!right)
			//    this.UpdateAsObservable().Subscribe(_ => {
			//        LogOutputReceiver.Instance.Log($"_palmOpMiddleDistance: {_palmOpMiddleDistance(false)}\n" +
			//            $"_backwordTriggerConditions met? {_backwordTriggerConditions(false)}");
			//    });
			this.Wait(() => _backwordTriggerConditions(right), () => OpMiddleToPalmTouch(right));
		}

		private void OpMiddleToPalmTouch(bool right)
		{
			//Debug.Log($"Opposite-middle-to-palm-touch triggered @ time: {Time.time}");
			//LogOutputReceiver.Instance.Log($"Opposite-middle-to-palm-touch triggered @ time: {Time.time}");
			OppositeMiddleToPalmTouch?.I(right);
			this.WaitS(_backwordTriggerResetInterval, () => this.Wait(() => _backwordTriggerReady(right), () => BackwordTriggerCheck(right)));
		}
		#endregion helper methods

		#region Unit Tests

		#endregion Unit Tests
	}
}
#endif
