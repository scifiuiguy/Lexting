using SpatialGestureText.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.TwoZoneCube
{
	public static class TwoZoneGrabState
	{
		/// <summary>
		/// The structure instance that houses all current data relevant to user's 2-handed grab/drag action
		/// </summary>
		public static GrabData grabData;

		public static SpatialGestureEvent OnGrabStateChanged = new SpatialGestureEvent();



		/// <summary>
		/// Set to true during assembly if the hand was loaded and just had its 2nd letter replaced
		/// </summary>
		public static bool currentReloaded;

		/// <summary>
		/// A composite of all grabbed letters from both hands in the user's chosen order, up to four letters total.
		/// Order is chosen w/ the following bias:
		/// 1st comp letter: First letter grabbed by the first hand to grab.
		/// 2nd comp letter: First letter grabbed by the Second hand to grab.
		/// 3rd comp letter: The only letter highlighted by the first hand, other than its start (empty if hand has returned to start).
		/// 4th comp letter: The only letter highlighted by the second hand, other than its start (empty if hand has returned to start).
		/// User can easily flip 3rd and 4th comp letters by mistake at fast speeds. The mistake remains here in the comp
		/// to represent user's undoctored input, but the assembler will correct the output in real time as long as the mistake yields 
		/// no dictionary result. In cases where inverting 3rd & 4th creates another viable word,
		/// user self-corrects by practicing both variations.
		/// </summary>
		public static string comp =>
			(string.IsNullOrEmpty(first(false)) || string.IsNullOrEmpty(first(true))) ? "" :
				((grabData.firstPair == GrabbedFirst.right) ? first(true) + first(false) : first(false) + first(true))
				+ (grabData.secondPair == GrabbedFirst.right
						? (!returned(true) ? second(true) : "") + (!returned(false) ? second(false) : "")
						: (!returned(false) ? second(false) : "") + (!returned(true) ? second(true) : ""));

		/// <summary>
		/// A composite of up to two letters grabbed by one dragging hand
		/// </summary>
		/// <param name="right">if true, right hand, false otherwise</param>
		public static string compSingle(bool right) => first(right) + second(right);

		/// <summary>
		/// if true, the right or left hand has chosen two characters
		/// </summary>
		/// <returns></returns>
		public static bool compSingleReady(bool right) => compSingle(right).Length == 2;

		/// <summary>
		/// if true, the right or left hand has chosen two characters and the other has not
		/// </summary>
		/// <param name="right">if true, the right chose 2 and left didn't, otherwise vice versa</param>
		/// <returns></returns>
		public static bool compSingleReadySolo(bool right) => compSingleReady(right) && !compSingleReady(!right);
		public static bool compReady => !string.IsNullOrEmpty(comp);

		public static bool returned(bool right) => second(right).Equals(first(right));

		public static bool returned(bool right, string newSecond) => newSecond.Equals(first(right));

		/// <summary>
		/// if true, the assembler just called the ReturnSecond method earlier this frame
		/// </summary>
		public static bool returnedThisFrame;

		public static string first(bool right) => right ? grabData.firstRight : grabData.firstLeft;
		public static string second(bool right) => right ? grabData.secondRight : grabData.secondLeft;
		public static bool firstReady(bool right) => !string.IsNullOrEmpty(first(right));
		public static bool secondReady(bool right) => !string.IsNullOrEmpty(second(right));


		public static void SetCurrent(bool right, bool first, string letter)
		{
			if (right)
			{
				if (first) UpdateHandData(right, letter, ref grabData.firstRight, ref grabData.firstPair);
				else UpdateHandData(right, letter, ref grabData.secondRight, ref grabData.secondPair);
			}
			else {
				if (first) UpdateHandData(right, letter, ref grabData.firstLeft, ref grabData.firstPair);
				else UpdateHandData(right, letter, ref grabData.secondLeft, ref grabData.secondPair);
			}
			OnGrabStateChanged?.I();
		}

		public static void ReturnSecond(bool right)
		{
			if (right)
				grabData.secondRight = "";
			else grabData.secondLeft = "";
			grabData.secondPair = GrabbedFirst.none;
			returnedThisFrame = true;
		}

		private static void UpdateHandData(bool right, string letter, ref string target, ref GrabbedFirst pair)
		{
			target = letter;
			if (pair == GrabbedFirst.none)
				pair = right ? GrabbedFirst.right : GrabbedFirst.left;
			else if (letter.Equals(""))
				pair = GrabbedFirst.none;
		}

		public static void Clear()
		{
			grabData.firstRight = "";
			grabData.secondRight = "";
			grabData.firstLeft = "";
			grabData.secondLeft = "";
			grabData.firstPair = GrabbedFirst.none;
			grabData.secondPair = GrabbedFirst.none;
			returnedThisFrame = false;
		}


		/// <summary>
		/// This method swaps the second character of each hand since it is easy
		/// for the user to jumble them by mistake. This is only called if no dictionary results
		/// are found on the as-is first search of the user's original comp.
		/// </summary>
		private static string temp;
		public static void FlipSeconds()
		{
			if (grabData.firstPair == GrabbedFirst.none || grabData.secondPair == GrabbedFirst.none) return;
			
			temp = grabData.secondLeft;
			grabData.secondLeft = grabData.secondRight;
			grabData.secondRight = temp;			
		}
	}
}
