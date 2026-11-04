using SpatialGestureText.Hotkeys;
using SpatialGestureText.Input;
using SpatialGestureText.Interfaces;
using SpatialGestureText.LetterCube;
using SpatialGestureText.Streaming;
using SpatialGestureText.TwoZoneCube;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.Utilities
{
	/// <summary>
	/// A class containing static shortcuts to common singleton and pingleton refs
	/// </summary>
	public static class Lexting
	{
		public static LextingInputModuleManager inputManager => LextingInputModuleManager.Instance;
		public static SpatialGestureInput input						=> SpatialGestureInput.Instance;
		public static HandGestureState handState(bool right)		=> input.currentState(right);
		public static HandGestureState rhState						=> input.currentRightHandGestureState;
		public static HandGestureState lhState						=> input.currentLeftHandGestureState;
		public static CubeRotationState rotState(bool right)		=> input.currentRotationState(right);
		public static CubeRotationState rhRotState					=> input.currentRightRotationState;
		public static CubeRotationState lhRotState					=> input.currentLeftRotationState;
		public static TwoZoneCubeControllers controllers			=> TwoZoneCubeControllers.Instance;
		public static TwoZoneCubeController controller(bool right)	=> right ? TwoZoneCubeControllers.Instance.right : TwoZoneCubeControllers.Instance.left;
		public static CubeWordAssemblyController assembler			=> CubeWordAssemblyController.Instance;
		public static TwoZoneWordListController finalizer			=> TwoZoneWordListController.Instance;
		public static LextingConjugationState conjugator			=> LextingConjugationState.Instance;
		public static CubeDragLineVisualizer rhLine					=> controllers.rightDragLineHandler;
		public static CubeDragLineVisualizer lhLine					=> controllers.leftDragLineHandler;
		public static CubeDragLineVisualizer line(bool right)		=> right ? rhLine : lhLine;
		public static CubeAudioManager audio						=> CubeAudioManager.Instance;
		public static LextingOutputManager printer					=> LextingOutputManager.Instance;
		public static LextingHotkeyManager hotkeys					=> LextingHotkeyManager.Instance;
		public static HandMarker rHandMarker						=> inputManager.rightMarker;
		public static HandMarker lHandMarker						=> inputManager.leftMarker;
		public static HandMarker hMarker(bool right)				=> right ? rHandMarker : lHandMarker;
		public static HeadMarker head								=> HeadMarker.Instance;
		public static StreamReceiverClient streamReceiver			=> StreamReceiverClient._.Instance;


		//public static MouseController mouse => MouseController._.Instance;
	}
}
