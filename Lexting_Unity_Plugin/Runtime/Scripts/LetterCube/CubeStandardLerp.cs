using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.LetterCube
{
    public class CubeStandardLerp : MonoBehaviour
    {
        public float lerpValue;
        private float _startTime;
        private IEnumerator _lerp;
        public Action<float> UpdateCallback;
        public Action<float> FinishCallback;

        public void BeginLerp(float start, float finish, float duration)
        {
            EndLerp();
            _startTime = Time.time;
            _lerp = CurrentLerp(start, finish, duration);
            StartCoroutine(_lerp);
        }

        public void EndLerp()
        {
            if (_lerp != null)
                StopCoroutine(_lerp);
        }

        IEnumerator CurrentLerp(float start, float finish, float duration)
        {
            float t = 0;

            if (finish == 3)
                Debug.Log("");

            while (t < 1)
            {
                lerpValue = Mathf.SmoothStep(start, finish, t);
                UpdateCallback?.Invoke(lerpValue);
                t = (Time.time - _startTime) / duration;
                yield return new WaitForEndOfFrame();
            }
            lerpValue = finish;
            //Debug.Log("finished lerp with value - " + finish);
            if (lerpValue == 3)
                Debug.Log("");
            FinishCallback?.Invoke(lerpValue);
            yield return null;
        }
    }
}



