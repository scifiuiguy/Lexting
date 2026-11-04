using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using SpatialGestureText.TwoZoneCube;
using SpatialGestureText.Interfaces;

namespace SpatialGestureText.LetterCube
{
    /// <summary>
    /// DEPRECATED - eliminating the need for error filtering was a primary reason for
    /// upgrading from three-zone to two-zone
    /// </summary>
    public class ErrorFilter
    {
        #region vars
        /// <summary>
        /// Cached spatial distance between two letters
        /// </summary>
        private float _nearestDistance;

        /// <summary>
        /// Cached spatial distance between current letter and current neighbor
        /// </summary>
        private float _currentDistance;
        
        /// <summary>
        /// Cached spatial position of current letter
        /// </summary>
        private Vector3 _currentLetterPosition;

        /// <summary>
        /// Cached spatial position of current neighbor
        /// </summary>
        private Vector3 _neighborPosition;

        private char _currentNearestNeighbor;

        private Vector3 _currentHandWorldPosition;

        /// <summary>
        /// A tally of all words in the dictionary, categorized by their 1st two letters, used for prevalence analysis
        /// </summary>
        public ErrorAnalyzer[] errorAnalyzers;

        /// <summary>
        /// All letters in the alphabet in relation to letters spatially adjacent in the cube
        /// </summary>
        private Dictionary<char, List<string>> nearestNeighbors;

        /// <summary>
        /// Cached potential replacement for a single character of user input
        /// </summary>
        private char _firstLetterSwap, _secondLetterSwap;

        /// <summary>
        /// Cached potential replacement for all variations of user input
        /// </summary>
        private string _firstLetterResult, _secondLetterResult, _bothLetterResult;

        /// <summary>
        /// All combinations of two non-vowel letters that form the beginning of common English words
        /// </summary>
        private List<string> nonVowelWordBeginners = new List<string>() {
            "pr", "st", "tr", "ch", "sp", "th", "sh", "br", "gr", "cr", "cl",
            "pl", "sl", "sc", "wh", "fr", "dr", "gl", "sw", "sk", "sn",
            "ph", "kn", "tw", "sm", "wr", "gn", "ps", "gh", "rh"
        };

        /// <summary>
        /// Cached dictionary word currently being analyzed (at runtime)
        /// </summary>
        private string _word;

        private bool _isUsingTwoZone;
        #endregion vars

        #region init
        public ErrorFilter()
        {
            Init();
        }

        public ErrorFilter(bool isUsingTwoZone)
        {
            Init();
            _isUsingTwoZone = isUsingTwoZone;
        }

        public ErrorFilter(JSONNode wordList)
        {
            Init(wordList);
        }

        public void Init()
        {
            InitAnalyzer();
            InitNearestNeighborArray();
        }

        public void Init(JSONNode wordList)
        {
            InitAnalyzer();
            IncludeForAnalysis(wordList);
            InitNearestNeighborArray();
        }

