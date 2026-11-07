using SimpleJSON;
using SpatialGestureText.Data;
using SpatialGestureText.Input;
using SpatialGestureText.Interfaces;
using SpatialGestureText.LetterCube;
using SpatialGestureText.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace SpatialGestureText.TwoZoneCube
{
    /// <summary>
    /// This class is also known as Lexting.finalizer
    /// Manages display of alternate info in the UI grid during FINALIZATION MODE, meaning the user
    /// has indicated they have finished choosing letters and are now ready to choose the
    /// final word, punctuation, and conjugation. 
    /// 
    /// During FINALIZATION MODE:
    /// * Word is handled here on the left-hand side of the UI.
    /// * Punctuation is handled here on the right-hand side of the UI. 
    /// * Conjugation is handled by the LextingConjugationState class, driven by the user's wrist orientation.
    /// </summary>
	public class TwoZoneWordListController : Singleton<TwoZoneWordListController>, IWordListController
	{
        #region vars
        #region public vars
        public List<WordMetaData> wordList { get; set; }

        public string currentPunctuation = "";

        /// <summary>
        /// These track the current paging position of a collection of words that all fall within the same
        /// conjugation state, e.g. "action", "activation", and "activist" are all singular noun forms of "active".
        /// When there are multiple, user needs to be able to page them by tapping the open hand to iterate each respectively.
        /// </summary>
        public int nounCycleIndex = 0, verbCycleIndex = 0, adjCycleIndex = 0;

        public Text currentWord { get; set; }
        public WordMetaData currentWordObject { get; set; }

        public bool isSpatialListActive { get; set; }

        public bool SPATIALLISTACTIVETESTER;

        public void Update()
        {
            if (SPATIALLISTACTIVETESTER)
            {
                isSpatialListActive = true;
                SPATIALLISTACTIVETESTER = false;
            }
        }

        /// <summary>
        /// needed for interface compatibility only
        /// </summary>
        public bool IsReady() => this.Ready();

        public SpatialGestureEvent<string> OnRefreshWord = new SpatialGestureEvent<string>();
        public SpatialGestureEvent<string> OnCompleteWordSubmitted = new SpatialGestureEvent<string>();

        /// <summary>
        /// if true, the current conjugation state has multiple words available for the current word object
        /// </summary>
        public bool currentHasMultipleRecords => (currentWordObject.wordNode == null) ? false 
            : currentConjugatedOptionalWordArray.Count > 0;

        /// <summary>
        /// if true, the current conjugation state has multiple words available for the current word object
        /// </summary>
        public bool nounHasMultipleRecords => (currentWordObject.wordNode == null) ? false
            : currentNounOptionalWordArray.Count > 0;


        #endregion public vars
        #region private vars

        /// <summary>
        /// the record at the current conjugation key of the currentWordObject, which may be a string or an array of strings
        /// </summary>
        public JSONNode currentNounOptionalWordArray => currentWordObject.wordNode["nounSingular"];

        /// <summary>
        /// the record at the current conjugation key of the currentWordObject, which may be a string or an array of strings
        /// </summary>
        public JSONNode currentConjugatedOptionalWordArray =>
            currentWordObject.wordNode[ConjugationStateToKey(Lexting.conjugator.conjugationState)];

        /// <summary>
        /// if 0, the record is a single string, not an array, but otherwise this returns the number of optional words in
        /// the current conjugated record. This will never return 1, due to the way SimpleJSON handles arrays.
        /// </summary>
        public int currentCycleRange => (_isPossessive) ? currentNounOptionalWordArray.Count :currentConjugatedOptionalWordArray.Count;

        private bool _isPossessive => Lexting.conjugator.conjugationState == ConjugationState.possessiveNoun;
        
        /// <summary>
        /// DEPRECATED - use the public version instead
        /// </summary>
        [SerializeField] private List<WordMetaData> _wordList;

        /// <summary>
        /// DEPRECATED - was used to support word assembler when it was handling conjugation
        /// </summary>
        private List<WordMetaData> unconjunctiveWords = new List<WordMetaData>();

        private GameObject _wordListUI;
        private GameObject _punctuationUI;

        private Text[] _wordListUILabels;

        private string[] _punctuation = new string[] { "? ", "! ", " ", "", ". ", ", ", ": ", "; ", "\" " };

        /// <summary>
        /// DEPRECATED - no longer any need to cycle to more than 8 filtered word results...
        /// if there are more than 8, the CubeWordAssemblyController should not let the UI
        /// enter FINALIZATION MODE until
        /// </summary>
        private int _pageIndex;

        private bool _hasConjugationChangedDuringThisFinalization;
        private bool _hasPunctuationChangedDuringThisFinalization;
        /// <summary>First punctuation zone index seen after Finalization opens; default SPACE until hover leaves this zone (QA-M3-051).</summary>
        private int _punctuationHoverBaselineIndex = -1;
        private IDisposable conjugationStateMonitorLoop;

        private float timeSinceLastCycleTap;
        private const float cycleTapThreshold = 0.2f;


        /// <summary>
        /// A cache of the current word that includes full conjugation and punctuation
        /// updated in real time w/ every change of rotation, translation, and release
        /// </summary>
        private string _fullyRefreshedWord;

        private bool _otherHandIsClosed(bool right) => (right) ? Lexting.lhState == HandGestureState.Insert : Lexting.rhState == HandGestureState.Insert;

        private bool _cycleTriggerConditions(bool right) => _cycleTriggerReady && _otherHandIsClosed(right) 
            && this.TLock(timeSinceLastCycleTap, cycleTapThreshold) && currentWordObject.wordNode != null;

        private bool _cycleTriggerReady =>
            (Lexting.lhState == HandGestureState.Insert && Lexting.rhState != HandGestureState.Insert) ||
            (Lexting.rhState == HandGestureState.Insert && Lexting.lhState != HandGestureState.Insert);
        #endregion private vars
        #endregion vars

        #region init
        private void Awake()
        {
            wordList = new List<WordMetaData>();

            if (wordList.Count > 0)
                currentWord = wordList[0].label;

            _wordListUI = transform.GetChild(0).gameObject;
            _wordListUILabels = _wordListUI.transform.GetComponentsInChildren<Text>();
            _punctuationUI = transform.GetChild(1).gameObject;

            RegisterAllEventsAndActions();
        }
        #endregion init

        #region event handlers
        private void RegisterAllEventsAndActions()
        {
            this.GetGestures(
                (right, current, previous) => 
                {
                    if (isSpatialListActive)
                    {
                        //if (SpatialGestureInput.Instance.currentLeftHandGestureState == HandGestureState.Insert &&
                        //    SpatialGestureInput.Instance.currentRightHandGestureState == HandGestureState.Insert)
                        //    timeSincePotentialCycleTapBegan = Time.time;

                        /// cycle through the array of available words of a given conjugation if more than one is available


                        

                        if (current == HandGestureState.Neutral && previous == HandGestureState.Insert 
                            && _cycleTriggerConditions(right))
                            CycleWord();

                    }
                },
                (right, current, previous) =>
                {
                    /// determine whether to use the default word or to conjugate
                    if (isSpatialListActive)
                        _hasConjugationChangedDuringThisFinalization = true;
                }
            );

            Lexting.conjugator.ObserveEveryValueChanged(stateClass => stateClass.conjugationState)
                .Subscribe(state => {
                    if (isSpatialListActive && currentWordObject.wordNode != null)
                    {
                        Lexting.printer.SubtractWord(false);
                        Lexting.printer.AppendOutputs(GetConjugatedPunctuatedWord(), true);
                    }
                });

            this.Subscribe(Lexting.input.OnIndexPointing, (right)=> {
                if (isSpatialListActive) ShowList(false);
            });
        }
        #endregion event handlers

        #region helper methods

        /// <summary>
        /// Toggles FINALIZATION MODE (filtered word list on the left, punctuation on the right)
        /// </summary>
        public void ShowList(bool on)
        {
            if ((!on && !isSpatialListActive) || (on && isSpatialListActive)) return;
            else if ((!on && isSpatialListActive) || (on && !isSpatialListActive))
            {
                _wordListUI.SetActive(on);
                _punctuationUI.SetActive(on);
                //if (!on && isSpatialListActive)
                //    currentWord.text = "";
                if (on && !isSpatialListActive && wordList.Count > 0)
                {
                    currentWordObject = wordList[0];
                    currentWord = currentWordObject.label;
                    currentPunctuation = " ";
                    _punctuationHoverBaselineIndex = -1;
                    var defaultWord = GetDefaultFirstWord();
                    Lexting.printer.SubtractWord(false);
                    Lexting.printer.AppendOutputs(defaultWord, true);
                    OnRefreshWord?.I(_fullyRefreshedWord);
                }
            }
            isSpatialListActive = on;
            if (!on) ResetState();

            if (!on)
                Debug.Log($"word list UI stuck? word list: {_wordListUI.activeSelf} punctuation list: {_punctuationUI.activeSelf}");

        }

        /// <summary>
        /// Refreshes UI display and output when user hovers into a new word list zone with the left hand
        /// </summary>
        /// <param name="index">the 1D index of the freshly hovered zone in the left-hand matrix</param>
        public void SelectWord(int index)
        {
            if (wordList.Count == 0)
            {
                Debug.LogWarning("The Lexting UI is not supposed to call this method if there are no current words to choose from. " +
                    "This may be evidence of a race condition between the finalizer and the rest of the Lexting interface" +
                    "during which the finalizer failed to disable itself on time.");
                return;
            }

            try {
                index = index > wordList.Count - 1 ? 0 : index;
                currentWordObject = wordList[index];
                currentWord = currentWordObject.label;
                Debug.Log($"SelectWord() - currentWord: {currentWord} currentWordObject: {currentWordObject.wordNode["default"]}");
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
            Lexting.printer.SubtractWord(false);
            Lexting.printer.AppendOutputs(
                (currentWord == null || string.IsNullOrEmpty(currentWord.text))
                    ? GetDefaultFirstWord() : GetConjugatedPunctuatedWord(), true);
        }

        /// <summary>
        /// Refreshes UI display and output when user hovers into a new punctuation zone w/ the right hand
        /// </summary>
        /// <param name="index">the 1D index of the freshly hovered zone in the right-hand matrix</param>
        public void SelectPunctuation(int index)
        {
            // Default SPACE from GetPunctuation() until the user moves off the zone they happened to
            // be over when Finalization opened (avoids same-frame / carry-over hover replacing default).
            if (!_hasPunctuationChangedDuringThisFinalization)
            {
                if (_punctuationHoverBaselineIndex < 0)
                {
                    _punctuationHoverBaselineIndex = index;
                    return;
                }
                if (index == _punctuationHoverBaselineIndex)
                    return;
                _hasPunctuationChangedDuringThisFinalization = true;
            }

            currentPunctuation = _punctuation[index];
            if (currentWordObject.wordNode != null)
            {
                Lexting.printer.SubtractWord(false);
                Lexting.printer.AppendOutputs(GetConjugatedPunctuatedWord(), true);
            }
        }

        /// <summary>
        /// Get the alphabetically first root (unconjugated) word from the current list of found words
        /// </summary>
        /// <returns></returns>
        public string GetDefaultFirstWord()
        {

            if (wordList.Count == 0)
                return "";
            else {
                _fullyRefreshedWord = wordList.Count > 0 ? wordList[0].wordNode["default"].Value : "";
                return _fullyRefreshedWord;
            }
        }

        public void UpdateListTransform(Transform newTransform)
        {
            // must keep this function for interface compliance, but transform updates are
            // not necessary with the two-zone cube
        }

        public void UpdateTransform()
        {
            transform.position =  Lexting.controllers.transform.position;
            transform.rotation = Lexting.controllers.transform.rotation;
        }

        /// <summary>
        /// Caches the current filtered results from user's first few letters, maintaining
        /// an association between each dictionary record and the UI element that will display it.
        /// </summary>
        /// <param name="list"></param>
        public void UpdateWordList(List<JSONNode> list)
        {
            if (list == null || list.Count == 0)
                Debug.Log("list is empty");
            wordList.Clear();
            for (int i = 0; i < 8; i++)
            {
                if (list.Count > i)
                {
                    _wordListUILabels[i].text = list[i]["default"];
                    wordList.Add(new WordMetaData(_wordListUILabels[i], list[i]));
                }
                else _wordListUILabels[i].text = "";
            }
        }

        #region Conjugation handlers
        /// <summary>
        /// Retrieves the currect word with conjugation and punctuation.
        /// </summary>
        public string GetConjugatedPunctuatedWord()
        {
            _fullyRefreshedWord = Get();
            OnRefreshWord?.I(_fullyRefreshedWord);
            return _fullyRefreshedWord;
        }

        /// <summary>
        /// Retrieves the currect word with conjugation and punctuation.
        /// </summary>
        /// <param name="final">if true, this is the final retrieval before word assembly deactivates</param>
        public string GetConjugatedPunctuatedWord(bool final)
        {
            _fullyRefreshedWord = Get();
            if (final)
            {
                // conditional single caps if punctuation calls for a new sentence to begin
                if (currentPunctuation.Contains(".") || currentPunctuation.Contains(",") ||
                    currentPunctuation.Contains("!") || currentPunctuation.Contains("?"))
                {
                    this.WaitF(1, () => Lexting.printer.CapitalizeNextLetter());
                }
            }
            OnRefreshWord?.I(_fullyRefreshedWord);
            OnCompleteWordSubmitted?.I(_fullyRefreshedWord);
            return _fullyRefreshedWord;
        }

        private string GetPunctuation()
        {
            if (_hasPunctuationChangedDuringThisFinalization)
                return currentPunctuation;
            else return " ";
        }

        /// <summary>
        /// A helper for the various overloads of the GetConjugatedPunctuatedWord method
        /// </summary>
        private string Get()
        {
            try
            {
                // If user hasn't changed wrist orientation since finalizaiton mode began,
                // they want the default word.
                if (!_hasConjugationChangedDuringThisFinalization)
                    return currentWordObject.wordNode["default"].Value + GetPunctuation();
                else
                {
                    // If user HAS changed wrist orientation at least once, they want a specific conjugation...
                    if (Lexting.conjugator.conjugationState == ConjugationState.possessiveNoun)
                        return GetNoun(Lexting.conjugator.conjugationState, currentWordObject.wordNode) + 
                            "'s" + GetPunctuation();
                    else if ((int)Lexting.conjugator.conjugationState <= 3)
                        return GetVerb(Lexting.conjugator.conjugationState, 
                            currentWordObject.wordNode) + GetPunctuation();
                    else if ((int)Lexting.conjugator.conjugationState <= 5)
                        return GetNoun(Lexting.conjugator.conjugationState, 
                            currentWordObject.wordNode) + GetPunctuation();
                    else if ((int)Lexting.conjugator.conjugationState > 5)
                        return GetAdjective(Lexting.conjugator.conjugationState, 
                            currentWordObject.wordNode) + GetPunctuation();
                    else return currentWordObject.wordNode["default"].Value + GetPunctuation();
                }
            }
            catch (NullReferenceException e)
            {
                Debug.Log(e.Message);
            }
            return "";
        }

        /// <summary>
        /// We often receive the conjugation state from Lexting.conjugator, and we need it converted into a SimpleJSON-ready string.
        /// </summary>
        private string ConjugationStateToKey(ConjugationState state)
        {
            switch (state)
            {
                case ConjugationState.firstSingularVerb: return "verbSingular";
                case ConjugationState.thirdSignularVerb: return "verbPlural";
                case ConjugationState.presentProgressiveVerb: return "verbParticiple";
                case ConjugationState.pastVerb: return "verbPast";
                case ConjugationState.singularNoun: return "nounSingular";
                case ConjugationState.pluralNoun: return "nounPlural";
                case ConjugationState.adjective: return "adj";
                case ConjugationState.adverb: return "adv";
                default: return "";
            }
        }

        private string GetVerb(ConjugationState state, JSONNode node)
        {
            return GetWord("verbSingular", verbCycleIndex, ConjugationState.firstSingularVerb, state, node);
        }

        private string GetNoun(ConjugationState state, JSONNode node)
        {
            return GetWord("nounSingular", nounCycleIndex, ConjugationState.singularNoun, state, node);
        }

        private string GetAdjective(ConjugationState state, JSONNode node)
        {
            return GetWord("adj", adjCycleIndex, ConjugationState.adjective, state, node);
        }

        /// <summary>
        /// Retrieve the user's desired conjugated word from the currentWordObject hierarchy.
        /// </summary>
        /// <param name="wordType">the key that matches the root type, i.e. "verbSingular", "mounSingular", or "adj"</param>
        /// <param name="cycleIndex">the current cycle index for the conjugation type (verb, noun, or adj)</param>
        /// <param name="root">the root conjugation type, i.e. if current conjugation is thirdSingularVerb, the root is firstSingularVerb</param>
        /// <param name="state">current conjugation state determined by user's wrist orientation</param>
        /// <param name="node">currentWordObject</param>
        /// <returns></returns>
        private string GetWord(string wordType, int cycleIndex, ConjugationState root, ConjugationState state, JSONNode node)
        {
            if (node["default"].Value.Equals("active") && state == ConjugationState.singularNoun)
            {
                Debug.Log(currentHasMultipleRecords);
            }
            try {
                if (node[wordType] == null)
                    return GetFirstInArrayOptionally(node, "default");
                else if (state == root || node[ConjugationStateToKey(state)] == null)
                    return GetInArrayOptionally(node, wordType, cycleIndex);
                else if (node[ConjugationStateToKey(state)].Count > 0)
                {
                    // more than one word available - let the user cycle to choose
                    if (cycleIndex >= node[ConjugationStateToKey(state)].Count)
                        return GetInArrayOptionally(node, ConjugationStateToKey(state), 
                            node[ConjugationStateToKey(state)].Count);
                    else return GetInArrayOptionally(node, ConjugationStateToKey(state), cycleIndex);
                }
                // only one word available for the current conjugation:
                else return GetFirstInArrayOptionally(node, ConjugationStateToKey(state));
            }
            catch (NullReferenceException e)
            {
                Debug.Log(e.Message);
            }
            return "";
            
        }
        #endregion Conjugation Handlers

        /// <summary>
        /// Updates the index used to determine current word choice out of a given conjugation array.
        /// </summary>
        private void CycleWord()
        {
            try
            {
                if ((int)Lexting.conjugator.conjugationState <= 3)
                    IterateCycleIndex(ref verbCycleIndex, currentWordObject.wordNode["verbSingular"]?.Count ?? 0);
                else if ((int)Lexting.conjugator.conjugationState <= 5 ||
                            Lexting.conjugator.conjugationState == ConjugationState.possessiveNoun)
                    IterateCycleIndex(ref nounCycleIndex, currentWordObject.wordNode["nounSingular"]?.Count ?? 0);
                else IterateCycleIndex(ref nounCycleIndex, currentWordObject.wordNode["adj"]?.Count ?? 0);
                Lexting.printer.SubtractWord(false);
                Lexting.printer.AppendOutputs(GetConjugatedPunctuatedWord(), true);
                OnRefreshWord?.I(_fullyRefreshedWord);
                timeSinceLastCycleTap = Time.time;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
        private void IterateCycleIndex(ref int current, int cycleRange)
        {
            if (cycleRange <= 0) return;
            else current = (current + 1) % cycleRange;
        }

        private void ResetState()
        {
            _hasConjugationChangedDuringThisFinalization = false;
            _hasPunctuationChangedDuringThisFinalization = false;
            _punctuationHoverBaselineIndex = -1;
            verbCycleIndex = 0;
            nounCycleIndex = 0;
            adjCycleIndex = 0;
            ClearListAndDisplay();
        }

        /// <summary>
        /// Remove all current word results from the word list and its UI.
        /// </summary>
        private void ClearListAndDisplay()
        {
            if (wordList.Count == 0)
                Lexting.printer.SubtractWord(true);
            foreach (Text text in _wordListUILabels)
                text.text = "";
            wordList.Clear();
            currentWordObject = new WordMetaData();
            currentWord = null;
        }

        private string GetFirstInArrayOptionally(JSONNode wordThatMayBeAnArray, string key)
        {
            if (wordThatMayBeAnArray[key].IsArray)
                return wordThatMayBeAnArray[key][0];
            else return wordThatMayBeAnArray[key];
        }

        private string GetInArrayOptionally(JSONNode wordThatMayBeAnArray, string key, int index)
        {
            if (wordThatMayBeAnArray[key].IsArray)
                return wordThatMayBeAnArray[key][index];
            else return wordThatMayBeAnArray[key];
        }
        #endregion helper methods
    }
}
