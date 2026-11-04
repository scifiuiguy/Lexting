using SpatialGestureText.Input;
using SpatialGestureText.LetterCube;
using SpatialGestureText.TwoZoneCube;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using Pixelplacement.TweenSystem;
using SpatialGestureText.Utilities;
using UnityEngine.Events;

namespace SpatialGestureText.Hotkeys
{
	public class LextingHotkeyManager : Singleton<LextingHotkeyManager>
	{
		#region vars
		#region public vars
		/// <summary>
		/// becomes true when both hands are closed, then becomes false again when both hands open
		/// </summary>
		public bool bothHands;

		/// <summary>
		/// if true, both hands are hovering in the same zone and oriented in the same rotation, otherwise false
		/// </summary>
		public bool isHotkeyAvailable;



		/// <summary>
		/// This event invokes true just before the ExecuteHotkey method, or it invokes false if the hands failed
		/// to open within the hotkeyReleaseSpeedThreshold.
		/// </summary>
		public SpatialGestureEvent<bool> OnHotkey = new SpatialGestureEvent<bool>();


		public SpatialGestureEvent<MatrixDisplayState> OnMatrixDisplayStateChanged = new SpatialGestureEvent<MatrixDisplayState>();
		#endregion public vars
		#region private vars
		private float _timeSinceBothHandsClosed;

		/// <summary>
		/// The group (index 0) that controls alpha for the hotkey display name field, 
		/// plus any others that will exist in the future
		/// </summary>
		private CanvasGroup[] _canvasGroups;

		/// <summary>
		/// if true, the Hotkey Names are visible just above each character zone, otherwise they're invisible
		/// </summary>
		private bool _isVisible;

		public enum MatrixDisplayState
		{ 
			alpha,
			numbers,
			special
		}

		/// <summary>
		/// specifies the current kind of characters on display and available for activation in the UI matrices
		/// </summary>
		private MatrixDisplayState _currentMatrixState;

		/// <summary>
		/// The names that will be displayed at the bottom of the UI when a given hotkey is hovered
		/// </summary>
		private string[][] _hotkeyNames = new string[][] {
							// relax, pronate, supinate
				new string[] { "COPY","PASTE","CUT" },
				new string[] { "BOLD","ITALICS","UNDERLINE" },
				new string[] { "TAB","TAB FORWARD","TAB BACKWARD" },
				new string[] { "BACKWORD","BACKSPACE","DELETE" },
				new string[] { "ALT","SHIFT","CTRL" },
				new string[] { "CAPITALIZE ONE","UNDO","REDO" },
				new string[] { "CURSOR","SELECT LETTERS","SELECT WORDS" },
				new string[] { "SELECT ALL","MOUSE","MACROS & EMOJIS" },
				new string[] { "NUMBERS","ENTER","ESCAPE"}
		};

		/// <summary>
		/// The names that will be displayed at the bottom of the UI when a given hotkey is hovered
		/// while the Alt key is toggled on
		/// </summary>
		private string[][] _hotkeyAltNames = new string[][] {
			new string[] { "","","" },
			new string[] { "","","" },
			new string[] { "","","" },
			new string[] { "","","" },
			new string[] { "ALT", "SHIFT", "CTRL" },
			new string[] { "","","" },
			new string[] { "","","" },
			new string[] { "","","" },
			new string[] { "Suffix Cheatsheet", "","" }
		};

		/// <summary>
		/// The names that will be displayed at the bottom of the UI when a given hotkey is hovered
		/// while the Ctrl key is toggled on
		/// </summary>
		private string[][] _hotkeyCtrlNames = new string[][] {
			new string[] { "","","" },
			new string[] { "","","" },
			new string[] { "","","" },
			new string[] { "","","" },
			new string[] { "ALT", "SHIFT", "CTRL" },
			new string[] { "","","" },
			new string[] { "","","" },
			new string[] { "","","" },
			new string[] { "","","" }
		};