        private void InitNearestNeighborArray()
        {
            nearestNeighbors = new Dictionary<char, List<string>>();
            nearestNeighbors.Add('a', new List<string>() { "h", "n", "s", "g", "m", "l", "v", "k" });
            nearestNeighbors.Add('b', new List<string>() { "e", "o", "u", "t", "f", "d" });
            nearestNeighbors.Add('c', new List<string>() { "i", "y", "m", "p", "l", "q", "v", "x" });
            nearestNeighbors.Add('d', new List<string>() { "e", "u", "b", "p", "t", "f", "r", "p" });
            nearestNeighbors.Add('e', new List<string>() { "n", "h", "t", "d", "r", "b", "g", "j" });
            nearestNeighbors.Add('f', new List<string>() { "o", "u", "b", "z", "t", "d", "w", "x" });
            nearestNeighbors.Add('g', new List<string>() { "a", "e", "y", "n", "r", "m" });
            nearestNeighbors.Add('h', new List<string>() { "a", "e", "o", "n", "t", "s" });
            nearestNeighbors.Add('i', new List<string>() { "v", "c", "l", "w", "k", "q", "x", "z" });
            nearestNeighbors.Add('j', new List<string>() { "e", "u", "y", "r", "d", "p" });
            nearestNeighbors.Add('k', new List<string>() { "a", "i", "o", "s", "v", "w" });
            nearestNeighbors.Add('l', new List<string>() { "a", "i", "y", "m", "v", "c" });
            nearestNeighbors.Add('m', new List<string>() { "a", "y", "g", "l", "n", "r", "c", "v" });
            nearestNeighbors.Add('n', new List<string>() { "a", "e", "h", "g", "t", "s", "r", "m" });
            nearestNeighbors.Add('o', new List<string>() { "i", "y", "l", "q", "m", "p", "v", "x" });
            nearestNeighbors.Add('p', new List<string>() { "u", "y", "j", "p", "r", "d", "c", "x" });
            nearestNeighbors.Add('q', new List<string>() { "i", "u", "y", "p", "c", "x" });
            nearestNeighbors.Add('r', new List<string>() { "e", "y", "g", "j", "n", "d", "m", "p" });
            nearestNeighbors.Add('s', new List<string>() { "a", "o", "h", "k", "t", "n", "v", "w" });
            nearestNeighbors.Add('t', new List<string>() { "e", "o", "h", "b", "n", "d", "s", "f" });
            nearestNeighbors.Add('u', new List<string>() { "d", "f", "b", "p", "j", "q", "x", "z" });
            nearestNeighbors.Add('v', new List<string>() { "a", "i", "l", "k", "s", "m", "c", "w" });
            nearestNeighbors.Add('w', new List<string>() { "i", "o", "k", "z", "s", "f", "v", "x" });
            nearestNeighbors.Add('x', new List<string>() { "i", "u", "q", "z", "f", "p", "c", "w" });
            nearestNeighbors.Add('y', new List<string>() { "r", "g", "j", "m", "p", "l", "c", "q" });
            nearestNeighbors.Add('z', new List<string>() { "o", "u", "i", "f", "w", "x" });
            nearestNeighbors.Add(' ', new List<string>() { "a", "e", "o", "h", "n", "t", "s", "d", "r", "i", "u", "b", "f", "y", "g", "j", "m", "p", "l", "c", "q", "v", "k", "w", "x", "z"});
        }

        public void InitAnalyzer()
        {
            errorAnalyzers = new ErrorAnalyzer[26] {
                new ErrorAnalyzer('a'), new ErrorAnalyzer('b'), new ErrorAnalyzer('c'), new ErrorAnalyzer('d'), new ErrorAnalyzer('e'), new ErrorAnalyzer('f'),
                new ErrorAnalyzer('g'), new ErrorAnalyzer('h'), new ErrorAnalyzer('i'), new ErrorAnalyzer('j'), new ErrorAnalyzer('k'), new ErrorAnalyzer('l'),
                new ErrorAnalyzer('m'), new ErrorAnalyzer('n'), new ErrorAnalyzer('o'), new ErrorAnalyzer('p'), new ErrorAnalyzer('q'), new ErrorAnalyzer('r'),
                new ErrorAnalyzer('s'), new ErrorAnalyzer('t'), new ErrorAnalyzer('u'), new ErrorAnalyzer('v'), new ErrorAnalyzer('w'), new ErrorAnalyzer('x'),
                new ErrorAnalyzer('y'), new ErrorAnalyzer('z')
            };
        }
        #endregion init

