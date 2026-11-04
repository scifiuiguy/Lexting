using SimpleJSON;
using SpatialGestureText.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpatialGestureText.Interfaces
{
	/// <summary>
	/// Enables classes to access the list controller whether it is for a two-by-two-by-two or a three-by-three-by-three cube
	/// </summary>
	public interface IWordListController : IComponent
	{
		bool isSpatialListActive { get; set; }
		List<WordMetaData> wordList { get; set; }
		WordMetaData currentWordObject { get; set; }
		Text currentWord { get; set; }
		void SelectWord(int index);
		void SelectPunctuation(int index);
		void UpdateListTransform(Transform newTransform);
		void ShowList(bool on);
		void UpdateWordList(List<JSONNode> list);
		string GetConjugatedPunctuatedWord();

	}
}
