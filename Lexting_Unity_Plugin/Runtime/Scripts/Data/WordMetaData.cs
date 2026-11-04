using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpatialGestureText.Data
{
    /// <summary>
    /// A template for a conjugatable single dictionary record 
    /// associated with a UI element meant to display the word.
    /// </summary>
    [Serializable]
	public struct WordMetaData 
	{
        public WordMetaData(Text _label, JSONNode _wordNode)
        {
            label = _label;
            wordNode = _wordNode;
        }
        public Text label;
        public JSONNode wordNode;
    }
}
