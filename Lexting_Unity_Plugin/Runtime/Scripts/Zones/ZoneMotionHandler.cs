using Pixelplacement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SpatialGestureText.Zones
{
	public class ZoneMotionHandler : MonoBehaviour
	{
        #region vars
        public bool right;
        [HideInInspector] public float zoneWidthApart;
        [HideInInspector] public AnimationCurve zoneEaseBounce;
        #endregion vars

        #region helper methods
        public void Initialize()
        {
            transform.localPosition = Vector3.zero;
        }

        public void Introduce(UnityAction callback)
        {
            Tween.LocalPosition(transform, new Vector3(((right)?-1:1) * zoneWidthApart, 0, 0), 0.2f, 0.2f, zoneEaseBounce, Tween.LoopType.None, null, ()=>callback.I());
        }

        public void Dismiss(UnityAction callback)
        {
            Tween.LocalPosition(transform, Vector3.zero, 0.1f, 0, Tween.EaseOut, Tween.LoopType.None, null, ()=>callback.I());
        }
		#endregion helper methods
	}
}
