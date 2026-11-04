using Pixelplacement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SpatialGestureText.TwoZoneCube
{
	public class TwoZoneTriggerScaler : MonoBehaviour
	{
        #region vars
        #region public vars
        public float initScale;
        #endregion public vars
        #region private vars
        private float _bumpScale = 0.8f;
        private Vector3 _bumpScaleCache;
        #endregion private vars
        #endregion vars

        #region init
        private void Awake()
		{
            InitAtScale(initScale);
		}

        public void InitAtScale(float scale)
        {
            transform.localScale = new Vector3(scale, scale, scale);
            _bumpScaleCache = new Vector3(_bumpScale, _bumpScale, _bumpScale);
        }
        #endregion init

        #region loops

        #endregion loops

        #region event handlers

        #endregion event handlers

        #region helper methods


        public void Bump(UnityAction callback)
        {
            Tween.LocalScale(transform, _bumpScaleCache, 0.05f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.LocalScale(transform, Vector3.one, 0.05f, 0, Tween.EaseOut, Tween.LoopType.None, null, () => callback?.I());
            });
        }
        #endregion helper methods

        #region Unit Tests

        #endregion Unit Tests
    }
}
