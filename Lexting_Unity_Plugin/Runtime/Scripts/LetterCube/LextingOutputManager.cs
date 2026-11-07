using SpatialGestureText.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WindowsInput;
using WindowsInput.Native;
using System.Runtime.InteropServices;
using System.Diagnostics;
using SpatialGestureText.Utilities;
using SpatialGestureText.Interfaces;
using SpatialGestureText.TwoZoneCube;
//using System.Windows.Forms;
//using UnityEngine.UI.Mod;

namespace SpatialGestureText.LetterCube
{
    public class LextingOutputManager : Singleton<LextingOutputManager>
    {

        #region vars
        #region private vars
        /// <summary>
        /// the length of the remaining string following caret position right after a delete command
        /// </summary>
        private int _numCharsAfterDeletionIndex;

        /// <summary>
        /// the remaining string following caret position right after a delete command executes
        /// </summary>
        private string _newSuffix;

        /// <summary>
        /// used to restore caretPosition whenever CurrentOutputValue updates, since several update actions cause
        /// the caret to mistakenly jump to the end of the string in Unity's default InputField
        /// </summary>
        private int _caretPositionCache;

        /// <summary>
        /// A cached reference to what will be the current word's new length immediately after
        /// the replacement operation
        /// </summary>
        private int _newWordLength;

        /// <summary>
        /// Stores state of single-char capitalization for the next letter to be appended to CurrentOutputValue
        /// </summary>
        private bool _capsOne;

        /// <summary>
        /// A cache of the current output change requested from anywhere else in the interface
        /// </summary>
        private string _output;

        /// <summary>
        /// Values used to time-lock the BackWord function temporarily so it can't double-execute by mistake
        /// </summary>
        private float _timeSinceLastBackWord = 0;
        private float _backWordMinRepeatInterval = 0.2f;
        #endregion private vars
        #region public vars

        public enum CapsState
        {
            lower,
            CapsOne,
            CapsAll
        }

        /// <summary>
        /// The current state of capitalization of all incoming input characters 
        /// (lower - all input will be lowercase, CapsOne - capitalize only the next letter, CapsAll - capitalize all input) 
        /// </summary>
        public CapsState capsState;

        /// <summary>
        /// A list of all Unity UI text labels that are currently subscribed to receive real-time text entry input
        /// </summary>
        [HideInInspector] public List<Text> displays = new List<Text>();

        /// <summary>
        /// A list of all Unity UI Input Fields that are currently subscribed to receive real-time text entry input
        /// </summary>
        [HideInInspector] public List<InputField> fields = new List<InputField>();

        /// <summary>
        /// The current string value that should be refreshed in real time if this manager is printing manually
        /// to receiving fields and displays
        /// </summary>
        public string CurrentOutputValue { get; private set; }

        /// <summary>
        /// If true, the current selected platform is submitting key codes to windowsKeySim rather than printing 
        /// output manually to receiving displays and fields
        /// </summary>
        public bool isSendingKeysToWindows;

        /// <summary>
        /// An API used to submit Windows ASCII keyboard invocation commands when broadcasting to a Windows PC via LAN or PC app
        /// </summary>
        public InputSimulator windowsKeySim;

        /// <summary>
        /// if true, at least one active element exists that is capable of receiving text output right now
        /// </summary>
        public bool isActive => displays.Count + fields.Count > 0;

        /// <summary>
        /// the characters immediately preceding the first input char during word mode (up to the first found space)
        /// </summary>
        [HideInInspector] public string prefix = "";

        /// <summary>
        /// If true, it's the Lexting equivalent of SHIFT key held down. False is SHIFT key released.
        /// </summary>
        [HideInInspector] public bool shift;

        /// <summary>
        /// if true, shift is toggled on but alt and ctrl are not
        /// </summary>
        public bool shiftSolo => shift && !alt && !ctrl;

        /// <summary>
        /// If true, it's the Lexting equivalent of ALT key held down. False is ALT key released.
        /// </summary>
        [HideInInspector] public bool alt;

