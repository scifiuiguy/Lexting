using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.LetterCube
{
    public class CubeLetterTransition : MonoBehaviour
    {
        private CubeStandardLerp _lerp;
        private float[] letterScales = new float[3] { 1, 1.3f, 1.5f };
        private bool _isInitialized;

        public bool isScaling;
        public bool small;
        public bool backRow, frontRow;
        public LetterSize currentSize;
        

        private void Awake()
        {
            if (!_isInitialized)
                InitializeTransition();
        }

        private void InitializeTransition()
        {
            _lerp = GetComponent<CubeStandardLerp>();
            _lerp.UpdateCallback += LerpUpdate;
            _lerp.FinishCallback += FinishLerp;
            _isInitialized = true;
        }

        private void LerpUpdate(float newValue)
        {
            transform.localScale = new Vector3(newValue, newValue, newValue);
        }

        private void FinishLerp(float result)
        {
            isScaling = false;
            transform.localScale = new Vector3(result, result, result);
            for (int i = 0; i < 3; i++)
                if (result == letterScales[i])
                {
                    currentSize = (LetterSize)i;
                    break;
                }
        }

        public void ChangeSize(LetterSize newSize)
        {
            if (!_isInitialized)
                InitializeTransition();

            if (isScaling) return;
            isScaling = true;
            _lerp.BeginLerp(letterScales[(int)currentSize], letterScales[(int)newSize], 0.1f);
        }
        
        public void ClickBump()
        {
            if (!_isInitialized)
                InitializeTransition();

            if (isScaling) return;
            isScaling = true;
            StartCoroutine(ClickBumpSequence());
        }

        private IEnumerator ClickBumpSequence()
        {
            ChangeSize(LetterSize.clicked);
            yield return new WaitUntil(() => !isScaling);
            ChangeSize(LetterSize.hovered);
        }
    }

    public enum LetterSize
    {
        neutral,
        hovered,
        clicked
    }
}

