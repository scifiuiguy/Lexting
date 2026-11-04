using Pixelplacement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.Zones
{
    public class TextDepthAccordionHandler : MonoBehaviour
    {
        #region vars
        private List<Vector3> childStartPositions = new List<Vector3>();
        private int currentChildIndex;
        [SerializeField] private float totalDepth;
        #endregion vars

        #region init
        private void Awake()
        {
            foreach (Transform child in transform)
                childStartPositions.Add(child.position);
        }
        #endregion init

        #region helper methods
        public void LerpAccordion(bool backward, float duration)
        {
            foreach (Transform child in transform)
            {
                if (currentChildIndex > 0)
                    Tween.LocalPosition(child, 
                        (backward)
                            ? new Vector3(
                                child.transform.localPosition.x,
                                child.transform.localPosition.y,
                                childStartPositions[currentChildIndex].z * currentChildIndex / totalDepth)
                            : childStartPositions[currentChildIndex], 
                        duration, 0, Tween.EaseOut);
                currentChildIndex++;
            }
            currentChildIndex = 0;
        }
        #endregion helper methods

        #region Unit Tests
        public IEnumerator UnitTest_LerpBack()
        {
            LerpAccordion(true, 0.2f);
            yield break;
        }

        public IEnumerator UnitTest_LerpForward()
        {
            LerpAccordion(false, 0.2f);
            yield break;
        }
        #endregion Unit Tests
    }
}