        public bool altSolo => alt && !shift && !ctrl;

        /// <summary>
        /// If true, it's the Lexting equivalent of ctrl key held down. False is ctrl key released.
        /// </summary>
        [HideInInspector] public bool ctrl;

        /// <summary>
        /// if true, ctrl is toggled on but alt and shift are not
        /// </summary>
        public bool ctrlSolo => ctrl && !alt && !shift;

        #region Third-Party Integration events
        /// <summary>
        /// Events that trigger when the specified key is invoked... 3rd parties can use these to extend 
        /// keyboard commands, e.g. any app that integrates Lexting may want to do something unique when the 
        /// "Undo" trigger executes.
        /// </summary>
        public SpatialGestureEvent OnTab = new SpatialGestureEvent();
        public SpatialGestureEvent OnAltTab = new SpatialGestureEvent();
        public SpatialGestureEvent OnAltShiftTab = new SpatialGestureEvent();
        public SpatialGestureEvent OnEnter = new SpatialGestureEvent();
        public SpatialGestureEvent OnDelete = new SpatialGestureEvent();
        public SpatialGestureEvent OnBackspace = new SpatialGestureEvent();
        /// <summary>
        /// Upon execution, provides the number of characters in the word that was just removed from output
        /// </summary>
        public SpatialGestureEvent<int> OnBackWord = new SpatialGestureEvent<int>();
        public SpatialGestureEvent OnUndo = new SpatialGestureEvent();
        public SpatialGestureEvent OnRedo = new SpatialGestureEvent();
        /// <summary>
        /// Upon execution, provides the arrow key direction (up/down/left/right) the caret just moved by one character or line,
        /// and provides the new caret index position in the currently focused input field (relevant to Unity SDK only)
        /// </summary>
        public SpatialGestureEvent<ArrowKey> OnCursorMove = new SpatialGestureEvent<ArrowKey>();

        /// <summary>
        /// Upon execution, provides the arrow key direction the caret just moved while highlighting a character or line,
        /// and provides the current highlighted string
        /// </summary>
        public SpatialGestureEvent<ArrowKey, string> OnCursorShiftMove = new SpatialGestureEvent<ArrowKey, string>();

        /// <summary>
        /// Upon execution, provides the arrow key direction the caret just moved to highlight a word, and provides the number of 
        /// characters in that word, and provides the currently highlighted string
        /// </summary>
        public SpatialGestureEvent<ArrowKey, int, string> OnCursorShiftWordMove = new SpatialGestureEvent<ArrowKey, int, string>();

        /// <summary>
        /// Upon execution, if true, shift was just toggled on, otherwise off
        /// </summary>
        public SpatialGestureEvent<bool> OnAlt = new SpatialGestureEvent<bool>();

        /// <summary>
        /// Upon execution, if true, shift was just toggled on, otherwise off
        /// </summary>
        public SpatialGestureEvent<bool> OnCtrl = new SpatialGestureEvent<bool>();


        /// <summary>
        /// Upon execution, if true, shift was just toggled on, otherwise off
        /// </summary>
        public SpatialGestureEvent<bool> OnShift = new SpatialGestureEvent<bool>();

        /// <summary>
        /// Upon execution, if true, caps lock was just toggled on, otherwise off. This event is triggered both
        /// when caps is left on, and when it is on for a single character.
        /// </summary>
        public SpatialGestureEvent<bool> OnCaps = new SpatialGestureEvent<bool>();

        public SpatialGestureEvent<string> OnKey = new SpatialGestureEvent<string>();
        #endregion Third-Party Integration events

        #endregion public vars
        #endregion vars

        #region init
        private void Awake()
        {
            windowsKeySim = new InputSimulator();
            CurrentOutputValue = "";

            RegisterAllEventsAndActions();

            //UnitTest_ReplaceAcrossLan();
        }
        #endregion init

