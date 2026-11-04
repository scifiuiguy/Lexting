using SpatialGestureText.Interfaces;
using SpatialGestureText.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace SpatialGestureText.Input
{
	public class HandMarker : MonoBehaviour
	{
		#region vars
		#region public vars
		public bool right;
		public GameObject triggerActivatorPrefab;
		public Vector3 palmNormal => ResolvePalmNormal();

		Vector3 ResolvePalmNormal()
		{
#if !UNITY_STANDALONE && UNITY_ANDROID
			var fromQuest = QuestHandPalmNormal.Compute(right, _hand, QuestHandPalmNormal.DefaultSource);
			if (fromQuest.sqrMagnitude > 1e-4f)
				return fromQuest.normalized;
#endif
			return LegacyCrossPalmNormal();
		}

		Vector3 LegacyCrossPalmNormal()
		{
			var knuckle = _knuckleLineDirection;
			var wristToTip = _wristToMiddleTipDirection;
			if (knuckle.sqrMagnitude < 1e-6f || wristToTip.sqrMagnitude < 1e-6f)
				return Vector3.zero;
			return ((right) ? 1 : -1) * Vector3.Cross(knuckle, wristToTip).normalized;
		}
		public const float handNoiseDampeningFactor = 0.2f;

		/// <summary>
		/// Enables HandCursor to sync position such that the hand can comfortably rotate 
		/// into various modes without moving the cursor too much by accient.
		/// </summary>
		public Vector3 averageHandPos
		{
			get
			{
#if !UNITY_STANDALONE
				if (_hand.indexBase.Ready())
					return _hand.indexBase.position;
				if (_hand.palm.Ready())
					return _hand.palm.position;
				if (_hand.trackingWrist.Ready())
					return _hand.trackingWrist.position;
				if (_hand.wrist.Ready())
					return _hand.wrist.position;
				return Vector3.zero;
#else
				return Vector3.zero;
#endif
			}
		}

		[SerializeField] public float horizontalOffset;
		[HideInInspector] public GameObject cursorAnchor;
		#endregion public vars
		#region private vars
		private XRInputModalityManager _xrMode;
		private IDisposable _update;
		private Transform _tracker;
		private LextingHand _hand => Lexting.input.hand(right);
		private Vector3 _knuckleLineDirection => 
			((_hand.indexBase.Ready() && _hand.middleBase.Ready())) ? (((_hand.indexBase.position) - _hand.middleBase.position).normalized) : Vector3.zero;
		private Vector3 _wristToMiddleTipDirection =>
			(_hand.wrist.Ready() && _hand.middleTip.Ready())
				? (_hand.wrist.position - _hand.middleTip.position).normalized
				: Vector3.zero;
		private Vector3 _newSmoothDestination;

		/// <summary>
		/// if true, no controller is present because this instance is just the input stream for a different client
		/// </summary>
		[SerializeField] private bool _remoteControlActive;
		#endregion private vars
		#endregion vars

		#region init
		private void Awake()
		{
			_xrMode = transform.GinG<XRInputModalityManager>();
			//triggerActivator = Instantiate(Resources.Load("") triggerActivatorPrefab);
			cursorAnchor = new GameObject("Lexting" + (right ? "Right" : "Left") + "CursorAnchorNode");
			RegisterAllEventsAndActions();
		}
        #endregion init

        #region event handlers
        private void RegisterAllEventsAndActions()
		{
			this.UpdateAsObservable().Subscribe(_=> {
				// Refresh palm normal value and position every frame (some APIs don't do this for us yet).
				// Also, refresh the values averageHandPos needs so the HandCursor can sync position.
				//#if !UNITY_EDITOR && !UNITY_STANDALONE
				if (!_remoteControlActive && cursorAnchor != null)
				{
#if !UNITY_STANDALONE && UNITY_ANDROID
					if (QuestHandCursorPosition.HasTrackingPoint(right))
						cursorAnchor.transform.position = QuestHandCursorPosition.GetWorldPosition(right);
					else
#endif
					{
					var target = averageHandPos;
					if (target.sqrMagnitude > 1e-6f)
					{
						cursorAnchor.transform.position = Vector3.Lerp(
							cursorAnchor.transform.position,
							target,
							handNoiseDampeningFactor);
					}
					}
				}

				if (_remoteControlActive || Lexting.inputManager.isDesktop)
				{
					transform.position = Vector3.Lerp(transform.position, _newSmoothDestination, 0.01f);
					cursorAnchor.transform.position = transform.position;
				}

#if LEXTING_FOUND_OCULUS_SDK && UNITY_ANDROID
				SyncQuestOvrHandAnchorIfPresent();
#endif
			});

			if (_xrMode == null)
			{
				Debug.LogWarning($"[HandMarker] XRInputModalityManager not found for {(right ? "Right" : "Left")} hand marker.");
				return;
			}

			// this is for the new Unity XR Hands implementation
			this.RegisterUpdate(_xrMode.trackedHandModeStarted, _xrMode.trackedHandModeEnded, () => {
				_update?.Dispose();
				_update = this.UpdateAsObservable().Subscribe(_ => {
					if (_tracker == null)
						_tracker = XrHandVisualizerUtility.FindHandRoot(transform.parent, right);
					if (_tracker == null)
						return;
					transform.position = _tracker.position;
					transform.rotation = _tracker.rotation;
				});
			});
        }
		#endregion event handlers

		#region helper methods
		public void UpdateSmoothDestination(Vector3 newDestination)
		{
			_newSmoothDestination = newDestination;
		}

#if LEXTING_FOUND_OCULUS_SDK && UNITY_ANDROID
		private void SyncQuestOvrHandAnchorIfPresent()
		{
			if (_remoteControlActive || QuestHandBindingState.UnityXRHandsActive)
				return;

			var ovrHand = transform.GetComponentInChildren<OVRHand>();
			if (ovrHand == null || !ovrHand.IsTracked)
				return;

			var skeleton = transform.GetComponentInChildren<OVRSkeleton>();
			if (skeleton == null || !skeleton.IsInitialized || skeleton.Bones == null)
				return;

			var wristIndex = (int)OVRSkeleton.BoneId.Hand_WristRoot;
			if (skeleton.Bones.Count <= wristIndex)
				return;

			var wrist = skeleton.Bones[wristIndex].Transform;
			if (wrist == null)
				return;

			transform.SetPositionAndRotation(wrist.position, wrist.rotation);
		}
#endif
		#endregion helper methods
	}
}
