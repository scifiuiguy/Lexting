using SpatialGestureText.Hotkeys;
using SpatialGestureText.Input;
using SpatialGestureText.LetterCube;
using SpatialGestureText.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static SpatialGestureText.Hotkeys.LextingHotkeyManager;

namespace SpatialGestureText.TwoZoneCube
{
	public class LetterSwitcher : MonoBehaviour
	{
        #region vars
        #region public vars
        [SerializeField] public List<string> letters;
        [HideInInspector] public TwoZoneCubeController controller;

        /// <summary>
        /// An offset to be added to the current letters index to modulate from alpha to numbers to special characters
        /// </summary>
        private int _indexOffset;
        public bool IsCenter
        {
            get
            {
                if (letters.Count > 0)
                    return letters[0] == "SPACE";
                else return false;
            }
        }
        #endregion public vars
        #region private vars
        //private const int _defaultFont = 300;
        //private const int _spaceFont = 150;
        private Text _letterDisplay;
        // Keep serialized field order stable for prefabs (materials first); hover uses Text.color only.
        [SerializeField] private Material neutralMat;
        [SerializeField] private Material highlightedMat;
        private Color _neutralColor = Color.white;
        private Color _highlightedColor = Color.black;
        private bool _isHighlighted;

        /// <summary>
        /// rotation state for this letter's hand
        /// </summary>
        private CubeRotationState _currentRotationState => (controller.IsRight) ? Lexting.rhRotState : Lexting.lhRotState;

        private Vector3 _startingPosition;
        #endregion private vars
        #endregion vars

        #region init
        private void Awake()
		{
            _startingPosition = transform.position;
            _letterDisplay = GetComponent<Text>();
            _letterDisplay.text = letters[0];
            _neutralColor = _letterDisplay.color;
            if (neutralMat != null)
                _letterDisplay.material = neutralMat;
            this.Wait(()=>controller.Ready(), ()=>RegisterAllEventsAndActions());
		}
        #endregion init

        #region loops

        #endregion loops

        #region event handlers
        private void RegisterAllEventsAndActions()
        {
            this.Subscribe(Lexting.hotkeys.OnMatrixDisplayStateChanged, (newMatrixState) => {
                _indexOffset = newMatrixState switch
                {
                    MatrixDisplayState.alpha => 0,
                    MatrixDisplayState.numbers => 3,
                    MatrixDisplayState.special => 6,
                    _=>throw new NotImplementedException()
                };
                _letterDisplay.text = letters[(int)_currentRotationState + _indexOffset];
            });

            this.GetGestures(
                (right,current,previous) => 
                {
                    // gesture state
                    //if (right != controller.IsRight) return;
                },
                (right,current,previous) => 
                {
                    // rotation state
                    if (right != controller.IsRight || (int)current > 3) return;

                    if (letters.Count > 9 && current == CubeRotationState.forward)
                        _letterDisplay.text = letters[9];
                    else if (current == CubeRotationState.forward) return;
                    else _letterDisplay.text = letters[(int)current + _indexOffset];
                    //_letterDisplay.fontSize = _letterDisplay.text.Equals("SPACE") ? _spaceFont : _defaultFont;                    
                }
            );
        }
        #endregion event handlers

        #region helper methods
        public void ToggleMaterial(bool on)
        {
            if (_isHighlighted == on) return;
            _isHighlighted = on;
            _letterDisplay.color = on ? _highlightedColor : _neutralColor;
        }

        private void BounceToFront()
        {
            transform.position = _startingPosition + transform.forward * 0.1f;
        }

        public void BounceBack()
        {
            transform.position = _startingPosition;
        }
		#endregion helper methods
		
		#region Unit Tests
		
		#endregion Unit Tests
	}
}
