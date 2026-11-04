using Pixelplacement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpatialGestureText.Tutorial
{
	public class BullseyeHandler : MonoBehaviour
	{
        #region vars
        #region public vars

        #endregion public vars
        #region private vars
        private Image bullseyeImage;
        private Vector3 startingScale;
        private bool _isMoving;
        [SerializeField] private AnimationCurve backEndBumpBounce;
		#endregion private vars
		#endregion vars
		
		#region init
		private void Awake()
		{
            bullseyeImage = GetComponent<Image>();
            startingScale = transform.localScale;
        }
        #endregion init

        #region loops

        #endregion loops

        #region event handlers

        #endregion event handlers

        #region helper methods
        public void Bump()
        {
            if (_isMoving) return;
            else _isMoving = true;

            Tween.LocalScale(transform, startingScale * 0.7f, 0.1f, 0, Tween.EaseOut, Tween.LoopType.None, null, () => {
                Tween.LocalScale(transform, startingScale, 0.1f, 0, backEndBumpBounce, Tween.LoopType.None, null, () => {
                    _isMoving = false;
                });
            });
        }
        #endregion helper methods

        #region Unit Tests
        public void UnitTest_Bump()
        {
            Bump();
        }
		#endregion Unit Tests
	}
}
