using Pixelplacement;
using Pixelplacement.TweenSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SpatialGestureText.LetterCube
{
    public class LetterScaleHandler : MonoBehaviour
    {
        private float defaultScale = 1;
        private float hoverScale = 1.5f;
        private float triggerScale = 2f;
        private float originalScale;
        private float currentScale;

        /// <summary>
        /// A value from 0 to 1 which represents the percentage change 
        /// the object will scale toward its target scale
        /// </summary>
        [Range(0,1)]
        private float dampener;

        bool test;
        private ScaleState currentScaleState;

        private IEnumerator lerpCoroutine;

        private TweenBase tween;

        #region init
        private void Awake()
        {
            originalScale = transform.localScale.x;
        }
        #endregion init
        
        #region loops
        
        #endregion loops

        #region helper methods
        private void LerpToScale(float scale, UnityAction callback)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.LocalScale(transform, new Vector3(scale, scale, scale), 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, ()=>callback.I());
            tween.Start();
        }

        public void Hover(bool on, UnityAction callback)
        {
            float startScale = (on) ? originalScale : hoverScale;
            float finishScale = (on) ? hoverScale : originalScale;
            if (tween != null && tween.Status == Tween.TweenStatus.Running || transform.localScale.x != finishScale)
            {
                LerpToScale(startScale, () =>
                {
                    LerpToScale(finishScale, callback);
                });
            }
            else LerpToScale(finishScale, callback);
        }

        public void Bump(UnityAction callback)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running || transform.localScale.x != hoverScale)
            {
                LerpToScale(hoverScale, () =>
                {
                    LerpToScale(triggerScale, () => {
                        LerpToScale(hoverScale, callback);
                    });
                });
            }
            else LerpToScale(triggerScale, () => {
                LerpToScale(hoverScale, callback);
            });
        }
        #endregion helper methods

        #region Unit Tests
        public void UnitTest_Hover_On()
        {
            Hover(true, null);
        }

        public void UnitTest_Hover_Off()
        {
            Hover(false, null);
        }

        public void UnitTest_Bump()
        {
            Bump(null);
        }
        #endregion Unit Tests
    }

    public enum ScaleState
    {
        Default,
        Hover,
        Trigger
    }
}

