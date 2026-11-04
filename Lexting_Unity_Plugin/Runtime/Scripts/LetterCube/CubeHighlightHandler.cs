using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.LetterCube
{
    public class CubeHighlightHandler : MonoBehaviour
    {
        private MeshRenderer _renderer;
        [SerializeField] Material _positiveMat, _negativeMat;

        private bool _correct;

        private void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
        }

        public void Toggle(bool on, bool correct)
        {
            _correct = correct;
            if (_correct)
                _renderer.material = _positiveMat;
            else _renderer.material = _negativeMat;
            _renderer.enabled = on;
        }
    }
}