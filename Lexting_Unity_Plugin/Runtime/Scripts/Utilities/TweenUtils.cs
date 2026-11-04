using Pixelplacement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Pixelplacement.TweenSystem;
using UnityEngine.UI;

namespace SpatialGestureText.Utilities
{
	public static class TweenUtils
	{
        /// <summary>
        /// In most cases, the Surge plugin asks for more required parameters than we need bc even the ones that are optional
        /// become required just bc they come before the 2nd-to-last option, which we need often: the complete callback.
        /// Also, extending the object to be lerped is more legible, so extension-izing the tween methods is handy.
        /// These extension wrappers eliminate the need for most unneeded params in the most common interpolation use cases.
        /// 
        /// Each type & category follow this overload parameter template:
        /// to
        /// to, duration
        /// to, callback
        /// to, tween (the tween param is of type TweenBase... it's a cancellation ref)
        /// to, duration, callback
        /// to, duration, tween
        /// to, callback, tween
        /// to, duration, callback, tween
        /// 
        /// Each overload comes in two types: LerpTo... and Bump... 
        /// 
        /// Available categories: position, scale, color (text/material/shader), alpha (text/material/shader), 
        /// TBD: float (mono/shader), vector2 (mono/shader), vector3 (mono/shader)
        /// 
        /// 8 param templates x 2 types x 14 categories = 224 overloads
        /// </summary>
        #region position interpolation
        public static void LerpToPos(this MonoBehaviour behavior, Vector3 to)
        {
            Tween.Position(behavior.transform, to, 0.1f, 0);
        }

        public static void BumpPos(this MonoBehaviour behavior, Vector3 to)
        {
            Vector3 originalPos = behavior.transform.position;
            Tween.Position(behavior.transform, to, 0.1f, 0, Tween.EaseIn,Tween.LoopType.None,null,()=> {
                Tween.Position(behavior.transform, originalPos, 0.1f, 0, Tween.EaseOut);
            });
        }

        public static void LerpToPos(this MonoBehaviour behavior, Vector3 to, float duration)
        {
            Tween.Position(behavior.transform, to, duration, 0, Tween.EaseInOut);
        }

        public static void BumpPos(this MonoBehaviour behavior, Vector3 to, float duration)
        {
            Vector3 originalPos = behavior.transform.position;
            Tween.Position(behavior.transform, to, duration / 2, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.Position(behavior.transform, originalPos, duration / 2, 0, Tween.EaseOut);
            });
        }

        public static void LerpToPos(this MonoBehaviour behavior, Vector3 to, UnityAction callback)
        {
            Tween.Position(behavior.transform, to, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
        }

        public static void BumpPos(this MonoBehaviour behavior, Vector3 to, UnityAction callback)
        {
            Vector3 originalPos = behavior.transform.position;
            Tween.Position(behavior.transform, to, 0.1f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.Position(behavior.transform, originalPos, 0.1f, 0, Tween.EaseOut, Tween.LoopType.None,null, ()=>callback?.I());
            });
        }

        public static void LerpToPos(this MonoBehaviour behavior, Vector3 to, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Position(behavior.transform, to, 0.1f, 0, Tween.EaseInOut);
            tween.Start();
        }

