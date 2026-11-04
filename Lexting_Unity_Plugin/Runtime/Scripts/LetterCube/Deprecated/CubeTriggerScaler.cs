using Pixelplacement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SpatialGestureText.LetterCube
{
    public class CubeTriggerScaler : MonoBehaviour
    {
        public int index;
        private float _initScale;
        private float _bumpScale = 0.8f;
        private Vector3 _bumpScaleCache;

        public void InitAtScale(float scale)
        {
            transform.localScale = new Vector3(scale, scale, scale);
            _initScale = scale;
            _bumpScaleCache = new Vector3(_bumpScale, _bumpScale, _bumpScale);
        }

        public void Introduce(AnimationCurve curve, UnityAction callback)
        {
            Tween.LocalScale(transform, Vector3.one, 0.2f, 0, curve, Tween.LoopType.None, null, () => callback?.I());
        }

        public void Dismiss(AnimationCurve curve, UnityAction callback)
        {
            Tween.LocalScale(transform, new Vector3(_initScale, _initScale, _initScale), 0.2f, 0, curve, Tween.LoopType.None, null, () => callback?.I());
        }

        public void Bump(UnityAction callback)
        {
            Tween.LocalScale(transform, _bumpScaleCache, 0.05f, 0, Tween.EaseIn, Tween.LoopType.None, null, () => {
                Tween.LocalScale(transform, Vector3.one, 0.05f, 0, Tween.EaseOut, Tween.LoopType.None, null, ()=>callback?.I());
            });
        }
    }
}

