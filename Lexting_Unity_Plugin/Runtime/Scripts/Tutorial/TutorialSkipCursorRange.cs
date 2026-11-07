using SpatialGestureText.TwoZoneCube;
using UnityEngine;

namespace SpatialGestureText.Tutorial
{
    /// <summary>
    /// While tutorial Skip is visible, widen hand-cursor outer radius so cursors can reach the skip cell.
    /// </summary>
    public class TutorialSkipCursorRange : MonoBehaviour
    {
        [SerializeField] private float outerHoverDistanceWhileVisible = 0.34f;

        private HandCursor[] _cursors;
        private float[] _savedOuter;
        private bool _extended;

        private void OnEnable()
        {
            ApplyExtendedRange(true);
        }

        private void OnDisable()
        {
            ApplyExtendedRange(false);
        }

        private void ApplyExtendedRange(bool extend)
        {
            if (_cursors == null || _cursors.Length == 0)
                CacheCursors();

            if (_cursors == null || _cursors.Length == 0)
                return;

            if (extend)
            {
                if (_extended)
                    return;
                _extended = true;
                if (_savedOuter == null || _savedOuter.Length != _cursors.Length)
                    _savedOuter = new float[_cursors.Length];

                for (var i = 0; i < _cursors.Length; i++)
                {
                    if (_cursors[i] == null)
                        continue;
                    _savedOuter[i] = _cursors[i].outerHoverDistanceThreshold;
                    _cursors[i].outerHoverDistanceThreshold = Mathf.Max(
                        _savedOuter[i],
                        outerHoverDistanceWhileVisible);
                }
            }
            else
            {
                if (!_extended)
                    return;
                _extended = false;
                for (var i = 0; i < _cursors.Length; i++)
                {
                    if (_cursors[i] == null)
                        continue;
                    if (_savedOuter != null && i < _savedOuter.Length)
                        _cursors[i].outerHoverDistanceThreshold = _savedOuter[i];
                }
            }
        }

        private void CacheCursors()
        {
            if (TwoZoneCubeControllers.Instance == null)
                return;
            _cursors = TwoZoneCubeControllers.Instance.GetComponentsInChildren<HandCursor>(true);
        }
    }
}
