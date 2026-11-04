using SimpleJSON;
using SpatialGestureText.Data;
using SpatialGestureText.Debugging;
using SpatialGestureText.EnglishDictionaries;
using SpatialGestureText.Hotkeys;
using SpatialGestureText.Input;
using SpatialGestureText.Interfaces;
using SpatialGestureText.TwoZoneCube;
using SpatialGestureText.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;
using System.IO;

using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SpatialGestureText.LetterCube
{
    /// <summary>
    /// Controls the construction of a word that will be submitted to output when a drag gesture is complete
    /// </summary>
    public class CubeWordAssemblyController : Singleton<CubeWordAssemblyController>
    {
        #region private vars
        [SerializeField] private CubeDragLineVisualizer _rightDragHandler, _leftDragHandler;
        private IController _leadingController;

        /// <summary>
        /// Actions that prevent race conditions during the addition of letters to the TwoZoneGrabState structure
        /// in a variety of sequences from user's hands
        /// </summary>
        private UnityAction _initWordAssembly, _addLetter;

        /// <summary>
        /// index iterators, always reset upon use, cached to cut down on mem garbage 
        /// </summary>
        private int _i, _j, _k;

        private string _previousComp = "";

        /// <summary>
        /// a cached reference of the word provided by the word list controller during final submission
        /// </summary>
        private string _finalWord;
        #endregion private vars

        #region public vars

        #region data model vars
        public List<JSONNode> tempWordList = new List<JSONNode>();
        public List<JSONNode> wordList = new List<JSONNode>();
        #endregion data model vars

        #region state vars
        public bool UnitTestWordListConstruction;
        public bool isWordAssemblyActive;
        public bool isWordScrollActive;

        // used to detect simultaneous release of both hands during word submission
        public float timeSinceListDisplayBegan;

        public SpatialGestureEvent OnWordAssemblyBecameInactive = new SpatialGestureEvent();


        #endregion state vars
        #endregion public vars

        #region init
        private void Awake()
        {

            TwoZoneGrabState.Clear();
            LextingDictionary.InitializeEnglishWordDatabase();
            RegisterAllEventsAndActions();

            //LextingDictionary.UnitTest_PageableRecordGetters();
        }
        #endregion init

        #region loops and timers
        //private void Update()
        //{
        //    #region Unit Tests
        //    if (UnitTestWordListConstruction)
        //    {
        //        UnitTestWordListConstruction = false;
        //        ConstructWordList();
        //    }
        //    #endregion Unit Tests
        //}
        #endregion loops and timers

        #region event handlers
        private void RegisterAllEventsAndActions()
        {
            this.Wait(() => LextingController.Instance.IsReady(), () => {
                _leadingController = LextingController.Instance;
                this.Subscribe(_rightDragHandler.LineTracingActivated, OnRightLineTracingActivated);
                this.Subscribe(_leftDragHandler.LineTracingActivated, OnLeftLineTracingActivated);
                this.Subscribe(_leadingController.OutputBegan, OnOutputBegan);
                this.Subscribe(Lexting.input.OnHandTrackingToggled, (on) => {});
                this.Subscribe(Lexting.input.OnIndexPointing, OnIndexPointing);
            });
        }

        private void OnRightLineTracingActivated(bool on)
        {
            //Debug.Log(string.Format("OnRightLineTracingActivated - " +
            //    "_leftDragHandler.isLineTracingActive? {0} " +
            //    "isWordAssemblyActive? {1}",
            //    _leftDragHandler.isLineTracingActive,
            //    isWordAssemblyActive));

            if (on)
            {
                LextingController.Instance.currentRightTrigger.SetStartingIndex(true);
                if (_leftDragHandler.isLineTracingActive && !isWordAssemblyActive /*&& !Lexting.hotkeys.isHotkeyAvailable*/)
                    isWordAssemblyActive = true;
            }
        }

        private void OnLeftLineTracingActivated(bool on)
        {
            //Debug.Log(string.Format("OnLeftLineTracingActivated - " +
            //    "_righttDragHandler.isLineTracingActive? {0} " +
            //    "isWordAssemblyActive? {1}",
            //    _rightDragHandler.isLineTracingActive,
            //    isWordAssemblyActive));

            if (on)
            {
                LextingController.Instance.currentRightTrigger.SetStartingIndex(false);
                if (_rightDragHandler.isLineTracingActive && !isWordAssemblyActive)
                    isWordAssemblyActive = true;
            }
        }

        /// <summary>
        /// Initializes the assembler when the first letter of an assembly has just been submitted.
        /// </summary>
        /// <param name="right">if true, the right hand submitted the first letter, otherwise left did.</param>
        /// <param name="outputLetter">the first letter of the new assembly</param>
        private void OnOutputBegan(bool right, string outputLetter)
        {

            if (!Lexting.input.inputActive) return;
            _initWordAssembly = () => {
                _previousComp = TwoZoneGrabState.comp;

                TwoZoneGrabState.SetCurrent(right, true, outputLetter);
                if (!string.IsNullOrEmpty(TwoZoneGrabState.comp))
                    ConstructWordList();

                if (TwoZoneGrabState.comp.Length == 3)
                {
                    _previousComp = TwoZoneGrabState.compSingle(!right) + outputLetter;
                    RefreshInProgressWordOutput(right);
                }
            };

            if (right)
                this.Wait(() => _rightDragHandler.isLineTracingActive, _initWordAssembly);
            else this.Wait(() => _leftDragHandler.isLineTracingActive, _initWordAssembly);
        }

        /// <summary>
        /// Drag was active in both hands and has just been released by the first hand.
        /// Toggle drag capability off and turn list scroll on.
        /// </summary>
        public void HandleFourLetterSubmission(bool right)
        {
            isWordScrollActive = true;
            LextingWordListController.Instance.ShowList(true);
            _leadingController.ToggleCharDisplay(false);
            timeSinceListDisplayBegan = Time.time;
            _leftDragHandler.ResetStateToIdle(true);
            _rightDragHandler.ResetStateToIdle(true);
        }

        /// <summary>
        /// Refreshes the entire current word being assembled.
        /// </summary>
        /// <param name="final">if true, this is the final submission of a change for this word</param>
        private void UpdateFullWord(bool final)
        {
            
            TryUpdateFastWord();
            TryUpdateRegularWord(final);
        }

        /// <summary>
        /// User may have selected an empty slot in the spatial list or released both hands simultaneously.
        /// in either case, they want the default unconjugated first found word. This method provides it.
        /// </summary>
        private void TryUpdateFastWord()
        {
            if ((Lexting.finalizer.currentWord == null
                    || string.IsNullOrEmpty(Lexting.finalizer.currentWord.text)
                    || DidReleaseBothSimultaneously()) && Lexting.finalizer.wordList.Count > 0)
            {
                Lexting.printer.SubtractWord(false);
                Lexting.printer.AppendOutputs(TwoZoneWordListController.Instance.GetDefaultFirstWord(), true);
            }
        }

        /// <summary>
        /// Refreshes output during word assembly based upon the latest change in user's hand position or orientation.
        /// </summary>
        /// <param name="final">if true, this is the final submission of a change for this word.</param>
        private void TryUpdateRegularWord(bool final)
        {
            if (Lexting.finalizer.isSpatialListActive
                    && Lexting.finalizer.currentWordObject.wordNode != null)
            {
                Lexting.printer.SubtractWord(false);
                if (final)
                {
                    _finalWord = TwoZoneWordListController.Instance.GetConjugatedPunctuatedWord(final);
                    Lexting.printer.AppendOutputs(_finalWord, true);

                    if (Lexting.inputManager.isDesktop)
                    {
                        Lexting.printer.RefreshCapsState(ref _finalWord);
                        //LextingStreamClient._.Instance.AppendServerRpc(_finalWord);
                    }
                }
                else Lexting.printer.AppendOutputs(TwoZoneWordListController.Instance.GetConjugatedPunctuatedWord(final), true);
            }
        }

        /// <summary>
        /// True if user released both hands from word mode simultaneously
        /// </summary>
        private bool DidReleaseBothSimultaneously()
        {
            return Lexting.rhState != HandGestureState.Insert && Lexting.lhState != HandGestureState.Insert &&
                    this.TLock(timeSinceListDisplayBegan, 0.1f); 
        }

        /// <summary>
        /// Drag was active in both hands but it has just been released by the last hand.
        /// Output the currently selected word and turn off the word assembler.
        /// </summary>
        public void HandleFullWordSubmission()
        {
            //string logOutput = "HandleFullWordSubmission - about to output full word - " +
            //    " current output: " + LextingOutputManager.Instance.CurrentOutputValue +
            //    " currentScrollIndex: " + _currentScrollIndex +
            //    " current word: " + currentWord +
            //    " wordList: ";
            //if (wordList.Count > 0)
            //    for (_i = 0; _i < wordList.Count; _i++)
            //        logOutput += " " + _i + ":" + wordList[i]["default"].Value;
            //Debug.Log(((logOutput.Length > 0) ? logOutput : logOutput + "empty"));

           // Debug.Log("FullWordSubmission executed.");

            UpdateFullWord(true);
            DeactivateWordAssembler();
            _leadingController.FlashCubeHighlight(true);
        }

        private void OnIndexPointing(bool right)
        {
            if (isWordAssemblyActive)
            {
                Lexting.printer.BackWord();
                DeactivateWordAssembler();
            }
        }
        #endregion event handlers

        #region helper methods
        public void DeactivateWordAssembler()
        {
            _leadingController.ResetAllTriggers();
            FinishDeactivation();
        }

        public void DeactivateWordAssembler(bool right)
        {
            _leadingController.ResetAllTriggers(right);
            FinishDeactivation();
        }

        private void FinishDeactivation()
        {
            TwoZoneGrabState.Clear();
            tempWordList.Clear();
            isWordAssemblyActive = false;
            isWordScrollActive = false;
            Lexting.finalizer.ShowList(false);
            if (_leadingController != null)
                _leadingController.ToggleCharDisplay(true);
            timeSinceListDisplayBegan = 0;
            OnWordAssemblyBecameInactive?.I();
        }

        /// <summary>
        /// As soon as drag is confirmed begun in both hands, we add all the possible words that start with
        /// the first two entry letters to a new list and display the top entry as potential output for the user.
        /// Later, as the user types, the list will contract to fewer options as this method gets
        /// called each time either hand creates a change in the search string. Each time the search string
        /// gest longer, this method filters down to fewer total results.
        /// </summary>
        private void ConstructWordList()
        {
            //Debug.Log("word list construction primed - currentWord: " + currentWord);
            //Debug.Log("current word length is " + currentWord.Length + " and isWordAssemblyActive is " + isWordAssemblyActive);
            if (!isWordAssemblyActive) return;

            if (TwoZoneGrabState.comp.Length >= 2)
            {
                if (RefreshWordList(TwoZoneGrabState.comp))
                {
                    string debugOutput = "";
                    foreach (JSONNode wordNode in tempWordList)
                        debugOutput += wordNode["default"].Value + ", ";
                    //Debug.Log("ConstructWordList - got word list: " + debugOutput);
                    //Debug.Log("ConstructWordList - first match for first four letters " + currentWord + " was found: " + tempWordList[0]["default"].Value);
                    Lexting.finalizer.UpdateWordList(tempWordList);
                }
                else
                {
                    //Debug.Log("ConstructWordList - found no words starting with " + currentWord);
                    //string altWord = (TwoZoneGrabState.comp.Length >= 4)
                    //    ? TwoZoneGrabState.comp.Substring(0, currentWord.Length - 2) + currentWord.Substring(TwoZoneGrabState.comp.Length - 1, 1) + TwoZoneGrabState.comp.Substring(TwoZoneGrabState.comp.Length - 2, 1)
                    //    : TwoZoneGrabState.comp;
                    TwoZoneGrabState.FlipSeconds();
                    if (TwoZoneGrabState.comp.Length >= 4 && RefreshWordList(TwoZoneGrabState.comp))
                    {
                        // user error confirmed - flip the 3rd and 4th digits in the display
                        Lexting.printer.ReplaceOutput(2, TwoZoneGrabState.comp.Substring(2, 2), TwoZoneGrabState.comp);

                        //string debugOutput = "";
                        //foreach (JSONNode wordNode in tempWordList)
                        //    debugOutput += wordNode["default"].Value + ", ";
                        //Debug.Log("ConstructWordList - got alt word list: " + debugOutput);
                        Lexting.finalizer.UpdateWordList(tempWordList);
                    }
                    else TwoZoneGrabState.FlipSeconds();
                    //else Debug.Log("ConstructWordList - found no matches for currentWord:" + currentWord);
                }
            }
        }

        /// <summary>
        /// Get all words from the dictionary that match the provided 2-, 3-, or 4-digit search string.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private bool RefreshWordList(string word)
        {
            tempWordList.Clear();
            int limit = 8;

            if (word.Length == 2)
                LextingDictionary.GetTwoLetterWords(ref tempWordList, word, limit);

            if (word.Length <= 3)
                LextingDictionary.DrillDownGet(ref tempWordList, word, LextingDictionary.threeLetterDictionary, limit);

            LextingDictionary.DrillDownGet(ref tempWordList, word, LextingDictionary.longerWordDictionary, limit);

            return tempWordList.Count > 0;
        }


        /// <summary>
        /// While word assembly is active, this method updates the latter two of the
        /// first four letters in the current word as each cursor drags into them.
        /// For example, User wants to spell "INTRODUCE". Drag begins w/ right hand on I 
        /// and left hand on N. Right drags into T. Left drags into R.
        /// The triggers for T and R each call this method respectively
        /// to update output. If User drags backward to return to I or N, previous submission 
        /// from that hand (T or R) is subtracted from output via this method too.
        /// This method also handles adding/updating the 2nd letter of single-hand drag input.
        /// </summary>
        public void AddLetter(bool right, string newLetter)
        {
            if (newLetter.Equals(" ")) return;

            _addLetter = () => {

                _previousComp = TwoZoneGrabState.comp;
                if (TwoZoneGrabState.returned(right, newLetter))
                    TwoZoneGrabState.SetCurrent(right, false, "");
                else {
                    if (TwoZoneGrabState.compSingleReady(right))
                        TwoZoneGrabState.currentReloaded = true;
                    TwoZoneGrabState.SetCurrent(right, false, newLetter);
                }
                RefreshInProgressWordOutput(right);
            };

            // Schedule 2nd letter for this hand to be added only after first letter has been initialized by OnOutputBegan:
            this.Wait(() => TwoZoneGrabState.firstReady(right), _addLetter);
        }


        
        public void SubtractLetter(bool right)
        {
            //LogOutputReceiver.Instance.Log($"Assembler about to subtract a letter in {((right)?"right":"left")} hand.");
            _previousComp = TwoZoneGrabState.comp;
            if (!string.IsNullOrEmpty(TwoZoneGrabState.second(right)))
                TwoZoneGrabState.ReturnSecond(right);
            RefreshInProgressWordOutput(right);
        }

        /// <summary>
        /// While hands continue dragging during word assembly, 3rd and 4th input characters can change
        /// or be removed, necessitating a refresh of the latest displayed result. This method causes that refresh.
        /// </summary>
        private void RefreshInProgressWordOutput(bool right)
        {
            //LogOutputReceiver.Instance.Log($"Assembler RefreshInProgressWordOuput about to process {((right) ? "right" : "left")} hand " +
            //    $"for current comp {TwoZoneGrabState.first(right) + TwoZoneGrabState.second(right)}");
            ConstructWordList();
            // 3 use cases: 1) a 2-hand unique comp is ready, or 2) a hand returned to its start, or 3) a 1-hand comp is ready,  
            if (TwoZoneGrabState.compReady && !TwoZoneGrabState.comp.Equals(_previousComp) && !isWordScrollActive)
                RefreshTwoHandComp();  // use case 1
            else if (TwoZoneGrabState.returnedThisFrame && !TwoZoneGrabState.compSingleReadySolo(right))
                RefreshHandReturnToStart();     // use case 2
            else if (TwoZoneGrabState.compSingleReadySolo(right) || TwoZoneGrabState.returnedThisFrame)
            {
                if (TwoZoneGrabState.currentReloaded)
                    RefreshSingleHandReplacesSecondLetter(right);   // use case 3a
                else RefreshSingleHandAddsSecondLetter(right);      // use case 3b
            }
        }

        private void RefreshSingleHandReplacesSecondLetter(bool right)
        {
            //if (Lexting.printer.CurrentOutputValue.Length > 0)
            //    LogOutputReceiver.Instance.Log($"Assembler RefreshSingleHandReplacesSecondLetter about to replace " +
            //        $"{Lexting.printer.CurrentOutputValue.Substring(Lexting.printer.CurrentOutputValue.Length - 1)} " +
            //        $"with {TwoZoneGrabState.compSingle(right).Substring(1)}");
            //else LogOutputReceiver.Instance.Log("Assembler RefreshSingleHandReplacesSecondLetter failed to replace output: output is empty");
            Lexting.printer.BackSpace();
            TwoZoneGrabState.currentReloaded = false;
            Lexting.printer.AppendOutputs(TwoZoneGrabState.compSingle(right).Substring(1), false);
            TwoZoneGrabState.OnGrabStateChanged?.I();
        }

        private void RefreshHandReturnToStart()
        {
            //if (Lexting.printer.CurrentOutputValue.Length > 0)
            //    LogOutputReceiver.Instance.Log($"Assembler RefreshHandReturnToStart about to remove " +
            //        $"{Lexting.printer.CurrentOutputValue.Substring(Lexting.printer.CurrentOutputValue.Length - 1)}");
            //else LogOutputReceiver.Instance.Log("Assembler RefreshSingleHandReplacesSecondLetter failed to replace output: output is empty");
            Lexting.printer.BackSpace();
            TwoZoneGrabState.returnedThisFrame = false;
            TwoZoneGrabState.OnGrabStateChanged?.I();
        }

        private void RefreshSingleHandAddsSecondLetter(bool right)
        {
            //LogOutputReceiver.Instance.Log($"Assembler Refreshsinglehandaddssecondletter about to add " +
            //        $"{TwoZoneGrabState.compSingle(right).Substring(1)}");
            Lexting.printer.AppendOutputs(TwoZoneGrabState.compSingle(right).Substring(1), false);
            TwoZoneGrabState.OnGrabStateChanged?.I();
        }

        private void RefreshTwoHandComp()
        {
            //LogOutputReceiver.Instance.Log($"Assembler RefreshTwoHandComp about to change " +
            //        $"{_previousComp} to {TwoZoneGrabState.comp}");
            Lexting.printer.ReplaceOutput(_previousComp.Length, TwoZoneGrabState.comp, _previousComp);
        }
        #endregion helper methods

        #region Unit Tests

        #endregion Unit Tests
    }
}