        #region filtering
        /// <summary>
        /// Determines whether the first two letters of user input are a mistake to be replaced,
        /// and if true, replaces them with best alternate.
        /// </summary>
        /// <param name="firstTwo">the first two letters of user input</param>
        public string CheckForAlternateLetters(string firstTwo)
        {
            
            Regex r = new Regex("^[a-zA-Z]*$");
            if (!r.IsMatch(firstTwo)) return firstTwo;
                
            PreparePossibleReplacements(firstTwo);
            if (IsVowel(_secondLetterSwap) && HasValidResults(_secondLetterResult))
                return _secondLetterResult;

            // This edge case list got out of hand and only yields slightly better results in 3x3x3. The problem is the UI itself.
            // Having 3 zones per axis enables almost all the errors we're trying to filter here. Time to reduce to 2 zones per axis.
            // Then we don't need any error filtering bc the user practically can't miss the intended zone.

            //Error Filtering Algorithm:
            //1) Requirement to begin: 1st two letters of user input match no dictionary words.
            // 2) If swapping out nearest neighbor(NN) in either hand yields a non - vowel beginner, replace input with that beginner and return.
            // 3) If swapping out NN in either hand yields a valid result in which the swapped letter is a vowel for either, compare the two swap results.
            // 3a) Among the two potential versions(i.e.v1 swaps 1st letter vs v2 swaps 2nd letter), replace user input with the version that is more common and return.
            // 4) If swapping out NN only yields a valid result in which the swapped letter is a vowel for one hand, replace with that result and return.
            // 5) If swapping out NN for both yields a valid result, then replace with that result and return.
            // 6) Keep 1st letter and sequence through all neighbors of 2nd letter. Replace with first valid result found and return.
            // 7) Keep 2nd letter and sequence through all neighbors of 1st letter. Replace with first valid result and return.
            // 8) Sequence through all neighbors of both 1st and 2nd letter. Replace with first valid result and return.



            // 1) Requirement to begin: 1st two letters of user input match no dictionary words.
            //    handled by CubeWordAssemblyController - CheckForAlternateLetters() only gets called if 1) is true

            //// 2) If swapping out nearest neighbor (NN) in either hand yields a non-vowel beginner...
            //PreparePossibleReplacements(firstTwo);
            //if (nonVowelWordBeginners.Contains(_firstLetterResult) && HasValidResults(_firstLetterResult))
            //    return _firstLetterResult;
            //else if (nonVowelWordBeginners.Contains(_secondLetterResult) && HasValidResults(_secondLetterResult))
            //    return _secondLetterResult;

            //// 3) If swapping out NN in either hand yields a valid result in which the swapped letter is a vowel for either, compare the two swap results.
            //if (IsVowel(_firstLetterSwap) && IsVowel(_secondLetterSwap) && HasValidResults(_firstLetterResult) && HasValidResults(_secondLetterResult))
            //{
            //    // 3a) Among the two potential versions (i.e. v1 swaps 1st letter vs v2 swaps 2nd letter), 
            //    // replace user input with the version that is more common and return.
            //    if (GetResultTotal(_firstLetterResult) > GetResultTotal(_secondLetterResult))
            //        return _firstLetterResult;
            //    else return _secondLetterResult;
            //}
            //// 4) If swapping out NN only yields a valid result in which the swapped letter is a vowel for one hand, 
            ////    replace with that result and return.
            //else if (IsVowel(_firstLetterSwap) && HasValidResults(_firstLetterResult))
            //    return _firstLetterResult;
            //else if (IsVowel(_firstLetterSwap) && HasValidResults(_firstLetterResult))
            //    return _firstLetterResult;

            //// 5) If swapping out NN for both yields a valid result, then replace with that result and return.
            //_bothLetterResult = _firstLetterSwap.ToString() + _secondLetterSwap.ToString();
            //if (HasValidResults(_bothLetterResult))
            //    return _bothLetterResult;

            //// 6) Keep 1st letter and sequence through all neighbors of 2nd letter. Replace with first valid result found and return.
            //foreach (string neighbor in nearestNeighbors[firstTwo[1]])
            //    if (HasValidResults(_firstLetterSwap.ToString() + neighbor))
            //        return _firstLetterSwap.ToString() + neighbor;

            //// 7) Keep 2nd letter and sequence through all neighbors of 1st letter. Replace with first valid result and return.
            //foreach (string neighbor in nearestNeighbors[firstTwo[0]])
            //    if (HasValidResults(neighbor + _secondLetterSwap.ToString()))
            //        return neighbor + _secondLetterSwap.ToString();

            //// 8) Sequence through all neighbors of both 1st and 2nd letter. Replace with first valid result and return.
            //foreach (string firstNeighbor in nearestNeighbors[firstTwo[0]])
            //    foreach (string secondNeighbor in nearestNeighbors[firstTwo[1]])
            //        if (HasValidResults(firstNeighbor + secondNeighbor))
            //            return firstNeighbor + secondNeighbor;

            return firstTwo;
        }

