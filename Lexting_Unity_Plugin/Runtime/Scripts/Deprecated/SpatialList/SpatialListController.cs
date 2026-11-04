using SimpleJSON;
using SpatialGestureText.Data;
using SpatialGestureText.Input;
using SpatialGestureText.Interfaces;
using SpatialGestureText.LetterCube;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpatialGestureText
{
    /// <summary>
    /// DEPRECATED - was used to display filtered word results in the old 3x3x3 UI grid.
    /// Controls display and behavior of the CubeWordAssemblyController's wordList object
    /// This is a list of words starting with the first four letters chosen by the User, displayed in a 3D grid.
    /// </summary>
    public class SpatialListController : Singleton<SpatialListController>, IWordListController
    {
        public List<WordMetaData> wordList { get; set; }

        [SerializeField] private List<WordMetaData> _wordList;
        private List<WordMetaData> unconjunctiveWords = new List<WordMetaData>(); 

        public Text currentWord { get; set; }
        public WordMetaData currentWordObject { get; set; }

        // not currently in use - perhaps use in future to allow scroll to further pages to increase word display beyond 27
        private int _pageIndex;

        public bool isSpatialListActive { get; set; }

        public bool IsReady() => this.Ready();

        private void Awake()
        {
            if (_wordList.Count > 0)
                currentWord = _wordList[0].label;
        }

        public void ShowList(bool on)
        {
            if ((!on && !isSpatialListActive) || (on && isSpatialListActive)) return;
            else if (!on && isSpatialListActive)
            {
                // clear list and toggle it off
                UpdateListObjectDisplay();
                currentWord.text = "";
                transform.GetChild(0).gameObject.SetActive(on);               
            }
            else if (on && !isSpatialListActive)
            {
                // refresh list and toggle it on
                transform.GetChild(0).gameObject.SetActive(on);
                UpdateListObjectDisplay();
            }
            isSpatialListActive = on;
        }

        /// <summary>
        /// DEPRECATED
        /// </summary>
        public void SelectWord(int index)
        {
            //foreach (WordMetaData wordObject in _wordList)
            //{
            //    //Debug.Log("Selecting word: wordObject.name: " + wordObject.label.transform.name + " wordObjectName: " + wordObjectName);
            //    if (wordObject.label.name == wordObjectName)
            //    {
            //        currentWord = wordObject.label;
            //        currentWordObject = wordObject;
            //        CubeWordAssemblyController.Instance.UpdateFullWordWithConjugation();
            //        break;
            //    }
            //}
        }

        public void SelectPunctuation(int index)
        { 
            /// deprecated - zone-based punctuation was not available until three-zone UI was already obsolete
            /// bc punctuation used to be handled via wrist orientation
        }
        
        public void UpdateListTransform(Transform newTransform)
        {
            transform.position = newTransform.position;
            transform.rotation = newTransform.rotation;
        }

        public void UpdateWordList(List<JSONNode> list)
        { 
            // This class was deprecated prior to the existence of this method
            // this is here just to adhere to the interface
        }
        public string GetConjugatedPunctuatedWord()
        {
            // This class was deprecated prior to the existence of this method
            // this is here just to adhere to the interface
            return "";
        }

        /// <summary>
        /// Refresh/Clear list of visible words in the spatial list
        /// </summary>
        private void UpdateListObjectDisplay()
        {
            unconjunctiveWords.Clear();

            for (int i = 0; i < 27; i++)
                _wordList[i].label.text = "";

            // CubeWordAssemblyController.Instance.wordList is DEPRECATED
            //Debug.Log($"SpatialListController - UpdateListObjectDisplay - assembly controller word list count: {CubeWordAssemblyController.Instance.wordList.Count }");
            //if (CubeWordAssemblyController.Instance.wordList.Count == 0) return;

            int offset = _pageIndex * 27;
            string debugOutput = "spatial list: ";

            int wordCount = 0;
            for (int i = 0; i < 27; i++)
            {
                wordCount = i;
                // CubeWordAssemblyController.Instance.wordList is DEPRECATED
                //if (i+offset > CubeWordAssemblyController.Instance.wordList.Count - 1)
                //{
                //    Debug.Log(debugOutput);
                //    break;
                //}


                // CubeWordAssemblyController.Instance.wordList is DEPRECATED
                //_wordList[i] = new WordMetaData(_wordList[i].label, CubeWordAssemblyController.Instance.wordList[i + offset]);


                _wordList[i].label.text = _wordList[i].wordNode["default"].Value;
                debugOutput += _wordList[i].wordNode["default"].Value + ", ";
                if (_wordList[i].wordNode["other"].Count > 0)
                    unconjunctiveWords.Add(_wordList[i]);
            }
            Debug.Log(debugOutput);
            foreach (WordMetaData wordData in unconjunctiveWords)
            {
                foreach (string word in wordData.wordNode["other"].Values)
                {
                    Debug.Log($"word count: {wordCount}");
                    if (wordCount >= 26)
                        break;
                    _wordList[wordCount] = new WordMetaData(_wordList[wordCount].label, wordData.wordNode);
                    _wordList[wordCount].label.text = word;
                    wordCount++;
                    debugOutput += _wordList[wordCount] + "_" + word + ", ";
                }
            }
            Debug.Log(debugOutput);
        }
    }
}


