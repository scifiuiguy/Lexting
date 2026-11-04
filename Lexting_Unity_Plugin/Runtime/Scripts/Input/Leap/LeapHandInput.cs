#if LEXTING_FOUND_LEAP_SDK
using Leap.Unity.Attachments;
using SpatialGestureText.Input.Leap;
using SpatialGestureText.LetterCube;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using SpatialGestureText.TwoZoneCube;
using Leap.Unity;
using SpatialGestureText.Utilities;
using UnityEngine.Events;
using SpatialGestureText.Interfaces;

namespace SpatialGestureText.Input
{
	/// <summary>
	/// The input module used to convert Leap hand motion to input triggers in the Lexting interface
	/// </summary>
	public class LeapHandInput : Singleton<LeapHandInput>
	{
		#region vars
		#region public vars
		[HideInInspector] public SpatialGestureEvent<HandGestureState, HandGestureState> LeftStateChanged = new SpatialGestureEvent<HandGestureState, HandGestureState>();
		[HideInInspector] public SpatialGestureEvent<HandGestureState, HandGestureState> RightStateChanged = new SpatialGestureEvent<HandGestureState, HandGestureState>();
		[HideInInspector] public SpatialGestureEvent<CubeRotationState, CubeRotationState> LeftRotationStateChanged = new SpatialGestureEvent<CubeRotationState, CubeRotationState>();
		[HideInInspector] public SpatialGestureEvent<CubeRotationState, CubeRotationState> RightRotationStateChanged = new SpatialGestureEvent<CubeRotationState, CubeRotationState>();
		
		/// <summary>
		/// An event that triggers when thumb moves beyond a distance threshold away from its palm, where the input specifices which hand
		/// </summary>
		[HideInInspector] public SpatialGestureEvent<bool> ThumbOut = new SpatialGestureEvent<bool>();

		/// <summary>
		/// An even that triggers when thumb and index tip come within a distance threshold while hand is in neutral pose
		/// </summary>
		[HideInInspector] public SpatialGestureEvent<bool> IndexThumbTouch = new SpatialGestureEvent<bool>();

		/// <summary>
		/// An even that triggers when thumb and index tip come within a distance threshold while hand is in neutral pose
		/// </summary>
		[HideInInspector] public SpatialGestureEvent<bool> OppositeMiddleToPalmTouch = new SpatialGestureEvent<bool>();


		/// <summary>
		/// An event that triggers when index finger tip moves beyond a distance threshold away from its base knuckle
		/// while all other fingers are closed. The boolean param specifies which hand began pointing.
		/// </summary>
		[HideInInspector] public SpatialGestureEvent<bool> IndexPointing = new SpatialGestureEvent<bool>();

		[SerializeField] public HandGestureState currentRightHandGestureState, currentLeftHandGestureState;
		[HideInInspector] public CubeRotationState currentRightRotationState, currentLeftRotationState;
		/*[HideInInspector]*/ public bool inputActive;
		#endregion public vars
		#region private vars
		private HandGestureState currentState;
		private HandGestureState _previousRightState;
		private HandGestureState _previousLeftState;
		private CubeRotationState _previousRightRotationState;
		private CubeRotationState _previousLeftRotationState;
		
		/// <summary>
		/// The parent node of the Leap SDK's hierarchy that contains the AttachmentHands class
		/// </summary>
		//private Transform _handsRoot;
		private Transform _rightRoot;
		private Transform _leftRoot;

		private SphereCollider _rightPalmCollider;
		//private SphereCollider 

		//private LeapServiceProvider _leapController;
		#endregion private vars
		#endregion vars

		#region init
		private void Start()
		{
			BindHandNodes(()=>RegisterAllEventsAndActions());
			//this.Ch(ref _leapController);
		}

		private void BindHandNodes(UnityAction callback)
		{
			this.Wait(()=>Lexting.inputManager.Ready(), () => {

				// Input manager initiates this workflow by creating this class, which immediately attempts to assign all hand nodes
				if (Lexting.inputManager.rightHand == null || Lexting.inputManager.leftHand == null)
				{
					Debug.LogError("Lexting Leap Input cannot initialize. " +
						"Hands may not not have been assigned in the LextingInputModuleManager yet.");
					return;
				}
				else {
					Lexting.input.rightHand.wrist = Lexting.inputManager.rightHand;
					Lexting.input.leftHand.wrist = Lexting.inputManager.leftHand;

				}

				foreach (Transform child in transform)
				{
					if (child.name.Contains("Left"))
						_leftRoot = child;
					if (child.name.Contains("Right"))
						_rightRoot = child;
				}

				if (_leftRoot == null || _rightRoot == null)
				{
					Debug.LogError("The Leap AttachmentHands script seems to be missing one or more of its hands. The Lexting" +
						"Input Module Manager couldn't locate the root transform of each hand in the Leap controller.");
					return;
				}

				AssignNode<LextingLeapPalmMarker>(true, ref Lexting.input.rightHand.palm, "Palm", _rightRoot, "right", "palm");
				AssignNode<LextingLeapPalmMarker>(false, ref Lexting.input.leftHand.palm, "Palm", _leftRoot, "left", "palm");
				AssignNode<LextingLeapThumbMarker>(true, ref Lexting.input.rightHand.thumbTip, "ThumbTip", _rightRoot, "right", "thumb tip");
				AssignNode<LextingLeapThumbMarker>(false, ref Lexting.input.leftHand.thumbTip, "ThumbTip", _leftRoot, "left", "thumb tip ");
				AssignNode<LextingLeapFingertipMarker>(true, ref Lexting.input.rightHand.middleTip, "MiddleTip", _rightRoot, "right", "middle fingertip");
				AssignNode<LextingLeapFingertipMarker>(false, ref Lexting.input.leftHand.middleTip, "MiddleTip", _leftRoot, "left", "middle fingertip");
				AssignNode<LextingLeapIndexBaseMarker>(true, ref Lexting.input.rightHand.indexBase, "IndexKnuckle", _rightRoot, "right", "base index finger knuckle");
				AssignNode<LextingLeapIndexBaseMarker>(false, ref Lexting.input.leftHand.indexBase, "IndexKnuckle", _leftRoot, "left", "base index finger knuckle");
				AssignNode<LextingLeapIndexMarker>(true, ref Lexting.input.rightHand.indexTip, "IndexTip", Lexting.input.rightHand.indexBase, "right", "index fingertip");
				AssignNode<LextingLeapIndexMarker>(false, ref Lexting.input.leftHand.indexTip, "IndexTip", Lexting.input.leftHand.indexBase, "left", "index fingertip");
				callback?.I();
			});
		}