        public static void BumpPos(this MonoBehaviour behavior, Vector3 to, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            Vector3 originalPos = behavior.transform.position;
            tween = Tween.Position(behavior.transform, to, 0.1f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                tween = Tween.Position(behavior.transform, originalPos, 0.1f, 0, Tween.EaseOut);
                tween.Start();
            });
            tween.Start();
        }

        public static void LerpToPos(this MonoBehaviour behavior, Vector3 to, float duration, UnityAction callback)
        {
            Tween.Position(behavior.transform, to, duration, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
        }

        public static void BumpPos(this MonoBehaviour behavior, Vector3 to, float duration, UnityAction callback)
        {
            Vector3 originalPos = behavior.transform.position;
            Tween.Position(behavior.transform, to, duration/2, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.Position(behavior.transform, originalPos, duration/2, 0, Tween.EaseOut, Tween.LoopType.None, null, () => callback?.I());
            });
        }

        public static void LerpToPos(this MonoBehaviour behavior, Vector3 to, float duration, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Position(behavior.transform, to, duration, 0, Tween.EaseInOut);
            tween.Start();
        }

        public static void BumpPos(this MonoBehaviour behavior, Vector3 to, float duration, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            Vector3 originalPos = behavior.transform.position;
            tween = Tween.Position(behavior.transform, to, duration / 2, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                tween = Tween.Position(behavior.transform, originalPos, duration / 2, 0, Tween.EaseOut);
                tween.Start();
            });
            tween.Start();
        }

        public static void LerpToPos(this MonoBehaviour behavior, Vector3 to, UnityAction callback, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Position(behavior.transform, to, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            tween.Start();
        }

        public static void BumpPos(this MonoBehaviour behavior, Vector3 to, UnityAction callback, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            Vector3 originalPos = behavior.transform.position;
            tween = Tween.Position(behavior.transform, to, 0.1f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                tween = Tween.Position(behavior.transform, originalPos, 0.1f, 0, Tween.EaseOut, Tween.LoopType.None, null, () => callback?.I());
                tween.Start();
            });
            tween.Start();
        }

        public static void LerpToPos(this MonoBehaviour behavior, Vector3 to, float duration, UnityAction callback, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Position(behavior.transform, to, duration, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            tween.Start();
        }

        public static void BumpPos(this MonoBehaviour behavior, Vector3 to, float duration, UnityAction callback, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            Vector3 originalPos = behavior.transform.position;
            tween = Tween.Position(behavior.transform, to, duration / 2, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                tween = Tween.Position(behavior.transform, originalPos, duration / 2, 0, Tween.EaseOut, Tween.LoopType.None, null, () => callback?.I());
                tween.Start();
            });
            tween.Start();
        }

        #region forward interpolation
        public static void LerpForward(this MonoBehaviour behavior, float distance)
        {
            Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.forward*distance, 0.1f, 0);
        }

        public static void BumpForward(this MonoBehaviour behavior, float distance)
        {
            Vector3 originalPos = behavior.transform.position;
            Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.forward*distance, 0.1f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.Position(behavior.transform, originalPos, 0.1f, 0, Tween.EaseOut);
            });
        }

        public static void LerpForward(this MonoBehaviour behavior, float distance, float duration)
        {
            Tween.Position(behavior.transform, behavior.transform.forward*distance, duration, 0, Tween.EaseInOut);
        }

        public static void BumpForward(this MonoBehaviour behavior, float distance, float duration)
        {
            Vector3 originalPos = behavior.transform.position;
            Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.forward*distance, duration / 2, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.Position(behavior.transform, originalPos, duration / 2, 0, Tween.EaseOut);
            });
        }

        public static void LerpForward(this MonoBehaviour behavior, float distance, UnityAction callback)
        {
            Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.forward*distance, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
        }

        public static void BumpForward(this MonoBehaviour behavior, float distance, UnityAction callback)
        {
            Vector3 originalPos = behavior.transform.position;
            Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.forward*distance, 0.1f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.Position(behavior.transform, originalPos, 0.1f, 0, Tween.EaseOut, Tween.LoopType.None, null, () => callback?.I());
            });
        }

        public static void LerpForward(this MonoBehaviour behavior, float distance, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.forward*distance, 0.1f, 0, Tween.EaseInOut);
            tween.Start();
        }

        public static void BumpForward(this MonoBehaviour behavior, float distance, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            Vector3 originalPos = behavior.transform.position;
            tween = Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.forward*distance, 0.1f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                tween = Tween.Position(behavior.transform, originalPos, 0.1f, 0, Tween.EaseOut);
                tween.Start();
            });
            tween.Start();
        }

        public static void LerpForward(this MonoBehaviour behavior, float distance, float duration, UnityAction callback)
        {
            Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.forward*distance, duration, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
        }

        public static void BumpForward(this MonoBehaviour behavior, float distance, float duration, UnityAction callback)
        {
            Vector3 originalPos = behavior.transform.position;
            Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.forward*distance, duration / 2, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.Position(behavior.transform, originalPos, duration / 2, 0, Tween.EaseOut, Tween.LoopType.None, null, () => callback?.I());
            });
        }

        public static void LerpForward(this MonoBehaviour behavior, float distance, float duration, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.forward*distance, duration, 0, Tween.EaseInOut);
            tween.Start();
        }

        public static void BumpForward(this MonoBehaviour behavior, float distance, float duration, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            Vector3 originalPos = behavior.transform.position;
            tween = Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.forward*distance, duration / 2, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                tween = Tween.Position(behavior.transform, originalPos, duration / 2, 0, Tween.EaseOut);
                tween.Start();
            });
            tween.Start();
        }

        public static void LerpForward(this MonoBehaviour behavior, float distance, UnityAction callback, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.forward*distance, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            tween.Start();
        }

        public static void BumpForward(this MonoBehaviour behavior, float distance, UnityAction callback, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            Vector3 originalPos = behavior.transform.position;
            tween = Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.forward*distance, 0.1f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                tween = Tween.Position(behavior.transform, originalPos, 0.1f, 0, Tween.EaseOut, Tween.LoopType.None, null, () => callback?.I());
                tween.Start();
            });
            tween.Start();
        }

        public static void LerpForward(this MonoBehaviour behavior, float distance, float duration, UnityAction callback, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.forward*distance, duration, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            tween.Start();
        }

        public static void BumpForward(this MonoBehaviour behavior, float distance, float duration, UnityAction callback, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            Vector3 originalPos = behavior.transform.position;
            tween = Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.forward*distance, duration / 2, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                tween = Tween.Position(behavior.transform, originalPos, duration / 2, 0, Tween.EaseOut, Tween.LoopType.None, null, () => callback?.I());
                tween.Start();
            });
            tween.Start();
        }
        #endregion forward interpolation

        #region down interpolation
        public static void LerpDown(this MonoBehaviour behavior, float distance)
        {
            Tween.Position(behavior.transform, behavior.transform.position - 1 * behavior.transform.up * distance, 0.1f, 0);
        }

        public static void BumpDown(this MonoBehaviour behavior, float distance)
        {
            Vector3 originalPos = behavior.transform.position;
            Tween.Position(behavior.transform, behavior.transform.position - 1 * behavior.transform.up * distance, 0.1f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.Position(behavior.transform, originalPos, 0.1f, 0, Tween.EaseOut);
            });
        }

        public static void LerpDown(this MonoBehaviour behavior, float distance, float duration)
        {
            Tween.Position(behavior.transform, behavior.transform.forward * distance, duration, 0, Tween.EaseInOut);
        }

        public static void BumpDown(this MonoBehaviour behavior, float distance, float duration)
        {
            Vector3 originalPos = behavior.transform.position;
            Tween.Position(behavior.transform, behavior.transform.position - 1 * behavior.transform.up * distance, duration / 2, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.Position(behavior.transform, originalPos, duration / 2, 0, Tween.EaseOut);
            });
        }

        public static void LerpDown(this MonoBehaviour behavior, float distance, UnityAction callback)
        {
            Tween.Position(behavior.transform, behavior.transform.position - 1 * behavior.transform.up * distance, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
        }

        public static void BumpDown(this MonoBehaviour behavior, float distance, UnityAction callback)
        {
            Vector3 originalPos = behavior.transform.position;
            Tween.Position(behavior.transform, behavior.transform.position - 1 * behavior.transform.up * distance, 0.1f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.Position(behavior.transform, originalPos, 0.1f, 0, Tween.EaseOut, Tween.LoopType.None, null, () => callback?.I());
            });
        }

        public static void LerpDown(this MonoBehaviour behavior, float distance, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Position(behavior.transform, behavior.transform.position - 1 * behavior.transform.up * distance, 0.1f, 0, Tween.EaseInOut);
            tween.Start();
        }

        public static void BumpDown(this MonoBehaviour behavior, float distance, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            Vector3 originalPos = behavior.transform.position;
            tween = Tween.Position(behavior.transform, behavior.transform.position - 1 * behavior.transform.up * distance, 0.1f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                tween = Tween.Position(behavior.transform, originalPos, 0.1f, 0, Tween.EaseOut);
                tween.Start();
            });
            tween.Start();
        }

        public static void LerpDown(this MonoBehaviour behavior, float distance, float duration, UnityAction callback)
        {
            Tween.Position(behavior.transform, behavior.transform.position - 1 * behavior.transform.up * distance, duration, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
        }

        public static void BumpDown(this MonoBehaviour behavior, float distance, float duration, UnityAction callback)
        {
            Vector3 originalPos = behavior.transform.position;
            Tween.Position(behavior.transform, behavior.transform.position - 1 * behavior.transform.up * distance, duration / 2, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.Position(behavior.transform, originalPos, duration / 2, 0, Tween.EaseOut, Tween.LoopType.None, null, () => callback?.I());
            });
        }

        public static void LerpDown(this MonoBehaviour behavior, float distance, float duration, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Position(behavior.transform, behavior.transform.position - 1 * behavior.transform.up * distance, duration, 0, Tween.EaseInOut);
            tween.Start();
        }

        public static void BumpDown(this MonoBehaviour behavior, float distance, float duration, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            Vector3 originalPos = behavior.transform.position;
            tween = Tween.Position(behavior.transform, behavior.transform.position - 1 * behavior.transform.up * distance, duration / 2, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                tween = Tween.Position(behavior.transform, originalPos, duration / 2, 0, Tween.EaseOut);
                tween.Start();
            });
            tween.Start();
        }

        public static void LerpDown(this MonoBehaviour behavior, float distance, UnityAction callback, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Position(behavior.transform, behavior.transform.position - 1 * behavior.transform.up * distance, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            tween.Start();
        }

        public static void BumpDown(this MonoBehaviour behavior, float distance, UnityAction callback, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            Vector3 originalPos = behavior.transform.position;
            tween = Tween.Position(behavior.transform, behavior.transform.position - 1 * behavior.transform.up * distance, 0.1f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                tween = Tween.Position(behavior.transform, originalPos, 0.1f, 0, Tween.EaseOut, Tween.LoopType.None, null, () => callback?.I());
                tween.Start();
            });
            tween.Start();
        }

        public static void LerpDown(this MonoBehaviour behavior, float distance, float duration, UnityAction callback, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Position(behavior.transform, behavior.transform.position - 1 * behavior.transform.up * distance, duration, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            tween.Start();
        }

        public static void BumpDown(this MonoBehaviour behavior, float distance, float duration, UnityAction callback, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            Vector3 originalPos = behavior.transform.position;
            tween = Tween.Position(behavior.transform, behavior.transform.position - 1 * behavior.transform.up * distance, duration / 2, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                tween = Tween.Position(behavior.transform, originalPos, duration / 2, 0, Tween.EaseOut, Tween.LoopType.None, null, () => callback?.I());
                tween.Start();
            });
            tween.Start();
        }
        #endregion downinterpolation

        #region right interpolation
        public static void LerpRight(this MonoBehaviour behavior, float distance)
        {
            Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.right * distance, 0.1f, 0);
        }

        public static void BumpRight(this MonoBehaviour behavior, float distance)
        {
            Vector3 originalPos = behavior.transform.position;
            Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.right * distance, 0.1f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.Position(behavior.transform, originalPos, 0.1f, 0, Tween.EaseOut);
            });
        }

        public static void LerpRight(this MonoBehaviour behavior, float distance, float duration)
        {
            Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.right * distance, duration, 0, Tween.EaseInOut);
        }

        public static void BumpRight(this MonoBehaviour behavior, float distance, float duration)
        {
            Vector3 originalPos = behavior.transform.position;
            Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.right * distance, duration / 2, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.Position(behavior.transform, originalPos, duration / 2, 0, Tween.EaseOut);
            });
        }

        public static void LerpRight(this MonoBehaviour behavior, float distance, UnityAction callback)
        {
            Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.right * distance, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
        }

        public static void BumpRight(this MonoBehaviour behavior, float distance, UnityAction callback)
        {
            Vector3 originalPos = behavior.transform.position;
            Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.right * distance, 0.1f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.Position(behavior.transform, originalPos, 0.1f, 0, Tween.EaseOut, Tween.LoopType.None, null, () => callback?.I());
            });
        }

        public static void LerpRight(this MonoBehaviour behavior, float distance, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.right * distance, 0.1f, 0, Tween.EaseInOut);
            tween.Start();
        }

        public static void BumpRight(this MonoBehaviour behavior, float distance, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            Vector3 originalPos = behavior.transform.position;
            tween = Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.right * distance, 0.1f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                tween = Tween.Position(behavior.transform, originalPos, 0.1f, 0, Tween.EaseOut);
                tween.Start();
            });
            tween.Start();
        }

        public static void LerpRight(this MonoBehaviour behavior, float distance, float duration, UnityAction callback)
        {
            Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.right * distance, duration, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
        }

        public static void BumpRight(this MonoBehaviour behavior, float distance, float duration, UnityAction callback)
        {
            Vector3 originalPos = behavior.transform.position;
            Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.right * distance, duration / 2, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.Position(behavior.transform, originalPos, duration / 2, 0, Tween.EaseOut, Tween.LoopType.None, null, () => callback?.I());
            });
        }

        public static void LerpRight(this MonoBehaviour behavior, float distance, float duration, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.right * distance, duration, 0, Tween.EaseInOut);
            tween.Start();
        }

        public static void BumpRight(this MonoBehaviour behavior, float distance, float duration, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            Vector3 originalPos = behavior.transform.position;
            tween = Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.right * distance, duration / 2, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                tween = Tween.Position(behavior.transform, originalPos, duration / 2, 0, Tween.EaseOut);
                tween.Start();
            });
            tween.Start();
        }

        public static void LerpRight(this MonoBehaviour behavior, float distance, UnityAction callback, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.right * distance, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            tween.Start();
        }

        public static void BumpRight(this MonoBehaviour behavior, float distance, UnityAction callback, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            Vector3 originalPos = behavior.transform.position;
            tween = Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.right * distance, 0.1f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                tween = Tween.Position(behavior.transform, originalPos, 0.1f, 0, Tween.EaseOut, Tween.LoopType.None, null, () => callback?.I());
                tween.Start();
            });
            tween.Start();
        }

        public static void LerpRight(this MonoBehaviour behavior, float distance, float duration, UnityAction callback, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.right * distance, duration, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            tween.Start();
        }

        public static void BumpRight(this MonoBehaviour behavior, float distance, float duration, UnityAction callback, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            Vector3 originalPos = behavior.transform.position;
            tween = Tween.Position(behavior.transform, behavior.transform.position + behavior.transform.right * distance, duration / 2, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                tween = Tween.Position(behavior.transform, originalPos, duration / 2, 0, Tween.EaseOut, Tween.LoopType.None, null, () => callback?.I());
                tween.Start();
            });
            tween.Start();
        }
        #endregion right interpolation

        public static Vector3 CurrentPlus(this MonoBehaviour behavior, float amount, Vector3 direction)
        { 
            return behavior.transform.position + amount * direction;
        }
        #endregion position interpolation

        #region scale interpolation
        public static void LerpToScale(this MonoBehaviour behavior, float to)
        {
            Tween.LocalScale(behavior.transform, behavior.transform.localScale*to, 0.1f,0);
        }

        public static void BumpScale(this MonoBehaviour behavior, float to)
        {
            float originalScale = behavior.transform.localScale.x;
            Tween.LocalScale(behavior.transform, behavior.transform.localScale*to, 0.1f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.LocalScale(behavior.transform, new Vector3(originalScale, originalScale, originalScale), 0.1f, 0, Tween.EaseOut);
            });
        }

        public static void LerpToScale(this MonoBehaviour behavior, float to, float duration)
        {
            Tween.LocalScale(behavior.transform, behavior.transform.localScale*to, duration, 0);
        }

        public static void BumpScale(this MonoBehaviour behavior, float to, float duration)
        {
            float originalScale = behavior.transform.localScale.x;
            Tween.LocalScale(behavior.transform, behavior.transform.localScale*to, duration/2, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.LocalScale(behavior.transform, new Vector3(originalScale, originalScale, originalScale), duration/2, 0, Tween.EaseOut);
            });
        }

        public static void LerpToScale(this MonoBehaviour behavior, float to, UnityAction callback)
        {
            Tween.LocalScale(behavior.transform, behavior.transform.localScale*to, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback.I());
        }

        public static void BumpScale(this MonoBehaviour behavior, float to, UnityAction callback)
        {
            float originalScale = behavior.transform.localScale.x;
            Tween.LocalScale(behavior.transform, behavior.transform.localScale*to, 0.1f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.LocalScale(behavior.transform, new Vector3(originalScale, originalScale, originalScale), 0.1f, 0, Tween.EaseOut, Tween.LoopType.None, null, ()=>callback?.I());
            });
        }

        public static void LerpToScale(this MonoBehaviour behavior, float to, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.LocalScale(behavior.transform, behavior.transform.localScale*to, 0.1f, 0, Tween.EaseInOut);
            tween.Start();
        }

        public static void BumpScale(this MonoBehaviour behavior, float to, TweenBase tween)
        {
            float originalScale = behavior.transform.localScale.x;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.LocalScale(behavior.transform, behavior.transform.localScale*to, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                tween = Tween.LocalScale(behavior.transform, new Vector3(originalScale, originalScale, originalScale), 0.1f, 0, Tween.EaseInOut);
                tween.Start();
            });
            tween.Start();
        }

        public static void LerpToScale(this MonoBehaviour behavior, float to, float duration, UnityAction callback)
        {
            Tween.LocalScale(behavior.transform, behavior.transform.localScale*to, duration, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback.I());
        }

        public static void BumpScale(this MonoBehaviour behavior, float to, float duration, UnityAction callback)
        {
            float originalScale = behavior.transform.localScale.x;
            Tween.LocalScale(behavior.transform, behavior.transform.localScale*to, duration/2, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.LocalScale(behavior.transform, new Vector3(originalScale, originalScale, originalScale), duration/2, 0, Tween.EaseOut, Tween.LoopType.None, null, () => callback?.I());
            });
        }

        public static void LerpToScale(this MonoBehaviour behavior, float to, float duration, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.LocalScale(behavior.transform, behavior.transform.localScale*to, duration, 0, Tween.EaseInOut);
            tween.Start();
        }

        public static void BumpScale(this MonoBehaviour behavior, float to, float duration, TweenBase tween)
        {
            float originalScale = behavior.transform.localScale.x;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.LocalScale(behavior.transform, behavior.transform.localScale*to, duration / 2, 0, Tween.EaseInOut, Tween.LoopType.None, null, () =>
            {
                tween = Tween.LocalScale(behavior.transform, new Vector3(originalScale, originalScale, originalScale), duration / 2, 0, Tween.EaseInOut);
                tween.Start();
            });
            tween.Start();
        }

        public static void LerpToScale(this MonoBehaviour behavior, float to, UnityAction callback, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.LocalScale(behavior.transform, behavior.transform.localScale*to, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback.I());
            tween.Start();
        }

        public static void BumpScale(this MonoBehaviour behavior, float to, UnityAction callback, TweenBase tween)
        {
            float originalScale = behavior.transform.localScale.x;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.LocalScale(behavior.transform, behavior.transform.localScale*to, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                tween = Tween.LocalScale(behavior.transform, new Vector3(originalScale, originalScale, originalScale), 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
                tween.Start();
            });
            tween.Start();
        }

        public static void LerpToScale(this MonoBehaviour behavior, float to, float duration, UnityAction callback, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.LocalScale(behavior.transform, behavior.transform.localScale*to, duration, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback.I());
            tween.Start();
        }

        public static void BumpScale(this MonoBehaviour behavior, float to, float duration, UnityAction callback, TweenBase tween)
        {
            float originalScale = behavior.transform.localScale.x;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.LocalScale(behavior.transform, behavior.transform.localScale*to, duration, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                tween = Tween.LocalScale(behavior.transform, new Vector3(originalScale, originalScale, originalScale), duration/2, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
                tween.Start();
            });
            tween.Start();
        }
        #endregion scale interpolation

        #region color interpolation
        #region text objects
        public static void LerpToColor(this Text display, Color to)
        {
            Tween.Color(display, to, 0.1f, 0, Tween.EaseInOut);
        }

        public static void BumpColor(this Text display, Color to)
        {
            Color originalColor = display.color;
            Tween.Color(display, to, 0.1f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.Color(display, originalColor, 0.1f, 0, Tween.EaseOut);
            });
        }

        public static void LerpToColor(this Text display, Color to, float duration)
        {
            Tween.Color(display, to, duration, 0, Tween.EaseInOut);
        }

        public static void BumpColor(this Text display, Color to, float duration)
        {
            Color originalColor = display.color;
            Tween.Color(display, to, duration/2, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.Color(display, originalColor, duration/2, 0, Tween.EaseOut);
            });
        }

        public static void LerpToColor(this Text display, Color to, UnityAction callback)
        {
            Tween.Color(display, to, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
        }

        public static void BumpColor(this Text display, Color to, UnityAction callback)
        {
            Color originalColor = display.color;
            Tween.Color(display, to, 0.1f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.Color(display, originalColor, 0.1f, 0, Tween.EaseOut, Tween.LoopType.None,null, ()=>callback?.I());
            });
        }

        public static void LerpToColor(this Text display, Color to, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Color(display, to, 0.1f, 0, Tween.EaseInOut);
            tween.Start();
        }

        public static void BumpColor(this Text display, Color to, TweenBase tween)
        {
            Color originalColor = display.color;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Color(display, to, 0.1f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                tween = Tween.Color(display, originalColor, 0.1f, 0, Tween.EaseOut);
            });
            tween.Start();
        }

        public static void LerpToColor(this Text display, Color to, float duration, UnityAction callback)
        {
            Tween.Color(display, to, duration, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
        }

        public static void BumpColor(this Text display, Color to, float duration, UnityAction callback)
        {
            Color originalColor = display.color;
            Tween.Color(display, to, duration/2, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.Color(display, originalColor, duration/2, 0, Tween.EaseOut, Tween.LoopType.None, null, () => callback?.I());
            });
        }

        public static void LerpToColor(this Text display, Color to, float duration, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Color(display, to, duration, 0, Tween.EaseInOut);
            tween.Start();
        }

        public static void BumpColor(this Text display, Color to, float duration, TweenBase tween)
        {

            Color originalColor = display.color;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Color(display, to, duration/2, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                tween = Tween.Color(display, originalColor, duration/2, 0, Tween.EaseOut);
            });
            tween.Start();
        }

        public static void LerpToColor(this Text display, Color to, UnityAction callback, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Color(display, to, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            tween.Start();
        }

        public static void BumpColor(this Text display, Color to, UnityAction callback, TweenBase tween)
        {

            Color originalColor = display.color;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Color(display, to, 0.1f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                tween = Tween.Color(display, originalColor, 0.1f, 0, Tween.EaseOut, Tween.LoopType.None, null, () => callback?.I());
            });
            tween.Start();
        }

        public static void LerpToColor(this Text display, Color to, float duration, UnityAction callback, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Color(display, to, duration, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            tween.Start();
        }

        public static void BumpColor(this Text display, Color to, float duration, UnityAction callback, TweenBase tween)
        {

            Color originalColor = display.color;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Color(display, to, duration/2, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                tween = Tween.Color(display, originalColor, duration/2, 0, Tween.EaseOut, Tween.LoopType.None, null, () => callback?.I());
            });
            tween.Start();
        }
        #endregion text objects

        #region materials
        public static void LerpToColor(this Material surface, Color to)
        {
            Tween.Color(surface, to, 0.1f, 0, Tween.EaseInOut);
        }

        public static void BumpColor(this Material surface, Color to)
        {
            Color originalColor = surface.color;
            Tween.Color(surface, to, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, ()=> {
                Tween.Color(surface, originalColor, 0.1f, 0, Tween.EaseInOut);
            });
        }

        public static void LerpToColor(this Material surface, Color to, float duration)
        {
            Tween.Color(surface, to, duration, 0, Tween.EaseInOut);
        }

        public static void BumpColor(this Material surface, Color to, float duration)
        {
            Color originalColor = surface.color;
            Tween.Color(surface, to, duration/2, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                Tween.Color(surface, originalColor, duration/2, 0, Tween.EaseInOut);
            });
        }

        public static void LerpToColor(this Material surface, Color to, UnityAction callback)
        {
            Tween.Color(surface, to, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
        }

        public static void BumpColor(this Material surface, Color to, UnityAction callback)
        {
            Color originalColor = surface.color;
            Tween.Color(surface, to, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                Tween.Color(surface, originalColor, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            });
        }

        public static void LerpToColor(this Material surface, Color to, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Color(surface, to, 0.1f, 0, Tween.EaseInOut);
            tween.Start();
        }

        public static void BumpColor(this Material surface, Color to, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            Color originalColor = surface.color;
            Tween.Color(surface, to, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                tween = Tween.Color(surface, originalColor, 0.1f, 0, Tween.EaseInOut);
            });
            tween.Start();
        }

        public static void LerpToColor(this Material surface, Color to, float duration, UnityAction callback)
        {
            Tween.Color(surface, to, duration, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
        }

        public static void BumpColor(this Material surface, Color to, float duration, UnityAction callback)
        {
            Color originalColor = surface.color;
            Tween.Color(surface, to, duration / 2, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                Tween.Color(surface, originalColor, duration / 2, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            });
        }

        public static void BumpColor(this Material surface, Color to, float duration, TweenBase tween)
        {
            Color originalColor = surface.color;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Color(surface, to, duration / 2, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                tween = Tween.Color(surface, originalColor, duration / 2, 0, Tween.EaseInOut);
            });
            tween.Start();
        }

        public static void LerpToColor(this Material surface, Color to, UnityAction callback, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Color(surface, to, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            tween.Start();
        }

        public static void BumpColor(this Material surface, Color to, UnityAction callback, TweenBase tween)
        {
            Color originalColor = surface.color;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Color(surface, to, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                tween = Tween.Color(surface, originalColor, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            } );
            tween.Start();
        }

        public static void LerpToColor(this Material surface, Color to, float duration, UnityAction callback, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Color(surface, to, duration, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            tween.Start();
        }

        public static void BumpColor(this Material surface, Color to, float duration, UnityAction callback, TweenBase tween)
        {
            Color originalColor = surface.color;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Color(surface, to, duration/2, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                tween = Tween.Color(surface, originalColor, duration/2, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            });
            tween.Start();
        }
        #endregion materials

        #region shaders
        public static void LerpToColor(this Material surface, string propertyName, Color to)
        {
            Tween.ShaderColor(surface, propertyName, to, 0.1f, 0, Tween.EaseInOut);
        }

        public static void BumpColor(this Material surface, string propertyName, Color to)
        {
            Color originalColor = surface.color;
            Tween.ShaderColor(surface, propertyName, to, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                Tween.ShaderColor(surface, propertyName, originalColor, 0.1f, 0, Tween.EaseInOut);
            });
        }

        public static void LerpToColor(this Material surface, string propertyName, Color to, float duration)
        {
            Tween.ShaderColor(surface, propertyName, to, duration, 0, Tween.EaseInOut);
        }

        public static void BumpColor(this Material surface, string propertyName, Color to, float duration)
        {
            Color originalColor = surface.color;
            Tween.ShaderColor(surface, propertyName, to, duration / 2, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                Tween.ShaderColor(surface, propertyName, originalColor, duration / 2, 0, Tween.EaseInOut);
            });
        }

        public static void LerpToColor(this Material surface, string propertyName, Color to, UnityAction callback)
        {
            Tween.ShaderColor(surface, propertyName, to, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
        }

        public static void BumpColor(this Material surface, string propertyName, Color to, UnityAction callback)
        {
            Color originalColor = surface.color;
            Tween.ShaderColor(surface, propertyName, to, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                Tween.ShaderColor(surface, propertyName, originalColor, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            });
        }

        public static void LerpToColor(this Material surface, string propertyName, Color to, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.ShaderColor(surface, propertyName, to, 0.1f, 0, Tween.EaseInOut);
            tween.Start();
        }

        public static void BumpColor(this Material surface, string propertyName, Color to, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            Color originalColor = surface.color;
            Tween.ShaderColor(surface, propertyName, to, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                tween = Tween.Color(surface, originalColor, 0.1f, 0, Tween.EaseInOut);
            });
            tween.Start();
        }

        public static void LerpToColor(this Material surface, string propertyName, Color to, float duration, UnityAction callback)
        {
            Tween.ShaderColor(surface, propertyName, to, duration, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
        }

        public static void BumpColor(this Material surface, string propertyName, Color to, float duration, UnityAction callback)
        {
            Color originalColor = surface.color;
            Tween.ShaderColor(surface, propertyName, to, duration / 2, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                Tween.ShaderColor(surface, propertyName, originalColor, duration / 2, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            });
        }

        public static void BumpColor(this Material surface, string propertyName, Color to, float duration, TweenBase tween)
        {
            Color originalColor = surface.color;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.ShaderColor(surface, propertyName, to, duration / 2, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                tween = Tween.ShaderColor(surface, propertyName, originalColor, duration / 2, 0, Tween.EaseInOut);
            });
            tween.Start();
        }

        public static void LerpToColor(this Material surface, string propertyName, Color to, UnityAction callback, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.ShaderColor(surface, propertyName, to, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            tween.Start();
        }

        public static void BumpColor(this Material surface, string propertyName, Color to, UnityAction callback, TweenBase tween)
        {
            Color originalColor = surface.color;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.ShaderColor(surface, propertyName, to, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                tween = Tween.ShaderColor(surface, propertyName, originalColor, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            });
            tween.Start();
        }

        public static void LerpToColor(this Material surface, string propertyName, Color to, float duration, UnityAction callback, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.ShaderColor(surface, propertyName, to, duration, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            tween.Start();
        }

        public static void BumpColor(this Material surface, string propertyName, Color to, float duration, UnityAction callback, TweenBase tween)
        {
            Color originalColor = surface.color;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.ShaderColor(surface, propertyName, to, duration / 2, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                tween = Tween.ShaderColor(surface, propertyName, originalColor, duration / 2, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            });
            tween.Start();
        }
        #endregion shaders

        #endregion color interpolation

        #region alpha interpolation
        #region text objects
        public static void LerpToAlpha(this CanvasGroup display, float to)
        {
            Tween.CanvasGroupAlpha(display, to, 0.1f, 0, Tween.EaseInOut);
        }

        public static void BumpAlpha(this CanvasGroup display, float to)
        {
            float originalA = display.alpha;
            Tween.CanvasGroupAlpha(display, to, 0.1f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.CanvasGroupAlpha(display, originalA, 0.1f, 0, Tween.EaseOut);
            });
        }

        public static void LerpToAlpha(this CanvasGroup display, float to, float duration)
        {
            Tween.CanvasGroupAlpha(display, to, duration, 0, Tween.EaseInOut);
        }

        public static void BumpAlpha(this CanvasGroup display, float to, float duration)
        {
            float originalA = display.alpha;
            Tween.CanvasGroupAlpha(display, to, duration / 2, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.CanvasGroupAlpha(display, originalA, duration / 2, 0, Tween.EaseOut);
            });
        }

        public static void LerpToAlpha(this CanvasGroup display, float to, UnityAction callback)
        {
            Tween.CanvasGroupAlpha(display, to, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
        }

        public static void BumpAlpha(this CanvasGroup display, float to, UnityAction callback)
        {
            float originalA = display.alpha;
            Tween.CanvasGroupAlpha(display, to, 0.1f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.CanvasGroupAlpha(display, originalA, 0.1f, 0, Tween.EaseOut, Tween.LoopType.None, null, () => callback?.I());
            });
        }

        public static void LerpToAlpha(this CanvasGroup display, float to, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.CanvasGroupAlpha(display, to, 0.1f, 0, Tween.EaseInOut);
            tween.Start();
        }

        public static void BumpAlpha(this CanvasGroup display, float to, TweenBase tween)
        {
            float originalA = display.alpha;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.CanvasGroupAlpha(display, to, 0.1f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                tween = Tween.CanvasGroupAlpha(display, originalA, 0.1f, 0, Tween.EaseOut);
            });
            tween.Start();
        }

        public static void LerpToAlpha(this CanvasGroup display, float to, float duration, UnityAction callback)
        {
            Tween.CanvasGroupAlpha(display, to, duration, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
        }

        public static void BumpAlpha(this CanvasGroup display, float to, float duration, UnityAction callback)
        {
            float originalA = display.alpha;
            Tween.CanvasGroupAlpha(display, to, duration / 2, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.CanvasGroupAlpha(display, originalA, duration / 2, 0, Tween.EaseOut, Tween.LoopType.None, null, () => callback?.I());
            });
        }

        public static void LerpToAlpha(this CanvasGroup display, float to, float duration, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.CanvasGroupAlpha(display, to, duration, 0, Tween.EaseInOut);
            tween.Start();
        }

        public static void BumpAlpha(this CanvasGroup display, float to, float duration, TweenBase tween)
        {

            float originalA = display.alpha;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.CanvasGroupAlpha(display, to, duration / 2, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                tween = Tween.CanvasGroupAlpha(display, originalA, duration / 2, 0, Tween.EaseOut);
            });
            tween.Start();
        }

        public static void LerpToAlpha(this CanvasGroup display, float to, UnityAction callback, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.CanvasGroupAlpha(display, to, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            tween.Start();
        }

        public static void BumpAlpha(this CanvasGroup display, float to, UnityAction callback, TweenBase tween)
        {

            float originalA = display.alpha;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.CanvasGroupAlpha(display, to, 0.1f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                tween = Tween.CanvasGroupAlpha(display, originalA, 0.1f, 0, Tween.EaseOut, Tween.LoopType.None, null, () => callback?.I());
            });
            tween.Start();
        }

        public static void LerpToAlpha(this CanvasGroup display, float to, float duration, UnityAction callback, TweenBase tween)
        {
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.CanvasGroupAlpha(display, to, duration, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            tween.Start();
        }

        public static void BumpAlpha(this CanvasGroup display, float to, float duration, UnityAction callback, TweenBase tween)
        {

            float originalA = display.alpha;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.CanvasGroupAlpha(display, to, duration / 2, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                tween = Tween.CanvasGroupAlpha(display, originalA, duration / 2, 0, Tween.EaseOut, Tween.LoopType.None, null, () => callback?.I());
            });
            tween.Start();
        }
        #endregion text objects

        #region materials
        public static void LerpToAlpha(this Material surface, float to)
        {
            Color newColor = surface.color;
            newColor.a = to;
            Tween.Color(surface, newColor, 0.1f, 0, Tween.EaseInOut);
        }

        public static void BumpAlpha(this Material surface, float to)
        {
            Color originalColor = surface.color;
            Color newColor = surface.color;
            newColor.a = to;
            Tween.Color(surface, newColor, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                Tween.Color(surface, originalColor, 0.1f, 0, Tween.EaseInOut);
            });
        }

        public static void LerpToAlpha(this Material surface, float to, float duration)
        {
            Color newColor = surface.color;
            newColor.a = to;
            Tween.Color(surface, newColor, duration, 0, Tween.EaseInOut);
        }

        public static void BumpAlpha(this Material surface, float to, float duration)
        {
            Color newColor = surface.color;
            newColor.a = to;
            Color originalColor = surface.color;
            Tween.Color(surface, newColor, duration / 2, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                Tween.Color(surface, originalColor, duration / 2, 0, Tween.EaseInOut);
            });
        }

        public static void LerpToAlpha(this Material surface, float to, UnityAction callback)
        {
            Color newColor = surface.color;
            newColor.a = to;
            Tween.Color(surface, newColor, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
        }

        public static void BumpAlpha(this Material surface, float to, UnityAction callback)
        {
            Color newColor = surface.color;
            newColor.a = to;
            Color originalColor = surface.color;
            Tween.Color(surface, newColor, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                Tween.Color(surface, originalColor, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            });
        }

        public static void LerpToAlpha(this Material surface, float to, TweenBase tween)
        {
            Color newColor = surface.color;
            newColor.a = to;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Color(surface, newColor, 0.1f, 0, Tween.EaseInOut);
            tween.Start();
        }

        public static void BumpAlpha(this Material surface, float to, TweenBase tween)
        {
            Color newColor = surface.color;
            newColor.a = to;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            Color originalColor = surface.color;
            Tween.Color(surface, newColor, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                tween = Tween.Color(surface, originalColor, 0.1f, 0, Tween.EaseInOut);
            });
            tween.Start();
        }

        public static void LerpToAlpha(this Material surface, float to, float duration, UnityAction callback)
        {
            Color newColor = surface.color;
            newColor.a = to;
            Tween.Color(surface, newColor, duration, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
        }

        public static void BumpAlpha(this Material surface, float to, float duration, UnityAction callback)
        {
            Color newColor = surface.color;
            newColor.a = to;
            Color originalColor = surface.color;
            Tween.Color(surface, newColor, duration / 2, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                Tween.Color(surface, originalColor, duration / 2, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            });
        }

        public static void BumpAlpha(this Material surface, float to, float duration, TweenBase tween)
        {
            Color newColor = surface.color;
            newColor.a = to;
            Color originalColor = surface.color;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Color(surface, newColor, duration / 2, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                tween = Tween.Color(surface, originalColor, duration / 2, 0, Tween.EaseInOut);
            });
            tween.Start();
        }

        public static void LerpToAlpha(this Material surface, float to, UnityAction callback, TweenBase tween)
        {
            Color newColor = surface.color;
            newColor.a = to;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Color(surface, newColor, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            tween.Start();
        }

        public static void BumpAlpha(this Material surface, float to, UnityAction callback, TweenBase tween)
        {
            Color newColor = surface.color;
            newColor.a = to;
            Color originalColor = surface.color;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Color(surface, newColor, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                tween = Tween.Color(surface, originalColor, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            });
            tween.Start();
        }

        public static void LerpToAlpha(this Material surface, float to, float duration, UnityAction callback, TweenBase tween)
        {
            Color newColor = surface.color;
            newColor.a = to;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Color(surface, newColor, duration, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            tween.Start();
        }

        public static void BumpAlpha(this Material surface, float to, float duration, UnityAction callback, TweenBase tween)
        {
            Color newColor = surface.color;
            newColor.a = to;
            Color originalColor = surface.color;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.Color(surface, newColor, duration / 2, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                tween = Tween.Color(surface, originalColor, duration / 2, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            });
            tween.Start();
        }
        #endregion materials

        #region shaders
        public static void LerpToAlpha(this Material surface, string propertyName, float to)
        {
            Color newColor = surface.color;
            newColor.a = to;
            Tween.ShaderColor(surface, propertyName, newColor, 0.1f, 0, Tween.EaseInOut);
        }

        public static void BumpAlpha(this Material surface, string propertyName, float to)
        {
            Color newColor = surface.color;
            newColor.a = to;
            Color originalColor = surface.color;
            Tween.ShaderColor(surface, propertyName, newColor, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                Tween.ShaderColor(surface, propertyName, originalColor, 0.1f, 0, Tween.EaseInOut);
            });
        }

        public static void LerpToAlpha(this Material surface, string propertyName, float to, float duration)
        {
            Color newColor = surface.color;
            newColor.a = to;
            Tween.ShaderColor(surface, propertyName, newColor, duration, 0, Tween.EaseInOut);
        }

        public static void BumpAlpha(this Material surface, string propertyName, float to, float duration)
        {
            Color newColor = surface.color;
            newColor.a = to;
            Color originalColor = surface.color;
            Tween.ShaderColor(surface, propertyName, newColor, duration / 2, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                Tween.ShaderColor(surface, propertyName, originalColor, duration / 2, 0, Tween.EaseInOut);
            });
        }

        public static void LerpToAlpha(this Material surface, string propertyName, float to, UnityAction callback)
        {
            Color newColor = surface.color;
            newColor.a = to;
            Tween.ShaderColor(surface, propertyName, newColor, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
        }

        public static void BumpAlpha(this Material surface, string propertyName, float to, UnityAction callback)
        {
            Color newColor = surface.color;
            newColor.a = to;
            Color originalColor = surface.color;
            Tween.ShaderColor(surface, propertyName, newColor, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                Tween.ShaderColor(surface, propertyName, originalColor, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            });
        }

        public static void LerpToAlpha(this Material surface, string propertyName, float to, TweenBase tween)
        {
            Color newColor = surface.color;
            newColor.a = to;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.ShaderColor(surface, propertyName, newColor, 0.1f, 0, Tween.EaseInOut);
            tween.Start();
        }

        public static void BumpAlpha(this Material surface, string propertyName, float to, TweenBase tween)
        {
            Color newColor = surface.color;
            newColor.a = to;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            Color originalColor = surface.color;
            Tween.ShaderColor(surface, propertyName, newColor, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                tween = Tween.Color(surface, originalColor, 0.1f, 0, Tween.EaseInOut);
            });
            tween.Start();
        }

        public static void LerpToAlpha(this Material surface, string propertyName, float to, float duration, UnityAction callback)
        {
            Color newColor = surface.color;
            newColor.a = to;
            Tween.ShaderColor(surface, propertyName, newColor, duration, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
        }

        public static void BumpAlpha(this Material surface, string propertyName, float to, float duration, UnityAction callback)
        {
            Color newColor = surface.color;
            newColor.a = to;
            Color originalColor = surface.color;
            Tween.ShaderColor(surface, propertyName, newColor, duration / 2, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                Tween.ShaderColor(surface, propertyName, originalColor, duration / 2, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            });
        }

        public static void BumpAlpha(this Material surface, string propertyName, float to, float duration, TweenBase tween)
        {
            Color newColor = surface.color;
            newColor.a = to;
            Color originalColor = surface.color;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.ShaderColor(surface, propertyName, newColor, duration / 2, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                tween = Tween.ShaderColor(surface, propertyName, originalColor, duration / 2, 0, Tween.EaseInOut);
            });
            tween.Start();
        }

        public static void LerpToAlpha(this Material surface, string propertyName, float to, UnityAction callback, TweenBase tween)
        {
            Color newColor = surface.color;
            newColor.a = to;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.ShaderColor(surface, propertyName, newColor, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            tween.Start();
        }

        public static void BumpAlpha(this Material surface, string propertyName, float to, UnityAction callback, TweenBase tween)
        {
            Color newColor = surface.color;
            newColor.a = to;
            Color originalColor = surface.color;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.ShaderColor(surface, propertyName, newColor, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                tween = Tween.ShaderColor(surface, propertyName, originalColor, 0.1f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            });
            tween.Start();
        }

        public static void LerpToAlpha(this Material surface, string propertyName, float to, float duration, UnityAction callback, TweenBase tween)
        {
            Color newColor = surface.color;
            newColor.a = to;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.ShaderColor(surface, propertyName, newColor, duration, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            tween.Start();
        }

        public static void BumpAlpha(this Material surface, string propertyName, float to, float duration, UnityAction callback, TweenBase tween)
        {
            Color newColor = surface.color;
            newColor.a = to;
            Color originalColor = surface.color;
            if (tween != null && tween.Status == Tween.TweenStatus.Running)
                tween.Stop();
            tween = Tween.ShaderColor(surface, propertyName, newColor, duration / 2, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => {
                tween = Tween.ShaderColor(surface, propertyName, originalColor, duration / 2, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => callback?.I());
            });
            tween.Start();
        }
        #endregion shaders

        #endregion alpha interpolation

        ////// ALL TBD BEYOND THIS POINT

        #region float interpolation
        #region properties
        #endregion properties

        #region shaders
        #endregion shaders

        #endregion float interpolation

        #region Vector2 interpolation
        #region properties
        #endregion properties

        #region shaders
        #endregion shaders
        #endregion Vector2 interpolation

        #region Vector3 interpolation
        #region properties
        #endregion properties

        #region shaders
        #endregion shaders
        #endregion Vector3 interpolation
    }
}