		/// <summary>
		/// The names that will be displayed at the bottom of the UI when a given hotkey is hovered
		/// while the Shiftkey is toggled on
		/// </summary>
		private string[][] _hotkeyShiftNames = new string[][] {
			new string[] { "","","" },
			new string[] { "","","" },
			new string[] { "","","" },
			new string[] { "","","" },
			new string[] { "ALT", "SHIFT", "CTRL" },
			new string[] { "","","" },
			new string[] { "","","" },
			new string[] { "","","" },
			new string[] { "","","" }
		};

		/// <summary>
		/// A cached reference to the names array currently to be displayed 
		/// (e.g. default is _hotkeyNames, but we assign _altHotkeyNames instead if Alt key is toggled on
		/// </summary>
		private string[][] _currentDisplayTarget;

		[SerializeField] private Text _hotkeyNameDisplay;

		/// <summary>
		/// Uses a CancellableWait function to call back as soon as the threshold time has expired and trigger
		/// OnHotkey(false) if the hotkey was not executed prior to expiry.
		/// If the hotkey IS executed before expiry, it needs to use this var to cancel the failure check.
		/// </summary>
		private IDisposable _hotkeyFailureObserver;

		/// <summary>
		/// the time interval within which the user must release their hands onces both are closed in order to
		/// activate a desired hotkey
		/// </summary>
		[SerializeField] public float hotkeyReleaseSpeedThreshold;

		private UnityAction _hotKeyAction;

		/// <summary>
		/// Cached ref to the output manager to cut down on repetitive Instance cruft
		/// </summary>
		private LextingOutputManager _o;

		/// <summary>
		/// Used to observe changes to the current line's index in the ActivateHotkeySelctor method
		/// </summary>
		private IDisposable _dragLoop;

		private bool _currentSelectorIsRight;
		private int _currentSelectorIndex;
		#endregion private vars
		#endregion vars

		#region init
		private void Awake()
		{
			this.Chs(ref _canvasGroups);
			this.WaitF(1, () => _o = LextingOutputManager.Instance);
			RegisterAllEventsAndActions();
		}
		#endregion init

		#region loops

		#endregion loops

		#region event handlers
		private void RegisterAllEventsAndActions()
		{
            this.GetGestures(
                (right, current, previous) =>
                {
                    //// if both hands became tapped this frame, start the timer
                    //if ((right && current == HandGestureState.Insert && previous != HandGestureState.Insert
                    //    && Lexting.lhState == HandGestureState.Insert)
                    //    || (!right && current == HandGestureState.Insert && previous != HandGestureState.Insert
                    //    && Lexting.rhState == HandGestureState.Insert))
                    //{
                    //    _timeSinceBothHandsClosed = Time.time;
                    //    bothHands = true;

                    //    // if both hands haven't opened by expiry, inform the rest of the system it may carry on
                    //    _hotkeyFailureObserver = this.CancellableWaitS(hotkeyReleaseSpeedThreshold, () => OnHotkey?.I(false));
                    //}
                    //// if both hands have released after both were tapped within the time threshold, and both hands are 
                    //// hovering the same key, execute the hotkey for that key.
                    //else if (bothHands && Lexting.rhState != HandGestureState.Insert
                    //        && Lexting.lhState != HandGestureState.Insert)
                    //{
                    //    bothHands = false;
                    //    //Debug.Log($"current release speed: {Time.time - _timeSinceBothHandsClosed}");
                    //    if (Time.time - _timeSinceBothHandsClosed < hotkeyReleaseSpeedThreshold
                    //        && TwoZoneTrigger.currentLeftHighlightedLetter == TwoZoneTrigger.currentRightHighlightedLetter)
                    //    {
                    //        _hotkeyFailureObserver?.Dispose();
                    //        OnHotkey?.I(true);
                    //        ExecuteHotkey(TwoZoneTrigger.currentLeftHighlightedLetter);
                    //    }
                    //}
                },
                (right, current, previous) =>
                {
					if (!isHotkeyAvailable || _currentSelectorIsRight != right) return;

					_currentDisplayTarget = (_o.altSolo) ? _hotkeyAltNames
						: (_o.shiftSolo) ? _hotkeyShiftNames
						: (_o.ctrlSolo) ? _hotkeyCtrlNames : _hotkeyNames;
					_hotkeyNameDisplay.text =
						_currentDisplayTarget[_currentSelectorIndex][(int)Lexting.rotState(right)];
				});
        }

