using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SpatialGestureText.TwoZoneCube;
using UniRx;
using UniRx.Triggers;
using SpatialGestureText.Utilities;

namespace SpatialGestureText.TwoZoneCube
{
	public class CurrentLetterDisplayHandler : MonoBehaviour
	{
		#region vars
		#region public vars

		#endregion public vars
		#region private vars
		/// <summary>
		/// if set to true in inpsector, this component represents the right hand
		/// </summary>
		[SerializeField] private bool _right;

		private Text _text;
		private CanvasGroup _group;

		private string _displayLetter {
			get {
				if (_right)
				{
					if (TwoZoneTrigger.currentRightHighlightedLetter == " ")
						return "[_]";
					else return TwoZoneTrigger.currentRightHighlightedLetter;
				}
				else if (TwoZoneTrigger.currentLeftHighlightedLetter == " ")
					return "[_]";
				else return TwoZoneTrigger.currentLeftHighlightedLetter;

			}
		}
		#endregion private vars
		#endregion vars

		#region init
		private void Awake()
		{
			this.Ch(ref _text, ref _group);
			
			Lexting.finalizer.ObserveEveryValueChanged(x => x.isSpatialListActive)
				.Subscribe(active => _group.alpha = active ? 0 : 1);

			// Trigger ref updates before deferred TwoZoneTrigger.HandleTriggerEnter sets current*HighlightedLetter.
			Lexting.controllers
				.ObserveEveryValueChanged(x => _right ? x.currentRightTrigger : x.currentLeftTrigger)
				.Subscribe(_ => this.WaitF(1, RefreshDisplayedLetter));

            //Lexting.controllers.lastCurrentTrigger(_right)
            //	.ObserveEveryValueChanged(x => x.matrixDisplayState).Subscribe(state=>_text.text=state);

            this.Subscribe(Lexting.hotkeys.OnMatrixDisplayStateChanged, (state) =>
            {
				this.WaitF(1, () => {
					if (Lexting.controllers.lastCurrentTrigger(_right).IsMatrixState)
						_text.text = Lexting.controllers.lastCurrentTrigger(_right).matrixDisplayState;
				});
            });

            this.GetGestures((right, current, previous) => { }, (right, current, previous) => {
				if (right != _right) return;
				this.WaitF(1, RefreshDisplayedLetter);
			});
		}
		#endregion init

		private void RefreshDisplayedLetter()
		{
			if (_text == null)
				return;
			_text.text = _displayLetter;
		}
	}
}
