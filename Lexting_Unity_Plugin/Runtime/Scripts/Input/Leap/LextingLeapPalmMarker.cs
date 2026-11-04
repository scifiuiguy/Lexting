#if LEXTING_FOUND_LEAP_SDK
using Leap.Unity.Attachments;
using SpatialGestureText.Debugging;
using SpatialGestureText.LetterCube;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using SpatialGestureText.Utilities;
using SpatialGestureText.Interfaces;

namespace SpatialGestureText.Input.Leap
{
	public class LextingLeapPalmMarker : MonoBehaviour, IHandPointNode
	{
		#region vars
		#region public vars
		/// <summary>
		/// if true, this is the right palm, otherwise left palm
		/// </summary>
		public bool right { get; set; }
		public Vector3 palmNormal => -1 * transform.up;
		#endregion public vars
		#region private vars
		private string _logOutput;

		/// <summary>
		/// the middle finger tip of the opposite hand
		/// </summary>
		private Transform _oppositeFingertip;

		private AttachmentHands _bothHands;

		private AttachmentHand _thisHand;

		private AttachmentHand[] _bothHandArray = new AttachmentHand[2];

		/// <summary>
		/// the float distance between this palm and the other hand's middle finger
		/// </summary>
		private float _palmOpMiddleDistance => Mathf.Abs((transform.position - _oppositeFingertip?.position ?? Vector3.zero).magnitude);

		/// <summary>
		/// above the threshold?
		/// </summary>
		private bool _palmOpMiddleGreater => _palmOpMiddleDistance > _magnitudeThreshold;

		/// <summary>
		/// below the theshold?
		/// </summary>
		private bool _palmOpMiddleLess => _palmOpMiddleDistance < _magnitudeThreshold;

		/// <summary>
		/// Is opposite middle finger close enough to the palm?
		/// </summary>
		private bool _triggerConditions => _palmOpMiddleLess && _gestureState != HandGestureState.Insert
			&& Lexting.input.inputActive;

		/// <summary>
		/// Is the middle finger far away enough yet for us to start checking when it may touch again?
		/// </summary>
		private bool _triggerReady => _palmOpMiddleGreater && _gestureState != HandGestureState.Insert;

		private HandGestureState _gestureState => Lexting.handState(right);

		/// <summary>
		/// The threshold beyond which the opposite middle finger's distance from the palm will trigger an event
		/// </summary>
		[SerializeField] private float _magnitudeThreshold = 0.05f;

		/// <summary>
		/// The time interval that must elapse after the palm touch event triggers before it can be triggered again
		/// </summary>
		[SerializeField] private float _triggerResetInterval = 0.01f;
		#endregion private vars
		#endregion vars

		#region init
		private void Awake()
		{
			FindOtherHandMiddleFinger();
            this.Wait(() => Lexting.input.inputActive, () =>this.WaitS(1, () =>this.Wait(() => _triggerReady, () => CheckForTrigger())));	
		}

		private void FindOtherHandMiddleFinger()
		{
			this.Wait(() => Lexting.input.hand(right).middleTip.Ready(), () => {
				_oppositeFingertip = Lexting.input.hand(!right).middleTip;
			});
		}
		#endregion init

		#region loops
		private void Update()
        {
			CheckRotationState((right) 
				? LeapHandInput.Instance.currentRightRotationState 
				: LeapHandInput.Instance.currentLeftRotationState, right);
		}
		#endregion loops

		#region event handlers
		private void CheckForTrigger()
		{
			this.Wait(() => _triggerConditions, () => OppositeMiddleToPalmTouch());
		}

		private void OppositeMiddleToPalmTouch()
		{			
			//Debug.Log($"Opposite-middle-to-palm-touch triggered @ time: {Time.time}");
			LeapHandInput.Instance.OppositeMiddleToPalmTouch?.I(right);
			this.WaitS(_triggerResetInterval, () => this.Wait(() => _triggerReady, () => CheckForTrigger()));
		}
		#endregion event handlers


		#region helper methods
		private void CheckRotationState(CubeRotationState state, bool right)
		{
			if (Vector3.Dot(palmNormal, LeapHandInput.Instance.transform.forward) > 0.6f)
				RefreshRotationState(state, CubeRotationState.forward, right);
			else if (Vector3.Dot(palmNormal, ((right) ? -1 : 1) * LeapHandInput.Instance.transform.right) > 0.85f)
				RefreshRotationState(state, CubeRotationState.relax, right);
			else if (Vector3.Dot(palmNormal, Vector3.up) > 0.65f)
				RefreshRotationState(state, CubeRotationState.supinate, right);
			else if (Vector3.Dot(palmNormal, Vector3.down) > 0.65f)
				RefreshRotationState(state, CubeRotationState.pronate, right);
			else if (Vector3.Dot(palmNormal, -1 * LeapHandInput.Instance.transform.forward) > 0.75f)
				RefreshRotationState(state, CubeRotationState.other, right);

   //         if (right) return;
   //         _logOutput = "% from " + ((right) ? "Right: " : "Left: ") +
   //             $"{Vector3.Dot(palmNormal, ((right) ? -1 : 1) * LeapHandInput.Instance.transform.right)}\n" +
   //             $"% from up: {Vector3.Dot(palmNormal, Vector3.up)}\n" +
   //             $"% from down: {Vector3.Dot(palmNormal, Vector3.down)} " +
   //             $"% from forward: {Vector3.Dot(palmNormal, LeapHandInput.Instance.transform.forward)}";
			//_logOutput = ((right) ? "Right: " : "Left: \n") +
			//	$"Palm to opposite middle finger: {_palmOpMiddleDistance}\n";
			//LogOutputReceiver.Instance.Log(_logOutput);
        }

		private void RefreshRotationState(CubeRotationState state, CubeRotationState targetState, bool right)
		{
			if (state == targetState) return;
			else LeapHandInput.Instance.PalmRotationStateChanged(right, targetState);
		}
        #endregion helper methods

    }
}
#endif