        /// <summary>
        /// Checks to see if both hands are hovering in the same zone and rotated to the same state.
        /// If so, display the available hotkey for that zone & rotation.
        /// </summary>
        //public void OnRotationChange()
        //{
			

        //    if (Lexting.controllers.currentLeftTrigger == null || Lexting.controllers.currentRightTrigger == null) return;
        //    try
        //    {
        //        if (Lexting.controllers.currentLeftTrigger?.Index == Lexting.controllers.currentRightTrigger?.Index
        //            && (int)Lexting.lhRotState <= 2 && Lexting.lhRotState == Lexting.rhRotState)
        //        {
        //            _currentDisplayTarget = (_o.altSolo) ? _hotkeyAltNames
        //                : (_o.shiftSolo) ? _hotkeyShiftNames
        //                : (_o.ctrlSolo) ? _hotkeyCtrlNames : _hotkeyNames;
        //            _hotkeyNameDisplay.text =
        //                _currentDisplayTarget[Lexting.controllers.currentLeftTrigger.Index][(int)Lexting.lhRotState];
        //            ToggleHotKeyVisibility(true);
        //            isHotkeyAvailable = true;
        //        }
        //        else
        //        {
        //            ToggleHotKeyVisibility(false);
        //            isHotkeyAvailable = false;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.Log(e.Message);
        //    }

        //}
        #endregion event handlers



        #region helper methods
        public void ExecuteHotkey(string letter)
		{
			//Debug.Log($"LextingHotkeyManager - ExecuteHotkey - letter: {letter}");
			_hotkeyNameDisplay.BumpScale(0.9f);
			_hotkeyNameDisplay.BumpDown(0.02f, 0.2f);

			try {
				if (_o.altSolo)
					_hotKeyAction = letter switch
					{ // alt
						"g" => _o.ToggleAlt, "i" => _o.ToggleShift, "k" => _o.ToggleCtrl,
						_ => throw new NotImplementedException()
					};
				else if (_o.ctrlSolo)
					_hotKeyAction = letter switch { // ctrl
						"g" => _o.ToggleAlt, "i" => _o.ToggleShift, "k" => _o.ToggleCtrl,
						_ => throw new NotImplementedException()
					};
				else if (_o.shiftSolo)
					_hotKeyAction = letter switch { // shift
						"g" => _o.ToggleAlt, "i" => _o.ToggleShift, "k" => _o.ToggleCtrl,
						_ => throw new NotImplementedException()
					};
				else _hotKeyAction = letter switch { // default
					" "=>null, "x"=>null, "z"=>null, "c"=>_o.BackWord, "o"=>_o.BackSpace, 
					"r"=>_o.Delete, "m"=>_o.CapitalizeNextLetter, "Caps_1st" => _o.Enter, "[ENTER]" => _o.Enter,
					"->Numbers" => _o.Escape, "->Special" => _o.Escape, "->Alpha" => _o.Escape,
					"s"=>_o.Undo, "w"=>_o.Redo, "a"=>_o.Tab,
					"e"=>_o.AltTab, "y"=>_o.AltShiftTab, "g"=>_o.ToggleAlt, "i"=>_o.ToggleShift, "k"=>_o.ToggleCtrl,
					"t"=>_o.Copy, "b"=>_o.Paste, "h"=>_o.Cut, "n"=>_o.ToggleBold, "l"=>_o.ToggleItalics, 
					"j" => _o.ToggleUnderline, "u" => _o.MoveCursor, "p" => _o.DragCursor, "q" => _o.DragWordHighlight,
					"d" => _o.SelectAll, "v"=>LaunchMouse, "f"=>LaunchMacros, _ => throw new NotImplementedException()
				};
			}
			catch (NotImplementedException e)
			{ 
			
			}
			
			_hotKeyAction?.I();
		}

		private void ToggleHotKeyVisibility()
		{
			_isVisible = !_isVisible;
			foreach (CanvasGroup group in _canvasGroups)
				group.alpha = (_isVisible) ? 1 : 0;
		}