        //private void Awake()
        //{
        //    if (GetComponent<Text>().enabled)
        //    {
        //        // Used as a debugging console - if needed, toggle it on in Inspector
        //        Text output = GetComponent<Text>();
        //        RegisterOutput(ref output);
        //    }
        //}

        //private void Start()
        //{
        //    StartCoroutine(UNITTEST_SendToAllOutputs());
        //}

        #region loops
        private void Update()
        {
            #region unit tests
            //if (unitTest_notepadText)
            //{
            //    unitTest_notepadText = false;
            //    this.WaitS(5, () => {
            //        string test = notePadTextRetriever.GetControlText();
            //    });
            //}
            #endregion unit tests


            //_targetDirection = _motionRoot.transform.position - HeadMarker.Instance.transform.position;
            //_lookConfidence = Vector3.Dot(HeadMarker.Instance.transform.forward, _targetDirection.normalized);

            //if (_lookConfidence < 0)
            //    _isMoving = true;

            //if (_isMoving)
            //{
            //    _horizontalForward = -1 * Vector3.Cross(Vector3.up, Vector3.Cross(Vector3.up, HeadMarker.Instance.transform.forward));
            //    _targetPosition = HeadMarker.Instance.transform.position + _horizontalForward * _cameraDepthDistance;
            //    _motionRoot.transform.position = Vector3.Lerp(_motionRoot.transform.position, _targetPosition, _rootMotionDampeningFactor);
            //    if (_lookConfidence > 0.95f)
            //        _isMoving = false;
            //}
            //_targetRotation = Quaternion.LookRotation(_targetDirection);
            //_motionRoot.transform.rotation = Quaternion.Lerp(_motionRoot.transform.rotation, _targetRotation, _rootMotionDampeningFactor);
        }

        #endregion loops

        #region event handlers
        private void RegisterAllEventsAndActions()
        {
            this.Wait(()=>Lexting.input.Ready(), 
                      ()=>this.Subscribe(Lexting.input.OnOppositeMiddleToPalmTouch, (right)=>BackWordGesture()));
        }

        /// <summary>
        /// Connect a receiver text display to the zoning text output source
        /// </summary>
        public void RegisterOutput(ref Text _display)
        {
            displays?.Add(_display);
        }

        /// <summary>
        /// Disconnect a registered text display from the zoning text output source
        /// </summary>
        public void UnregisterOutput(ref Text _display)
        {
            if (displays.Contains(_display))
                displays.Remove(_display);
        }

        /// <summary>
        /// Connect a receiver text input field to the zoning text output source
        /// </summary>
        public void RegisterOutput(ref InputField _field)
        {
            fields?.Add(_field);
            EventSystem.current.SetSelectedGameObject(_field.gameObject, null);
            _field.OnPointerClick(new PointerEventData(EventSystem.current));
            _field.ActivateInputField();
            _field.MoveTextEnd(false);
            _field.lineType = InputField.LineType.MultiLineNewline;
        }

        /// <summary>
        /// Disconnect a registered text input field from the zoning text output source
        /// </summary>
        public void UnregisterOutput(ref InputField _field)
        {
            if (fields.Contains(_field))
                fields.Remove(_field);
        }

        /// <summary>
        /// Disconnect all connected string vars from the text output source
        /// </summary>
        public void UnregisterAll()
        {
            displays?.Clear();
            fields?.Clear();
        }
        #endregion event handlers

        #region helper methods
        public void UpdateOutputs(string newOutput)
        {
            UpdateOutputsWithoutServerRefresh(newOutput);

            // don't refresh when streaming until final word submission
            //if (Lexting.inputManager.isDesktop)
            //    LextingStreamClient._.Instance.AppendServerRpc(newOutput);
        }

        public void UpdateOutputsWithoutServerRefresh(string newOutput)
        {
            RefreshCapsState(ref newOutput);
            CurrentOutputValue = newOutput;
            UpdateAllReceivers();
        }

