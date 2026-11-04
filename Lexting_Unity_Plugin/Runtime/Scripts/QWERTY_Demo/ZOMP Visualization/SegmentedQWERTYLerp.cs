using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.QWERTY_Demo
{
	public class SegmentedQWERTYLerp : MonoBehaviour
	{
		#region vars
		#region public vars
		#endregion public vars
		#region private vars
		private QWERTYLerp[] _lerps;
		private QWERTYPresentation _siblingPresentation;
		private bool _cycleFinished;
		#endregion private vars
		#endregion vars

		#region init
		private void Awake()
		{
			this.Chs(ref _lerps);
			this.Ch(ref _siblingPresentation);
			this.Subscribe(_siblingPresentation.ForwardEvent, () => {
				if (_cycleFinished)
					 _siblingPresentation.ParentCallback();
				else {
					BringBothEndsIn();
					_cycleFinished = true;
				}
			});
		}
		#endregion init

		#region helper methods
		private void BringBothEndsIn()
		{
			if (_lerps.Length >= 2)
			{
				_lerps[0].MoveLeftSegment();
				_lerps[1].MoveRightSegment();
			}
			else Debug.LogError("SegmentedQWERTYLerp couldn't find at least one QWERTYLerp componente in its children.");
		}
		#endregion helper methods

		#region Unit Tests
		public void UnitTest_Both()
		{
			BringBothEndsIn();
		}
		#endregion Unit Tests
	}
}
