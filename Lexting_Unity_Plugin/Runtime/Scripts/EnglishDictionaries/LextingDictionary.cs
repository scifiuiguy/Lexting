using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SpatialGestureText.EnglishDictionaries
{
    /// <summary>
    /// Ingests JSON files containing dictionary root words and their conjugations, and places each record
    /// in a 4-tier fast look-up nested dictionary structure whereby each tier filters one of the first 
    /// four letters of the word. Akin to a hash table, each tier accelerates look-up by dividing results 
    /// by 26. 
    /// 
    /// In total, the top-level dictionary is filtered by 26 to the 4. 
    /// 
    /// For the MIT 10000 Word List, filtering yields fewer than 8 final results except in six 
    /// edge cases ("comp", "cons", "cont", "inte", "port", & "tran").
    /// 
    /// Edge cases are handled in the CubeWordAssemblyController class.
    /// </summary>
    public static class LextingDictionary
    {
        #region vars
        #region public vars
        /// <summary>
        /// Four-tier nested lookup structures in which each tier contains a subsequent letter of the final word (enables fast dicitonary lookups)
        /// </summary>
        public static Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, JSONNode>>>>> longerWordDictionary =
            new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, JSONNode>>>>>();
        public static Dictionary<string, JSONNode> twoLetterDictionary = new Dictionary<string, JSONNode>();
        public static Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, JSONNode>>>>> threeLetterDictionary =
           new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, JSONNode>>>>>();
        //public static Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, JSONNode>>>>> fourLetterDictionary =
        //   new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, JSONNode>>>>>();
        #endregion public vars
        #region private vars
        /// <summary>
        ///  Temporary nodes containing all the contents of a given JSON data file, used to init dictionary data from hard disk
        /// </summary>
        private static JSONNode _longerWords, _twoLetterWords, _threeLetterWords; //, _fourLetterWords;
        #endregion private vars
        #endregion vars

        #region init
        public static void InitializeEnglishWordDatabase()
        {
            InitTwoLetterWords();
            InitFastLookupDictionary("three_letter_words", /*"three_letter_words_json"*/ "three_letter_words_2x2x2", ref _threeLetterWords, ref threeLetterDictionary);
            //InitFastLookupDictionary("four_letter_words", "four_letter_words_json", ref _fourLetterWords, ref fourLetterDictionary);
            InitFastLookupDictionary("longerWords", /*"mit10000_json"*/ "longer_words_2x2x2", ref _longerWords, ref longerWordDictionary);

            // Debugging & Analysis///////////////
            //TestTwoByTwoByTwoGetters();
            //GenerateFirstFourAnalytics();
            //FindRecordsThatCannotBeProcedural();
            //FindRecordsThatEndWith("us", true);
            //////////////////////////////////////
        }

        private static void InitTwoLetterWords()
        {
            _twoLetterWords = InitWordArray("two_letter_words", /*"two_letter_words_json"*/ "two_letter_words_2x2x2");
            foreach (JSONNode wordNode in _twoLetterWords)
                DrillDownAdd(wordNode, twoLetterDictionary);
            //filter.IncludeForAnalysis(_twoLetterWords);
        }

        private static JSONNode InitWordArray(string arrayName, string sourceFileName)
        {
            TextAsset textAsset = (TextAsset)Resources.Load(sourceFileName);
            Debug.Log($"InitWordArray for array name {arrayName} got " + JSON.Parse(textAsset.text)[arrayName].Count + " words.");
            return JSON.Parse(textAsset.text)[arrayName];
        }

        /// <summary>
        /// Serialize JSON dictionary files into memory
        /// </summary>
        /// <param name="arrayName">The name of the array containing all words in the file (first line inside the file)</param>
        /// <param name="sourceFileName">The name of the JSON file</param>
        /// <param name="wordArray">The variable in which to store the contents of the array for parsing</param>
        /// <param name="dictionary">the dictionary that will hold the array's contents in memory</param>
        private static void InitFastLookupDictionary(
                string arrayName,
                string sourceFileName,
                ref JSONNode wordArray,
                ref Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, JSONNode>>>>> dictionary
            )
        {
            wordArray = InitWordArray(arrayName, sourceFileName);
            foreach (JSONNode wordNode in wordArray)
                DrillDownAdd(wordNode, dictionary);
            //filter.IncludeForAnalysis(wordArray);
            wordArray = null;
        }

        #region dictionary setters
        /// <summary>
        /// We needed a data structure for rapid English word access (i.e. drill down each letter as it is received).
        /// Originally implemented recursively with nested hash tables, 
        /// but default c# hash tables require boxing/unboxing at each tier during nesting.
        /// It was deadly slow... had to rewrite it.
        /// Alt 1: create a new hash table class from scratch with explicit casting.
        /// Alt 2: use nested dictionaries -> they're already explicit at runtime.
        /// Alt 2 would be silly if nesting were deeper, but for English we only need four tiers.
        /// This is the Alt 2 implementation.
        /// </summary>
        private static void DrillDownAdd(JSONNode wordNode, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, JSONNode>>>>> dictionary)
        {
            try {
                if (dictionary.ContainsKey(wordNode["default"].Value.Substring(0, 1).ToLower()))
                    DrillDownAdd(wordNode, dictionary[wordNode["default"].Value.Substring(0, 1).ToLower()]);
                else
                {
                    Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, JSONNode>>>> newDictionary =
                        new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, JSONNode>>>>();
                    dictionary.Add(wordNode["default"].Value.Substring(0, 1).ToLower(), newDictionary);
                    DrillDownAdd(wordNode, newDictionary);
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                Debug.Log($"DrillDownAdd ArgumentOutOfRange Exception on word: {wordNode["default"].Value}");
            }
        }

        private static void DrillDownAdd(JSONNode wordNode, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, JSONNode>>>> dictionary)
        {
            if (wordNode["default"].Value.Length <= 1)
            {
                if (dictionary.ContainsKey(wordNode["default"].Value.Length.ToString()))
                    DrillDownAdd(wordNode, dictionary[wordNode["default"].Value.Length.ToString()]);
                else
                {
                    Dictionary<string, Dictionary<string, Dictionary<string, JSONNode>>> newDictionary =
                        new Dictionary<string, Dictionary<string, Dictionary<string, JSONNode>>>();
                    dictionary.Add(wordNode["default"].Value.Length.ToString(), newDictionary);
                    DrillDownAdd(wordNode, newDictionary);
                }
            }
            else if (dictionary.ContainsKey(wordNode["default"].Value.Substring(1, 1).ToLower()))
                DrillDownAdd(wordNode, dictionary[wordNode["default"].Value.Substring(1, 1).ToLower()]);
            else
            {
                Dictionary<string, Dictionary<string, Dictionary<string, JSONNode>>> newDictionary =
                    new Dictionary<string, Dictionary<string, Dictionary<string, JSONNode>>>();
                dictionary.Add(wordNode["default"].Value.Substring(1, 1).ToLower(), newDictionary);
                DrillDownAdd(wordNode, newDictionary);
            }
        }

        private static void DrillDownAdd(JSONNode wordNode, Dictionary<string, Dictionary<string, Dictionary<string, JSONNode>>> dictionary)
        {
            if (wordNode["default"].Value.Length <= 2)
            {
                if (dictionary.ContainsKey(wordNode["default"].Value.Length.ToString()))
                    DrillDownAdd(wordNode, dictionary[wordNode["default"].Value.Length.ToString()]);
                else
                {
                    Dictionary<string, Dictionary<string, JSONNode>> newDictionary = new Dictionary<string, Dictionary<string, JSONNode>>();
                    dictionary.Add(wordNode["default"].Value.Length.ToString(), newDictionary);
                    DrillDownAdd(wordNode, newDictionary);
                }
            }
            else
            {
                if (dictionary.ContainsKey(wordNode["default"].Value.Substring(2, 1).ToLower()))
                    DrillDownAdd(wordNode, dictionary[wordNode["default"].Value.Substring(2, 1).ToLower()]);
                else
                {
                    Dictionary<string, Dictionary<string, JSONNode>> newDictionary = new Dictionary<string, Dictionary<string, JSONNode>>();
                    dictionary.Add(wordNode["default"].Value.Substring(2, 1), newDictionary);
                    DrillDownAdd(wordNode, newDictionary);
                }
            }
        }

        private static void DrillDownAdd(JSONNode wordNode, Dictionary<string, Dictionary<string, JSONNode>> dictionary)
        {
            if (wordNode["default"].Value.Length <= 3)
            {
                if (dictionary.ContainsKey(wordNode["default"].Value.Length.ToString()))
                    DrillDownAdd(wordNode, dictionary[wordNode["default"].Value.Length.ToString()]);
                else
                {
                    Dictionary<string, JSONNode> newDictionary = new Dictionary<string, JSONNode>();
                    dictionary.Add(wordNode["default"].Value.Length.ToString(), newDictionary);
                    DrillDownAdd(wordNode, newDictionary);
                }
            }
            else
            {
                if (dictionary.ContainsKey(wordNode["default"].Value.Substring(3, 1).ToLower()))
                    DrillDownAdd(wordNode, dictionary[wordNode["default"].Value.Substring(3, 1).ToLower()]);
                else
                {
                    Dictionary<string, JSONNode> newDictionary = new Dictionary<string, JSONNode>();
                    dictionary.Add(wordNode["default"].Value.Substring(3, 1).ToLower(), newDictionary);
                    DrillDownAdd(wordNode, newDictionary);
                }
            }
        }

        private static void DrillDownAdd(JSONNode wordNode, Dictionary<string, JSONNode> dictionary)
        {
            if (wordNode["default"].Value.Length <= 4)
            {
                if (!dictionary.ContainsKey(wordNode["default"].Value.ToLower()))
                {
                    dictionary.Add(wordNode["default"].Value.ToLower(), wordNode);
                    //Debug.Log(wordNode["default"].Value + " Just added");
                }

            }
            else if (!dictionary.ContainsKey(wordNode["default"].Value.Substring(4, 1).ToLower()))
            {
                dictionary.Add(wordNode["default"].Value.Substring(4, 1).ToLower(), wordNode);
                //Debug.Log(wordNode["default"].Value + " Just added at key: " + wordNode["default"].Value.Substring(4, 1));
            }
            else
            {
                // same word as one already saved but w/ a suffix
                if (dictionary.ContainsKey("_" + wordNode["default"].Value.Substring(4, wordNode["default"].Value.Length - 4)))
                    Debug.Log("Suffix Exception for word " + wordNode["default"].Value);
                else
                {
                    dictionary.Add("_" + wordNode["default"].Value.Substring(4, wordNode["default"].Value.Length - 4), wordNode);
                    //Debug.Log(wordNode["default"].Value + " Just added at key: " + "_" + wordNode["default"].Value.Substring(4, wordNode["default"].Value.Length - 4));
                }
            }
        }
        #endregion dictionary setters

        #region dictionary getters
        public static void GetTwoLetterWords(ref List<JSONNode> results, string input, int limit)
        {
            if (input.Length > 2) return;
            input = input.ToLower();
            foreach (KeyValuePair<string, JSONNode> nodePair in twoLetterDictionary)
            {
                if (results.Count >= limit) return;
                if (nodePair.Value["default"].Value.Substring(0,2).Equals(input) && !results.Contains(input))
                    results.Add(nodePair.Value);
            }
        }

        /// <summary>
        /// Given a string specifying the first four characters of the desired word, add all words
        /// from the fast-lookup dictionary that start with those first four characters into the results list.
        /// </summary>
        /// <param name="results">the list of results (provide it when this method begins and treat it as the return value when this method is complete)</param>
        /// <param name="firstFour">the desired first four characters of every word you want to filter into the results list</param>
        /// <param name="dictionary">the fast-lookup dictionary containing all the words that need to be filtered</param>
        /// /// <param name="dictionary">limits the total number of records returned in the final drill-down</param>
        public static void DrillDownGet(
            ref List<JSONNode> results,
            string firstFour,
            Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, JSONNode>>>>> dictionary,
            int limit)
        {
            if (limit <= 0)
                limit = 8;
            firstFour = firstFour.ToLower();
            if (dictionary.ContainsKey(firstFour.Substring(0, 1)))
                DrillDownGet(ref results, firstFour, dictionary[firstFour.Substring(0, 1)], limit);
        }

        /// <summary>
        /// A recursion-esque override used to filter the last three of the first four characters
        /// </summary>
        private static void DrillDownGet(
            ref List<JSONNode> results,
            string firstFour,
            Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, JSONNode>>>> dictionary,
            int limit)
        {
            if (dictionary.ContainsKey(firstFour.Substring(1, 1)))
                DrillDownGet(ref results, firstFour, dictionary[firstFour.Substring(1, 1)], limit);
        }

        /// <summary>
        /// A recursion-esque override used to filter the last two of the first four
        /// </summary>
        private static void DrillDownGet(
            ref List<JSONNode> results,
            string firstFour,
            Dictionary<string, Dictionary<string, Dictionary<string, JSONNode>>> dictionary,
            int limit)
        {
            if (firstFour.Length <= 2)
            {
                foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, JSONNode>>> childDictionaryPair in dictionary)
                    DrillDownGet(ref results, firstFour, childDictionaryPair.Value, limit);
            }
            else if (dictionary.ContainsKey(firstFour.Substring(2, 1)))
                DrillDownGet(ref results, firstFour, dictionary[firstFour.Substring(2, 1)], limit);
        }

        /// <summary>
        /// A recursion-esque override used to filter the last one of the first four characters
        /// </summary>
        private static void DrillDownGet(
            ref List<JSONNode> results, 
            string firstFour, 
            Dictionary<string, Dictionary<string, JSONNode>> dictionary,
            int limit)
        {
            if (firstFour.Length <= 2)
            {
                foreach (KeyValuePair<string, Dictionary<string, JSONNode>> childDictionaryPair in dictionary)
                    if (childDictionaryPair.Key.Equals("3"))
                        DrillDownGet(ref results, firstFour, childDictionaryPair.Value, limit);
                DrillDownGetAll(ref results, firstFour, dictionary, limit);
            }
            else
            if (firstFour.Length == 3)
            {
                if (HasDuplicateChars(firstFour) && dictionary.ContainsKey(firstFour.Substring(0, 1)))
                    DrillDownGet(ref results, firstFour + firstFour.Substring(0, 1), dictionary[firstFour.Substring(0, 1)], limit);
                else
                {
                    foreach (KeyValuePair<string, Dictionary<string, JSONNode>> childDictionaryPair in dictionary)
                        if (childDictionaryPair.Key.Equals("3") || childDictionaryPair.Key.Equals("4"))
                            DrillDownGet(ref results, firstFour, childDictionaryPair.Value, limit);
                    //else if (childDictionaryPair.Key == firstFour.Substring(2, 1))
                    DrillDownGetAll(ref results, firstFour, dictionary, limit);
                }
            }
            else if (dictionary.ContainsKey(firstFour.Substring(3, 1)))
                DrillDownGet(ref results, firstFour, dictionary[firstFour.Substring(3, 1)], limit);
        }

        /// <summary>
        /// A recursion-eque override used to collect all filtered results after drilling down through all four parent dictionaries.
        /// </summary>
        private static void DrillDownGet(ref List<JSONNode> results, string firstFour, Dictionary<string, JSONNode> dictionary, int limit)
        {
            foreach (KeyValuePair<string, JSONNode> pair in dictionary)
            {
                if (results.Count > limit) return;
                if (firstFour.Length > 4 && NoMatchWhenPrefixIsIncluded(firstFour, pair.Value["default"].Value))
                    continue;
                results.Add(pair.Value);
            }
        }

        /// <summary>
        /// Allows DrillDownGet to skip words that do no match the full currentWord (including prefix) - this only runs when the prefix is not empty
        /// </summary>
        /// <param name="userInput">current word input, including prefix</param>
        /// <param name="word">potentially matching word found in the dictionary (matches at least first 4 but possibly not the remainder)</param>
        /// <returns>if true, the foundWord is not a perfect match for userInput, false otherwise</returns>
        private static bool NoMatchWhenPrefixIsIncluded(string userInput, string foundWord)
        {
            return foundWord.Length < userInput.Length || (foundWord.Length >= userInput.Length && !foundWord.Substring(0, userInput.Length).Equals(userInput));
        }

        /// <summary>
        /// Get all results when only 2 letters are provided
        /// </summary>
        private static void DrillDownGetAll(ref List<JSONNode> results, string firstTwo, Dictionary<string, Dictionary<string, JSONNode>> dictionary, int limit)
        {
            foreach (KeyValuePair<string, Dictionary<string, JSONNode>> pair in dictionary)
            {
                if (results.Count >= limit) return;
                DrillDownGetAll(ref results, firstTwo, pair.Value, limit);
            }
            
        }

        /// <summary>
        /// Get all results when only 3 letters are provided
        /// </summary>
        private static void DrillDownGetAll(ref List<JSONNode> results, string firstThree, Dictionary<string, JSONNode> dictionary, int limit)
        {
            foreach (KeyValuePair<string, JSONNode> pair in dictionary)
            {
                if (results.Count >= limit) return;
                if (!pair.Value["default"].Value.EndsWith("ing"))
                    results.Add(pair.Value);
            }

        }
        #endregion dictionary getters

        /// <summary>
        /// Wipe all allocated dictionary records from memory. 
        /// Do this any time you know the Lexting interface is being dismissed for an
        /// extended period of time. It will take a moment to reconstruct the dictionary,
        /// but you may as well use this method to free up memory when it's not in use.
        /// Call InitFastLookupDictionary() to reconstruct the dictionary prior to re-initalizing the Lexting UI.
        /// </summary>
        public static void ClearEnglishWordDatabase()
        {
            longerWordDictionary.Clear();
            threeLetterDictionary.Clear();
            twoLetterDictionary.Clear();
        }
        #endregion init

        #region helper methods
        private static bool HasDuplicateChars(string input)
        {
            if (input.Length < 3)
            {
                Debug.LogWarning("HasDuplicateChars received an input that was less than 3 digits.");
                return false;
            }
            else
            {
                char[] inputChars = input.ToCharArray();
                return (inputChars[0] == inputChars[2]);
            }
        }

        /// <summary>
        /// Used for quick analysis of the database only - NOT used to pluralize words at runtime bc
        /// it cannot be guaranteed to spell words correctly 100% of the time
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private static string ConvertSingularToPlural(string word)
        {
            if (word.Length < 2) return "";
            if (word[word.Length - 1] == 'y')
            {
                if (word[word.Length - 2] == 'e')
                {
                    return word + "s"; // e.g. monkey to monkeys
                }
                else {
                    return word.Substring(0, word.Length - 1) + "ies";
                }
            }
           // else if ()

            return "";
        }
        #endregion helper methods

        #region Unit Tests
        private static string[] alphabet = new string[] {
                "a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z"
            };

        /// <summary>
        /// The new dictionary V2 syntax enables conjugations to optionally contain arrays instead of single strings.
        /// Get all indices of an example record to verify SimpleJSON parsing syntax.
        /// </summary>
        private static void TestTwoByTwoByTwoGetters()
        {
            List<JSONNode> results = new List<JSONNode>();
            DrillDownGet(ref results, "acce", longerWordDictionary, 3);
            Debug.Log($"Dictionary V2 getter test: access verbSingular default: {results[2]["default"].Count} [0]: {results[2]["verbSingular"][0]} [1]: {results[2]["verbSingular"][1]}");
        }

        /// <summary>
        /// For each dictionary entry that has the same first four letters as any other,
        /// tally the total number. If it's 7 or above, add that combo of first four to a priority list
        /// </summary>
        private static void GenerateFirstFourAnalytics()
        {
            Dictionary<string, int> priorityList = new Dictionary<string, int>();
            List<JSONNode> results = new List<JSONNode>();
            string testFour = "";

            foreach (string alphaChar1 in alphabet)
                foreach (string alphaChar2 in alphabet)
                    foreach (string alphaChar3 in alphabet)
                        foreach (string alphaChar4 in alphabet)
                        {
                            testFour = alphaChar1 + alphaChar2 + alphaChar3 + alphaChar4;
                            results.Clear();
                            DrillDownGet(ref results, testFour, longerWordDictionary, 8);
                            if (results.Count > 8)
                                priorityList.Add(testFour, results.Count);
                        }

            if (priorityList.Count > 0)
                Debug.Log(priorityList["acco"]);

            string output = "";
            foreach (KeyValuePair<string,int> pair in priorityList)
                output += pair.Key + " " + pair.Value + "\n";

            WriteOutliersToFile(output, "Assets/Resources/high_result_combos.txt");
        }

        /// <summary>
        /// DEPRECATED - only compatible w/ dictionary V1 (v2 has different keys)
        /// For each dictionary entry, determine whether the plural is simply
        /// identical to the original word plus extra chars.
        /// If not, add the word to an outlier list for review.
        /// </summary>
        private static void FindRecordsThatCannotBeProcedural()
        {
            List<string> outliers = new List<string>();
            string testFour = "";
            List<JSONNode> results = new List<JSONNode>();

            foreach (string alphaChar1 in alphabet)
                foreach (string alphaChar2 in alphabet)
                    foreach (string alphaChar3 in alphabet)
                        foreach (string alphaChar4 in alphabet)
                        {
                            testFour = alphaChar1 + alphaChar2 + alphaChar3 + alphaChar4;
                            results.Clear();

                            DrillDownGet(ref results, testFour, longerWordDictionary, 8);
                            foreach (JSONNode result in results)
                            {
                                if (result["plural"].Value != "null" &&
                                    (result["wordClass"] == 0 || result["wordClass"] == 1) &&
                                    !result["plural"].Value.Contains(result["default"].Value))
                                    outliers.Add(result["default"].Value);
                            }
                        }

            if (outliers.Count > 0)
                Debug.Log(outliers[0]);

            string listInline = "";
            foreach (string outlier in outliers)
                listInline += outlier + "\n";

            WriteOutliersToFile(listInline, "Assets/Resources/outliers.txt");
        }

        /// <summary>
        /// DEPRECATED - only compatible w/ dictionary V1 (V2 has different keys)
        /// For each dictionary entry, if the word's suffix matches the one provided,
        /// add it to a list of outliers for review.
        /// </summary>
        private static void FindRecordsThatEndWith(string suffix, bool verbsAndNounsOnly)
        {
            List<string> outliers = new List<string>();

            string testFour = "";
            List<JSONNode> results = new List<JSONNode>();
            bool suffixMatches = false;
            foreach (string alphaChar1 in alphabet)
                foreach (string alphaChar2 in alphabet)
                    foreach (string alphaChar3 in alphabet)
                        foreach (string alphaChar4 in alphabet)
                        {
                            testFour = alphaChar1 + alphaChar2 + alphaChar3 + alphaChar4;
                            results.Clear();
                            DrillDownGet(ref results, testFour, longerWordDictionary, 8);
                            foreach (JSONNode result in results)
                            {
                                suffixMatches = result["default"].Value.Substring(result["default"].Value.Length - suffix.Length, suffix.Length).Equals(suffix);
                                if (!verbsAndNounsOnly)
                                {
                                    if (suffixMatches)
                                        outliers.Add(result["default"].Value);
                                }
                                else {
                                    if ((result["wordClass"] == 0 || result["wordClass"] == 1) && suffixMatches)
                                        outliers.Add(result["default"].Value);
                                }
                            }
                        }

            if (outliers.Count > 0)
                Debug.Log(outliers[0]);

            string listInline = "";
            foreach (string outlier in outliers)
                listInline += outlier + "\n";

            WriteOutliersToFile(listInline, "Assets/Resources/recordsthatendwith" + suffix + ".txt");
        }

        /// <summary>
        /// Ingest the list of most common words in English. Tally most common 1st/3rd letter combos and 2nd/4th combos.
        /// Add the tally of each to its inverse if available, then sort by highest tally and print the list.
        /// This function is used to rank 2-letter combos by prevalence in English so we can
        /// determine which letters should go in each 2x2x2 grid together to minimize ulnar wrist rotation.
        /// Conclusion:
        /// MIT10000 prevalence showed these combos should go together:
        /// relax:		c n m a t g u d
        /// pronate:	p s b i e v l o
        /// supinate:	h w y r j f k q
        /// 
        /// Also, the following arrangements maximize overlap and/or adjacency 
        /// of high-popularity combos under the above constraints:
        /// RELAX:
        /// top front: 	    c n
        /// top back:	    a t
        /// bottom front:	m d
        /// bottom back:	g u
        /// PRONATE:
        /// top front:	    o l
        /// top back:	    e b
        /// bottom front:	s v
        /// bottom back:	i p
        /// SUPINATE:
        /// top front:	    r j
        /// top back:	    y h
        /// bottom front:	w f
        /// bottom back:	k q
        /// X and Z (least frequent) should fall in overflow positions (center-pronate and center-supinate).
        /// </summary>
        public static void SortTwoLetterCombos()
        {
            Dictionary<string,int> combos = new Dictionary<string,int>();
            string current13Combo = "";
            string current13Inverse = "";
            string current24Combo = "";
            string current24Inverse = "";

            JSONNode commonWordsJSON = InitWordArray("longerWords", "mit10000_json");
            /////////////// top 1000 from commonWords.json:
            //List<string> wordList = commonWordsJSON.ToString().Split(',').ToList();
            ////////////////

            //////////////// top 10000 from mit10000.json
            List<string> wordList = new List<string>();
            foreach (JSONNode node in commonWordsJSON)
                wordList.Add(node["default"].ToString().ToLower());
            ////////////////

            for (int i = 0; i < wordList.Count; i++)
            {
                Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                wordList[i] = rgx.Replace(wordList[i], "");
            }


            foreach (string word in wordList)
            {
                if (word.Length < 4) continue;
                current13Combo = word.Substring(0, 1) + word.Substring(2, 1);
                current13Inverse = word.Substring(2, 1) + word.Substring(0, 1);
                if (!combos.ContainsKey(current13Inverse))
                {
                    if (combos.ContainsKey(current13Combo))
                        combos[current13Combo] += 1;
                    else combos.Add(current13Combo, 1);
                }
                else if (!combos.ContainsKey(current13Combo))
                    combos.Add(current13Combo, 1);

                current24Combo = word.Substring(1, 1) + word.Substring(3, 1);
                current24Inverse = word.Substring(3, 1) + word.Substring(1, 1);
                if (!combos.ContainsKey(current24Inverse))
                {
                    if (combos.ContainsKey(current24Combo))
                        combos[current24Combo] += 1;
                    else combos.Add(current24Combo, 1);
                }
                else if (!combos.ContainsKey(current24Combo))
                    combos.Add(current24Combo, 1);
            }

            var combosSorted = combos.ToList();
            combosSorted.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

            string listInline = "";
            foreach (KeyValuePair<string,int> record in combosSorted)
                listInline += record.Key + " " + record.Value + "\n";
            WriteOutliersToFile(listInline, "Assets/Resources/combos.txt");
        }

        private static void WriteOutliersToFile(string content, string path)
        {
            StreamWriter writer = new StreamWriter(path, true);
            writer.WriteLine(content);
            writer.Close();
        }

        public static void UnitTest_PageableRecordGetters()
        {
            List<JSONNode> results = new List<JSONNode>();
            DrillDownGet(ref results, "acti", longerWordDictionary, 8);
            Debug.Log(results[0]["default"].Value);
        }
        #endregion Unit Tests
    }
}