        public void ReplaceOutput(int numCharsToRemove, string newCharsToAppend, string currentWord)
        {
            RefreshCapsState(ref newCharsToAppend);

            SubtractWord(false);
            CurrentOutputValue += prefix + newCharsToAppend;
            UpdateAllReceivers();

            //if (Lexting.inputManager.isDesktop)
            //    LextingStreamClient._.Instance.AppendServerRpc(newCharsToAppend);
        }

        /// <summary>
        /// Deletes an optional number of characters from the end of CurrentOutputValue
        /// </summary>
        /// <param name="charsToRemove">the number of characters to remove from CurrentOutputValue</param>
        public void RemoveOutput(int charsToRemove)
        {
            if (CurrentOutputValue.Length < charsToRemove)
            {
                CurrentOutputValue = "";
                return;
            }
            else CurrentOutputValue = CurrentOutputValue.Substring(0, CurrentOutputValue.Length - 1);
            UpdateAllReceivers();

            // don't refresh when streaming until final word submission
            //if (Lexting.inputManager.isDesktop)
            //    LextingStreamClient._.Instance.RemoveServerRpc(charsToRemove);
        }

        /// <summary>
        /// DEPRECATED
        /// </summary>
        public void ClearPrefix()
        {
            prefix = "";
        }

        /// <summary>
        /// Appends a new string to the end of CurrentOutputManager
        /// </summary>
        /// <param name="newOutputAddon">new string to append</param>
        /// <param name="first">if true, the first letter of the current about-to-be-assembled word 
        /// is the first char in newOutputAddon (needed to properly handle capsState)</param>
        public void AppendOutputs(string newOutputAddon, bool first)
        {
            if (first || capsState == CapsState.CapsAll)
                RefreshCapsState(ref newOutputAddon);
            
            CurrentOutputValue += newOutputAddon;
            UpdateAllReceivers();
            if (isSendingKeysToWindows && !string.IsNullOrEmpty(newOutputAddon))
                windowsKeySim.Keyboard.TextEntry(newOutputAddon);

            if (newOutputAddon.Contains(" "))
                prefix = "";
            //UnityEngine.Debug.Log($"XXXXX AppendOutputs newOutputAddon '{newOutputAddon}' prefix {prefix}");

            //if (Lexting.inputManager.isDesktop)
            //    LextingStreamClient._.Instance.AppendServerRpc(newOutputAddon);
        }

        public void SendArrow(ArrowKey key)
        {
            foreach (InputField field in fields)
            {
                if (field.IsActive())
                {
                    
                }
            }
        }

        private void UpdateAllReceivers()
        {
            foreach (Text display in displays)
                display.text = CurrentOutputValue;
            foreach (InputField field in fields)
                field.text = CurrentOutputValue;
            //UnityEngine.Debug.Log("CubeOutputManage just updated " + displays.Count + " text label(s) and " + fields.Count + "input fields.");
        }

        /// <summary>
        /// DEPRECATED
        /// </summary>
        private string _singleLetterAddon = "";
        public void ConfirmSingleLetterEntry(bool right)
        {
            //_singleLetterAddon = TwoZoneTrigger.currentHighlightedLetter(right);
            //if (_singleLetterAddon != " ")
            //    prefix += _singleLetterAddon;
        }

        /// <summary>
        /// Capitalizes either the current pending word or the first letter of the current pending word based upon current caps state,
        /// then refreshes caps state if necessary
        /// </summary>
        public void RefreshCapsState(ref string currentPendingOutput)
        {
            if (capsState != CapsState.lower)
            {
                if (capsState == CapsState.CapsOne)
                {
                    if (!string.IsNullOrEmpty(currentPendingOutput))
                        currentPendingOutput = currentPendingOutput.FirstUpper();
                    // if user input single letter or 2 letters w/ 1 hand, no need to keep CapsOne on for potential assembly refresh
                    if (!Lexting.assembler.isWordAssemblyActive)
                    {
                        this.WaitF(1,()=>this.Wait(()=>LextingController.dragOver, () => {
                            capsState = CapsState.lower;
                            OnCaps?.I(false);
                        }));
                    }
                }
                else currentPendingOutput = currentPendingOutput.ToUpper();
            }
        }
        #endregion helper methods