		private void ToggleHotKeyVisibility(bool on)
		{
			_isVisible = on;
			foreach (CanvasGroup group in _canvasGroups)
				group.alpha = (_isVisible) ? 1 : 0;
		}

		public void CycleMatrix()
		{
			_currentMatrixState = (MatrixDisplayState)(((int)_currentMatrixState + 1) % 3);
				
			//switch (_currentMatrixState)
			//{
			//	case MatrixDisplayState.alpha: _currentMatrixState = MatrixDisplayState.numbers; break;
			//	case MatrixDisplayState.numbers: _currentMatrixState = MatrixDisplayState.special; break;
			//	case MatrixDisplayState.special: _currentMatrixState = MatrixDisplayState.alpha; break;
			//}
			RefreshMatrixNameDisplay();
			OnMatrixDisplayStateChanged?.I(_currentMatrixState);
		}

		private void RefreshMatrixNameDisplay()
		{
			switch (_currentMatrixState)
			{
				case MatrixDisplayState.alpha: _hotkeyNames[8][0] = "NUMBERS"; break;
				case MatrixDisplayState.numbers: _hotkeyNames[8][0] = "SPECIAL CHARACTERS"; break;
				case MatrixDisplayState.special: _hotkeyNames[8][0] = "ALPHA CHARACTERS"; break;
			}
			_hotkeyNameDisplay.text = _hotkeyNames[Lexting.controllers.currentLeftTrigger.Index][(int)Lexting.lhRotState];
		}

		private void ToggleConjugationLegend()
		{
			SuffixLegendHandler.Instance.ToggleVisibility(true);
		}

		//private void ToggleSpecialMatrix()
		//{
		//	if (_specialMatrix)
		//	{
		//		// dismiss the special character matrix and reveal the default matrix
		//	}
		//	else 
		//	{
		//		// dismiss the default matrix and reveal the special character matrix
		//	}
		//}

		//private void ToggleNumberMatrix()
		//{
		//	if (_numberMatrix)
		//	{
		//		// dismiss the number matrix and revea the default matrix
		//	}
		//	else
		//	{
		//		// dismiss the default matrix and reveal the number matrix
		//	}
		//}

		private void LaunchMouse()
		{
			// TBD - I had a prototype working with the 3-zone cube last year, but had to scrap it
			// when I transitioned to the 2-zone cube.

			// When ready, this will allow the user map X and Y of local hand pose to X and Y of 
			// the networked PC or a PC that uses Leap Motion.
		}

		private void LaunchMacros()
		{
			// TBD - will allow users to define 2-to-4-letter acronyms that can each output
			// a macro string (word or phrase), an emoji, or any combination, w/ 9 conjugation options available
		}

		/// <summary>
		/// Observes changes to the hovered index of the currently dragging line so the
		/// hotkey manager can display the hotkey available at that index.
		/// </summary>
		/// <param name="line"></param>
		public void ActivateHotkeySelector(bool right, CubeDragLineVisualizer line)
		{
			RefreshSelector(right, line.currentTwoZoneIndex);
			
			_dragLoop = line.ObserveEveryValueChanged(x => x.currentTwoZoneIndex).Subscribe(index => {
				if (index >= 9) return;
				RefreshSelector(right, line.currentTwoZoneIndex);
				_currentDisplayTarget = (_o.altSolo) ? _hotkeyAltNames
						: (_o.shiftSolo) ? _hotkeyShiftNames
						: (_o.ctrlSolo) ? _hotkeyCtrlNames : _hotkeyNames;
				_hotkeyNameDisplay.text = _currentDisplayTarget[index][(int)Lexting.lhRotState];
				ToggleHotKeyVisibility(true);
				isHotkeyAvailable = true;
			});
			this.WaitF(2,()=>this.Wait(() => !line.isLineTracingActive, () => 
			{
				ToggleHotKeyVisibility(false);
				isHotkeyAvailable = false;
				_dragLoop.Dispose();
			}));
		}

		private void RefreshSelector(bool right, int index)
		{
			_currentSelectorIsRight = right;
			_currentSelectorIndex = index;
		}
		#endregion helper methods

		#region Unit Tests

		#endregion Unit Tests
	}
}