        // debug only
        public string CheckForAlternateLetters(string firstTwo, Vector3 handPosition)
        {
            PreparePossibleReplacements(firstTwo, handPosition);
            if (IsVowel(_secondLetterSwap) && HasValidResults(_secondLetterResult))
                return _secondLetterResult;
            return firstTwo;
        }
        #endregion filtering

        #region helper methods
        private bool IsVowel(char input)
        {
            return input.Equals('a') || input.Equals('e') || input.Equals('i') ||
                input.Equals('o') || input.Equals('u') || input.Equals('y');
        }

        public bool HasValidResults(string firstTwo)
        {
            return errorAnalyzers[CharToIndex(firstTwo[0])].ContainsSecondLetter(firstTwo[1]);
        }

        private int GetResultTotal(string firstTwo)
        {
            ErrorAnalyzer analyzer = errorAnalyzers[CharToIndex(firstTwo[0])];
            int secondLetterIndex = errorAnalyzers[CharToIndex(firstTwo[0])].CharToIndex(firstTwo[1]);
            return errorAnalyzers[CharToIndex(firstTwo[0])].secondLetters[secondLetterIndex].totalInstances;
        }

        private void PreparePossibleReplacements(string firstTwo)
        {
            //_firstLetterSwap = GetNearestNeighbor(firstTwo[0], CubeWordAssemblyController.Instance.rightBeganCurrentAssembly, true);
            //_secondLetterSwap = GetNearestNeighbor(firstTwo[1], !CubeWordAssemblyController.Instance.rightBeganCurrentAssembly, true);
            _firstLetterResult = _firstLetterSwap.ToString() + firstTwo[1];
            _secondLetterResult = firstTwo[0] + _secondLetterSwap.ToString();
        }

        // debug only
        private void PreparePossibleReplacements(string firstTwo, Vector3 handPosition)
        {
            //_firstLetterSwap = GetNearestNeighbor(firstTwo[0], CubeWordAssemblyController.Instance.rightBeganCurrentAssembly, false, handPosition);
            //_secondLetterSwap = GetNearestNeighbor(firstTwo[1], !CubeWordAssemblyController.Instance.rightBeganCurrentAssembly, false, handPosition);
            _firstLetterResult = _firstLetterSwap.ToString() + firstTwo[1];
            _secondLetterResult = firstTwo[0] + _secondLetterSwap.ToString();
        }

        private bool IsAdjacent(char currentLetter, char otherLetter)
        {
            return nearestNeighbors[currentLetter].Contains(otherLetter.ToString());
        }

        public char GetNearestNeighbor(char letter, bool right, bool vowelsOnly)
        {
            //_currentHandWorldPosition = (_isUsingTwoZone) 
            //    ? (right) ? HandCursor.rightPosition : HandCursor.leftPosition
            //    : (right) ? CubeHandTrackingVisualizer.rightPosition : CubeHandTrackingVisualizer.leftPosition;
            //_nearestDistance = 0;

            foreach (var neighbor in nearestNeighbors[letter])
            {
                if (vowelsOnly && !IsVowel(neighbor[0])) continue;

                if (_isUsingTwoZone)
                    _neighborPosition = Vector3.zero;
                else _neighborPosition = LextingController.Instance.GetLetterPosition(neighbor[0], right);
                _currentDistance = Mathf.Abs((_neighborPosition - _currentHandWorldPosition).magnitude);

                if (_nearestDistance == 0 || _currentDistance < _nearestDistance)
                {
                    _nearestDistance = _currentDistance;
                    _currentNearestNeighbor = neighbor[0];
                }
            }
            return _currentNearestNeighbor;
        }