        #region Hotkey functions
        public void BackSpace()
        {
            //ReplaceOutput(1, "", "");

            if (isSendingKeysToWindows)
                windowsKeySim.Keyboard.KeyPress(VirtualKeyCode.BACK);
            else RemoveOutput(1);
        }

        public void BackWord()
        {
            this.TLock(ref _timeSinceLastBackWord, _backWordMinRepeatInterval, () => {
                Lexting.audio.Play("BackspaceClick");
                SubtractWord(true);
            });
        }

        public void BackWordGesture()
        {
            this.TLock(ref _timeSinceLastBackWord, _backWordMinRepeatInterval, () => {
                Lexting.audio.Play("BackspaceClick");
                SubtractWord(true);
                //if (Lexting.inputManager.isDesktop)
                //    LextingStreamClient._.Instance.BackWordServerRpc();
            });
        }

        public void SubtractWord(bool withRefresh)
        {
            if (!CurrentOutputValue.Contains(" ") && !CurrentOutputValue.Contains("\n"))
                CurrentOutputValue = "";
            else
            {
                if (CurrentOutputValue.Substring(CurrentOutputValue.Length - 1, 1).Equals(" "))
                    CurrentOutputValue = CurrentOutputValue.Substring(0, CurrentOutputValue.Length - 1);
                else if (CurrentOutputValue.Substring(CurrentOutputValue.Length - 1, 1).Equals("\n"))
                    CurrentOutputValue = CurrentOutputValue.Substring(0, CurrentOutputValue.Length - 2);
                CurrentOutputValue = CurrentOutputValue.Substring(0, CurrentOutputValue.LastIndexOfAny(new char[] { ' ', '\n' }) + 1);
            }

            if (withRefresh)
                UpdateOutputsWithoutServerRefresh(CurrentOutputValue);
        }

        public void Delete()
        {
            if (isSendingKeysToWindows)
            {
                // invoke DELETE key
                
            }
            else if (fields.Count > 0 && fields[0].caretPosition < CurrentOutputValue.Length)
            {
                _caretPositionCache = fields[0].caretPosition;
                _numCharsAfterDeletionIndex = CurrentOutputValue.Length - fields[0].caretPosition;
                _newSuffix = CurrentOutputValue.Substring(fields[0].caretPosition, _numCharsAfterDeletionIndex);
                ReplaceOutput(_numCharsAfterDeletionIndex, _newSuffix.Substring(1,_newSuffix.Length-1), _newSuffix);
                foreach (var field in fields)
                    field.caretPosition = _caretPositionCache;
            }
        }

        public void Enter()
        {
            if (isSendingKeysToWindows)
            {
                windowsKeySim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.RETURN);
            }
            else if (fields.Count > 0 || displays.Count > 0)
                AppendOutputs("\n",false);
        }

