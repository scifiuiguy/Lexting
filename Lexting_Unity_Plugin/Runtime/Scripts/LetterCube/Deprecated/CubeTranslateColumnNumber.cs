using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.LetterCube
{

    public class CubeTranslateColumnNumber : MonoBehaviour
    {
        private CubeStandardLerp _lerp;
        private float _startingX;

        public bool isTranslating;
        public bool on;
        public bool backRow, frontRow;

        private void Awake()
        {
            _startingX = transform.localPosition.x;
            _lerp = GetComponent<CubeStandardLerp>();
            _lerp.UpdateCallback += LerpUpdate;
            _lerp.FinishCallback += DisableListeners;
        }

        void LerpUpdate(float newValue)
        {
            transform.localPosition = new Vector3(newValue, transform.localPosition.y, transform.localPosition.z);
        }

        void DisableListeners(float result)
        {
            isTranslating = false;
            transform.localPosition = new Vector3(result, transform.localPosition.y, transform.localPosition.z);
            on = result == 0;
        }

        public void ToggleOn()
        {
            isTranslating = true;
            _lerp.BeginLerp(_startingX, 0, 0.1f);
        }

        public void ToggleOff()
        {
            isTranslating = true;
            _lerp.BeginLerp(0, _startingX, 0.1f);
        }

        private void Update()
        {
            if (transform.hasChanged && !isTranslating)
            {
                if (!on)
                {
                    if (transform.parent.parent.localPosition.z + transform.parent.localPosition.z > -30f &&
                        transform.parent.parent.localPosition.z + transform.parent.localPosition.z < 30f)
                        ToggleOn();
                }
                else
                {
                    if ((transform.parent.parent.localPosition.z + transform.parent.localPosition.z <= -30f && !backRow) ||
                        transform.parent.parent.localPosition.z + transform.parent.localPosition.z >= 30f && !frontRow)
                        ToggleOff();
                }
            }
        }
    }
}