        /// <summary>
        /// for debugging only
        /// </summary>
        public char GetNearestNeighbor(char letter, bool right, bool vowelsOnly, Vector3 handPosition)
        {
            _currentHandWorldPosition = handPosition;
            _nearestDistance = 0;

            // debug objects
            GameObject debugPositionMarkers = new GameObject("DebugPositionMarkers");
            debugPositionMarkers.transform.position = LextingController.Instance.transform.position;
            //

            foreach (var neighbor in nearestNeighbors[letter])
            {
                _neighborPosition = LextingController.Instance.GetLetterPosition(neighbor[0], right);
                _currentDistance = Mathf.Abs((_neighborPosition - _currentHandWorldPosition).magnitude);

                if (_nearestDistance == 0 || _currentDistance < _nearestDistance)
                {
                    _nearestDistance = _currentDistance;
                    _currentNearestNeighbor = neighbor[0];
                }

                // debug objects
                GameObject debugPositionMarker = new GameObject($"PositionMarker_{neighbor[0]}");
                debugPositionMarker.transform.position = _neighborPosition;
                debugPositionMarker.transform.SetParent(debugPositionMarkers.transform);
                //
            }
            return _currentNearestNeighbor;
        }
        
        public void IncludeForAnalysis(JSONNode wordList)
        {
            foreach (JSONNode wordNode in wordList)
            {
                _word = wordNode["default"].Value;
                if (!errorAnalyzers[CharToIndex(_word[0])].ContainsSecondLetter(_word[1]))
                    errorAnalyzers[CharToIndex(_word[0])].secondLetters.Add(new ErrorSubanalyzer(_word[1]));
                
                if (errorAnalyzers[CharToIndex(_word[0])].CharToIndex(_word[1]) >= 0)
                {
                    int secondLetterIndex = errorAnalyzers[CharToIndex(_word[0])].CharToIndex(_word[1]);
                    errorAnalyzers[CharToIndex(_word[0])].secondLetters[secondLetterIndex].totalInstances++;
                }
            }
        }

        public void PrintAnalysis()
        {
            string output = "Word Error Analysis: \n";
            foreach (ErrorAnalyzer analyzer in errorAnalyzers)
                foreach (ErrorSubanalyzer subanalyzer in analyzer.secondLetters)
                    output += $"# words beginning with {analyzer.firstLetter.ToString().ToUpper() + subanalyzer.secondLetter.ToString().ToUpper()}: " +
                        $"{subanalyzer.totalInstances} \n";
            Debug.Log(output);
        }

        private int CharToIndex(char letter)
        {
            switch (letter)
            {
                case 'a': return 0; case 'b': return 1; case 'c': return 2; case 'd': return 3; case 'e': return 4; case 'f': return 5;
                case 'g': return 6; case 'h': return 7; case 'i': return 8; case 'j': return 9; case 'k': return 10; case 'l': return 11;
                case 'm': return 12; case 'n': return 13; case 'o': return 14; case 'p': return 15; case 'q': return 16; case 'r': return 17;
                case 's': return 18; case 't': return 19; case 'u': return 20; case 'v': return 21; case 'w': return 22; case 'x': return 23;
                case 'y': return 24; case 'z': return 25;
            }
            return 0;
        }
        #endregion helper methods

        #region support classes
        /// <summary>
        /// A unit of data to be compared against the first letter of user input
        /// </summary>
        public class ErrorAnalyzer
        {
            public char firstLetter;
            public List<ErrorSubanalyzer> secondLetters;

            public ErrorAnalyzer(char _firstLetter)
            {
                firstLetter = _firstLetter;
                secondLetters = new List<ErrorSubanalyzer>();
            }

            public bool ContainsSecondLetter(char secondLetter)
            {
                foreach (ErrorSubanalyzer subanalyzer in secondLetters)
                    if (subanalyzer.secondLetter.Equals(secondLetter))
                        return true;
                return false;
            }

            public int CharToIndex(char letter)
            {
                foreach (var subanalyzer in secondLetters)
                    if (subanalyzer.secondLetter.Equals(letter))
                        return secondLetters.IndexOf(subanalyzer);
                return -1;
            }
        }

        /// <summary>
        /// A unit of data to be compared against the second letter of user input
        /// </summary>
        public class ErrorSubanalyzer
        {
            public char secondLetter;
            public int totalInstances;

            public ErrorSubanalyzer(char _secondLetter)
            {
                secondLetter = _secondLetter;
            }
        }
        #endregion support classes
    }
}