using SpatialGestureText.LetterCube;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Pixelplacement;
using SpatialGestureText.Utilities;

namespace SpatialGestureText.TwoZoneCube
{
	public class TwoZoneCubeWordDisplayHandler : MonoBehaviour
	{
		#region vars
		#region public vars
		private Text currentWordDisplay;
		#endregion public vars
		#region private vars
		Vector3 startingPosition;
		Vector3 startingOffset;
		#endregion private vars
		#endregion vars
		
		#region init
		private void Awake()
		{
			startingPosition = transform.position;
			this.Ch(ref currentWordDisplay);

			this.Subscribe(TwoZoneGrabState.OnGrabStateChanged, () => {
				if (!Lexting.finalizer.isSpatialListActive)
					currentWordDisplay.text = TwoZoneGrabState.comp;
			});
			this.Subscribe(Lexting.printer.OnBackWord, (numberOfCharsRemoved) =>currentWordDisplay.text = "");

			//CubeWordAssemblyController.Instance.ObserveEveryValueChanged(x => x.currentWord)
			//	.Where(currentWord=>!TwoZoneWordListController.Instance.isSpatialListActive)
			//	.Subscribe(currentWord => currentWordDisplay.text = currentWord);
			Lexting.assembler.ObserveEveryValueChanged(x => x.isWordAssemblyActive)
				.Where(wordAssemblyActive=>!wordAssemblyActive)
				.Subscribe(wordAssemblyActive => AnimateDisplayOnWordSubmission());
			
			this.Subscribe(Lexting.finalizer.OnRefreshWord, word=>currentWordDisplay.text=word);

			Lexting.finalizer.transform.ObserveEveryValueChanged(x => x.position).Subscribe(newPos=>startingPosition = newPos + 0.1f*Vector3.up);
		}
		#endregion init

		private void AnimateDisplayOnWordSubmission()
        {
			this.LerpToPos(startingPosition + new Vector3(0, 0.1f, 0), ()=> {
				currentWordDisplay.text = "";
				transform.position = startingPosition;
			});
        }
	}
}
