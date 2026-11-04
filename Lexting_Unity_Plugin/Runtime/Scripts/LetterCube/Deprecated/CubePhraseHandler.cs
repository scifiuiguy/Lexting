using SpatialGestureText.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpatialGestureText.LetterCube
{
    //deprecated
    /// <summary>
    /// Tracks the status of a phrase as it is being constructed.
    /// </summary>
    //public class CubePhraseHandler : MonoBehaviour
    //{
    //    #region vars
    //    public Action<string> PhraseCompleted;

    //    private string _currentPhrase, _currentCharacter;
    //    private Text _prefix, _postfix;
    //    private int _currentCursorIndex;
    //    private CubeForwardTrigger _forwardTrigger;
    //    private CubeBackTrigger _backTrigger;
    //    private SpatialGestureInput _input;
    //    private bool _rightIsTyping, _leftIsTyping;
    //    #endregion vars

    //    #region init
    //    private void Awake()
    //    {
    //        _prefix = transform.GetChild(0).GetComponent<Text>();
    //        if (_prefix == null)
    //            Debug.LogError("PhraseHandler couldn't find prefix Text component.");
    //        _postfix = transform.GetChild(2).GetComponent<Text>();
    //        if (_postfix == null)
    //            Debug.LogError("PhraseHandler couldn't find postfix Text component.");
    //        _currentPhrase = "";
    //        _currentCursorIndex = 0;
    //        _forwardTrigger = CubeForwardTrigger.Instance;
    //        _backTrigger = CubeBackTrigger.Instance;
    //        _input = SpatialGestureInput.Instance;
    //    }

    //    private void OnEnable()
    //    {
    //        CubeLetterTrigger.LetterHighlightActivated += OnLetterHighlightActivated;
    //        _forwardTrigger.LetterSelectionActivated += OnLetterSelected;
    //        _backTrigger.MoveSenderCursorBack += OnMoveSenderCursorBack;
    //        _input.OnRightStateChanged += OnRightStateChanged;
    //        _input.OnLeftStateChanged += OnLeftStateChanged;
    //    }

    //    private void OnDisable()
    //    {
    //        CubeLetterTrigger.LetterHighlightActivated -= OnLetterHighlightActivated;
    //        _forwardTrigger.LetterSelectionActivated -= OnLetterSelected;
    //        _backTrigger.MoveSenderCursorBack -= OnMoveSenderCursorBack;
    //        _input.OnRightStateChanged -= OnRightStateChanged;
    //        _input.OnLeftStateChanged -= OnLeftStateChanged;
    //    }
    //    #endregion init

    //    #region event handlers

    //    private void OnLetterHighlightActivated(string highlightedLetter)
    //    {
    //        _currentCharacter = highlightedLetter;
    //    }

    //    private void OnLetterSelected(string selectedLetter)
    //    {
    //        Debug.Log("PhraseHandler got a new letter to type: " + selectedLetter);
    //        _currentCursorIndex++;
    //        //if (_postfix.text == "")
    //        //{
    //        _currentPhrase += _currentCharacter;
    //        _prefix.text += _currentCharacter;
    //        //}
    //        //else
    //        //{
    //        //    _currentCharacter = _postfix.text.Substring(0, 1);
    //        //    CubeSlideRail.Instance.currentIndex = CubeLetterTrigger.GetIndexByLetter(_currentCharacter);
    //        //    _postfix.text = _postfix.text.Substring(1);
    //        //}
    //    }

    //    private void OnMoveSenderCursorBack()
    //    {
    //        if (_currentCursorIndex > 0)
    //        {
    //            _currentCursorIndex--;
    //            //_postfix.text = _currentCharacter + _postfix.text;

    //            if (_prefix.text.Length > 0)
    //            {
    //                _currentCharacter = _prefix.text.Substring(_prefix.text.Length - 1);
    //                Debug.Log(string.Format("PhraseHandler got a new letter to remove: {0}",
    //                                        _currentCharacter));
    //                CubeSlideRail.Instance.currentIndex = CubeLetterTrigger.GetIndexByLetter(_currentCharacter);
    //                Debug.Log(string.Format("PhraseHandler got a new letter to remove: {0} {1}",
    //                                        CubeSlideRail.Instance.currentIndex.row,
    //                                        CubeSlideRail.Instance.currentIndex.column));
    //                _prefix.text = _prefix.text.Substring(0, _prefix.text.Length - 1);
    //            }
    //        }
    //    }

    //    private void OnRightStateChanged(HandGestureState newState, HandGestureState previousState)
    //    {
    //        if (_rightIsTyping && newState == HandGestureState.Neutral && previousState != HandGestureState.Neutral)
    //        {
    //            _rightIsTyping = false;
    //            PhraseCompleted?.Invoke(_currentPhrase);
    //            _currentPhrase = "";
    //            _prefix.text = "";
    //            _currentCharacter = "";
    //            _postfix.text = "";
    //        }
    //        else _rightIsTyping = true;

    //    }

    //    private void OnLeftStateChanged(HandGestureState newState, HandGestureState previousState)
    //    {
    //        if (_leftIsTyping && newState == HandGestureState.Neutral && previousState != HandGestureState.Neutral)
    //        {
    //            _leftIsTyping = false;
    //            PhraseCompleted?.Invoke(_currentPhrase);
    //            _currentPhrase = "";
    //            _prefix.text = "";
    //            _currentCharacter = "";
    //            _postfix.text = "";
    //        }
    //        else _leftIsTyping = true;
    //    }

    //    #endregion event handlers
    //}
}

