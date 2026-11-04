using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpatialGestureText.LetterCube
{
    /// <summary>
    /// deprecated - not needed bc spatial list controller overlays cube triggers one-to-one which already highlight effectively
    /// </summary>
    public class CubeWordAssemblyHighlighter : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _textHighlight;
        [SerializeField] private CubeWordAssemblyController _assemblyController;
        private Text _targetText;


        private void GetTargetText()
        {
            if (_canvasGroup.transform.childCount == 0)
                Debug.LogError("Word Assembly Highlighter can't locate text to highlight.");
            else _targetText = _canvasGroup.transform.GetChild(0).GetComponent<Text>();
        }

        private float GetTotalTextCharWidth()
        {
            float total = 0;
            for (int i = 6; i < _targetText.cachedTextGenerator.vertexCount; i += 6)
                total += Mathf.Abs((_targetText.cachedTextGenerator.verts[i].position - _targetText.cachedTextGenerator.verts[i - 6].position).magnitude);
            return total;
        }

        /// <summary>
        /// Returns pixel width of 1st X chars of the target text 
        /// </summary>
        private float GetTextCharWidthOfFirstX(int X)
        {
            float total = 0;
            for (int i = 6; i < X*6; i += 6)
                total += Mathf.Abs((_targetText.cachedTextGenerator.verts[i].position - _targetText.cachedTextGenerator.verts[i - 6].position).magnitude);
            return total;
        }
    }
}