        public void Undo()
        {
            if (isSendingKeysToWindows)
            {
                windowsKeySim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_Z);
            }
        }

        public void Redo()
        {
            if (isSendingKeysToWindows)
            {
                windowsKeySim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_Y);
            }
        }

        public void Tab()
        {
            if (isSendingKeysToWindows)
            {
                windowsKeySim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.TAB);
            }   
        }

        public void ToggleAlt()
        {
            alt = !alt;
            ToggleKeyHandler.Instance.ToggleVisibility(ToggleKeyHandler.ToggleKeyType.alt, alt);
            OnAlt?.I(alt);
        }

        public void ToggleShift()
        {
            shift = !shift;
            ToggleKeyHandler.Instance.ToggleVisibility(ToggleKeyHandler.ToggleKeyType.shift, shift);
            OnShift?.I(shift);
            // Shift-only layer: sticky caps until shift is toggled off (CapsAll path in AppendOutputs / RefreshCapsState).
            if (!alt && !ctrl)
            {
                if (shift)
                {
                    capsState = CapsState.CapsAll;
                    OnCaps?.I(true);
                }
                else if (capsState == CapsState.CapsAll)
                {
                    capsState = CapsState.lower;
                    OnCaps?.I(false);
                }
            }
        }

        public void ToggleCtrl()
        {
            ctrl = !ctrl;
            ToggleKeyHandler.Instance.ToggleVisibility(ToggleKeyHandler.ToggleKeyType.ctrl, ctrl);
            OnShift?.I(ctrl);
        }

        public void AltTab()
        {

        }

        public void AltShiftTab()
        {

        }

        public void CapitalizeNextLetter()
        {
            capsState = CapsState.CapsOne;
            OnCaps?.I(true);
        }

        public void ToggleCaps()
        {
            if (isSendingKeysToWindows)
            {
                windowsKeySim.Keyboard.KeyPress(VirtualKeyCode.CAPITAL);
            }
            else 
            {
                if (capsState != CapsState.CapsAll)
                    capsState = CapsState.CapsAll;
                else capsState = CapsState.lower;
            }
        }

        public void Escape()
        {
            if (isSendingKeysToWindows)
            {
                windowsKeySim.Keyboard.KeyPress(VirtualKeyCode.ESCAPE);
            }
        }

        public void Copy()
        {
            if (isSendingKeysToWindows)
            {
                windowsKeySim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
            }
        }

        public void Paste()
        {
            if (isSendingKeysToWindows)
            {
                windowsKeySim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
            }
        }

        public void Cut()
        {
            if (isSendingKeysToWindows)
            {
                windowsKeySim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_X);
            }
        }

        public void ToggleBold()
        {
            if (isSendingKeysToWindows)
            {
                windowsKeySim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_B);
            }
        }

        public void ToggleItalics()
        {
            if (isSendingKeysToWindows)
            {
                windowsKeySim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
            }
        }

        public void ToggleUnderline()
        {
            if (isSendingKeysToWindows)
            {
                windowsKeySim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_U);
            }
        }

        public void MoveCursor()
        {
            if (isSendingKeysToWindows)
            { 
                // map hand X-Y-relative-to-UI to screen-X-Y
                // At X distance threshold from hand, tick caret one char in that direction
                // at Y distance threshold from hand, tick caret up or down one line
            }
        }

        public void DragCursor()
        {
            if (isSendingKeysToWindows)
            {
                // map hand X-Y-relative-to-UI to screen-X-Y
                // At X distance threshold from hand, tick caret one char in that direction w/ SHIFT key down to select that char
                // At Y distance threshold from hand, tick caret up or down one line w/ SHIFT key down to select that line
            }
        }

        public void DragWordHighlight()
        {
            if (isSendingKeysToWindows)
            {
                // map hand X-Y-relative-to-UI to screen-X-Y
                // At X distance threshold from hand, tick caret one word (space-delimited) in that direction w/ SHIFT key down to select that char
                // At Y distance threshold from hand, tick caret up or down one line w/ SHIFT key down to select that line
            }
        }

        public void SelectAll()
        {
            if (isSendingKeysToWindows)
            {
                windowsKeySim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_A);
            }
        }
        #endregion Hotkey functions

        #region unit tests
        public void UNITTEST_DeletionCommand()
        {
            CurrentOutputValue = "This is a test.";
            UpdateAllReceivers();
            if (fields.Count > 0)
            {
                fields[0].caretPosition = 3;
            }
        }

        /// <summary>
        /// to repro: try to type the words "the quick "
        /// current result: 
        ///     When you move rHand from q to k (on its way toward c), output should become "the quk".
        ///     Instead, it becomes "thquk" by deleting too many preceding chars on the server-side only.
        /// This method simulates that execution pattern while debuggers can run on both client & server
        /// in two simultaneous instances of Unity. This method executes on client. Server is standing by.
        /// </summary>
        public void UnitTest_ReplaceAcrossLan()
        {
            this.WaitS(10, () => {
                ReplaceOutput(2, "quk", "qu");
            });
        }
        #endregion unit tests
    }
}


