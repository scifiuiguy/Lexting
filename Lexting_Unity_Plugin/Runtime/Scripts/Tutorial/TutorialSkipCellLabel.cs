using SpatialGestureText.TwoZoneCube;
using UnityEngine;
using UnityEngine.UI;

namespace SpatialGestureText.Tutorial
{
    /// <summary>
    /// Static SKIP label on the skip cell — same white/black + material swap as <see cref="LetterSwitcher"/>.
    /// </summary>
    public class TutorialSkipCellLabel : MonoBehaviour
    {
        private const int HoverTextRenderQueue = 4000;

        private Text _letterDisplay;
        private Material _neutralMat;
        private Material _highlightTextMaterial;
        private Color _neutralColor = Color.white;
        private Color _highlightedColor = Color.black;
        private bool _isHighlighted;

        public void Configure(Text letterDisplay, LetterSwitcher styleSource)
        {
            _letterDisplay = letterDisplay;
            if (_letterDisplay == null)
                return;

            _letterDisplay.text = "SKIP";
            _neutralColor = _letterDisplay.color;
            _neutralMat = styleSource != null ? styleSource.NeutralLabelMaterial : _letterDisplay.material;

            if (styleSource?.HighlightedLabelMaterial != null)
            {
                _highlightTextMaterial = Object.Instantiate(styleSource.HighlightedLabelMaterial);
                _highlightTextMaterial.renderQueue = HoverTextRenderQueue;
            }

            if (_neutralMat != null)
                ApplyLetterMaterial(_neutralMat);
            _letterDisplay.color = _neutralColor;
            _isHighlighted = false;
        }

        public void SetHovered(bool on)
        {
            if (_letterDisplay == null || _isHighlighted == on)
                return;

            _isHighlighted = on;
            _letterDisplay.color = on ? _highlightedColor : _neutralColor;
            if (on && _highlightTextMaterial != null)
                ApplyLetterMaterial(_highlightTextMaterial);
            else if (_neutralMat != null)
                ApplyLetterMaterial(_neutralMat);
        }

        private void ApplyLetterMaterial(Material mat)
        {
            if (_letterDisplay == null || mat == null)
                return;
            _letterDisplay.material = mat;
        }
    }
}
