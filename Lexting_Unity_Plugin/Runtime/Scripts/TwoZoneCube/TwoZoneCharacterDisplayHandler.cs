using Pixelplacement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.TwoZoneCube
{
	public class TwoZoneCharacterDisplayHandler : MonoBehaviour
	{
        #region vars
        #region public vars
        [HideInInspector] public TwoZoneCubeController controller;
        #endregion public vars
        #region private vars
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private LetterSwitcher[] _letterSwitchers;
		#endregion private vars
		#endregion vars
		
		#region init
		private void Awake()
		{
            _canvas = GetComponent<Canvas>();
            _canvasGroup = GetComponent<CanvasGroup>();
            this.Wait(() => controller.Ready(), () => {
                this.ChsFE(ref _letterSwitchers, (switcher) => switcher.controller = controller);
            });
		}
        #endregion init

        #region loops

        #endregion loops

        #region event handlers

        #endregion event handlers

        #region helper methods
        public void ToggleVisibility(bool on)
        {
            Tween.CanvasGroupAlpha(_canvasGroup, (on) ? 1 : 0, 0.5f, 0);
        }
		#endregion helper methods
		
		#region Unit Tests
		
		#endregion Unit Tests
	}
}