		//private Transform FindChildByName(string name, Transform start)
		//{
		//	foreach (Transform child in start)
		//	{
		//		if (child.name.Contains(name))
		//			return child;
		//	}
		//	return null;
		//}

		private void AssignNode<T>(bool isRight, ref Transform node, string hierarchyName, 
			Transform root, string right, string errorDisplayName) where T : IHandPointNode
		{
			//node = FindChildByName(hierarchyName, root);
			node = root.Find(hierarchyName);
			if (node == null)
			{
				Debug.LogError($"The Lexting Input Module Manager could not locate a " +
					$"{(isRight ? "right" : "left")} {errorDisplayName} object in the Leap controller. " +
					$"Double-check that the {errorDisplayName} checkbox is checked on in the AttachmentHands script.");
				return;
			}
			else ((node.gameObject.AddComponent(typeof(T))) as IHandPointNode).right = isRight;
		}
		#endregion init

		#region event handlers
		private void RegisterAllEventsAndActions()
		{
			inputActive = true;
			//this.UpdateAsObservable().Subscribe(_ => {
			//	if (_leapController.CurrentFrame.Hands.Count > 0)
			//		inputActive = true;
			//	else inputActive = false;
			//});

			//for (int i = 0; i < 2; i++)
			//    handBinders[i].ObserveEveryValueChanged(x => x.isTracked).Subscribe(isTracked =>
			//    {
			//        if (isTracked && handBinders[(i == 0) ? 1 : 0].isTracked)
			//            inputActive = true;
			//        else if (!isTracked && !handBinders[(i == 0) ? 1 : 0].isTracked)
			//            inputActive = false;
			//    });

			//this.Subscribe(HandCursor.OnHoverBoundaryUpdate, (right, isInBounds) => {
			//	if (HandCursor.rightInBounds && HandCursor.leftInBounds)
			//		inputActive = true;
			//	else if (!HandCursor.rightInBounds && !HandCursor.leftInBounds)
			//		inputActive = false;
			//});

			Debug.Log("Lexting Leap input binding finished initialization.");
        }

		public void IndexStateChanged(bool closed, bool right)
		{
			if (right) IndexStateChanged((closed) ? HandGestureState.Insert : HandGestureState.Neutral, 
				ref RightStateChanged, ref currentRightHandGestureState, ref _previousRightState);
			else IndexStateChanged((closed) ? HandGestureState.Insert : HandGestureState.Neutral, 
				ref LeftStateChanged, ref currentLeftHandGestureState, ref _previousLeftState);
            //Debug.Log("LeapHandInput " + ((right) ? "Right " : "Left ") + ((closed) ? "activated" : "deactivated"));
        }

		private void IndexStateChanged(
			HandGestureState newState, 
			ref SpatialGestureEvent<HandGestureState,HandGestureState> trigger, 
			ref HandGestureState currentState, 
			ref HandGestureState previousState)
		{
			currentState = newState;
			trigger.I(currentState, previousState);
			previousState = newState;
			Debug.Log($"LeapHandInput just sent a trigger to SpatialGestureInput - newstate: {newState} previous: {previousState}");
		}

		public void PalmRotationStateChanged(bool right, CubeRotationState newRotationState)
		{
			if (right) PalmRotationStateChanged(newRotationState, ref RightRotationStateChanged, ref currentRightRotationState, ref _previousRightRotationState);
			else PalmRotationStateChanged(newRotationState, ref LeftRotationStateChanged, ref currentLeftRotationState, ref _previousLeftRotationState);
            //Debug.Log("LeapHandInput " + ((right) ? "Right " : "Left ") + newRotationState);
        }

		private void PalmRotationStateChanged(CubeRotationState newState, ref SpatialGestureEvent<CubeRotationState, CubeRotationState> trigger, ref CubeRotationState currentState, ref CubeRotationState previousState)
		{
			currentState = newState;
			trigger.I(currentState, previousState);
			previousState = newState;
		}
		#endregion event handlers


	}
}
#endif
