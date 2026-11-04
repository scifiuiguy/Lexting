using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.LetterCube
{
    // DEPRECATED
    /// <summary>
    /// Provides quick access to a letter's current position
    /// relative to its top-level parent (the pivot point of the letter grid)
    /// when the letter's grid index is known
    /// -- needed for remembering the position of the letter grid 
    /// when the hand moves out of the grid to the left or right
    /// </summary>
    public class CubeTriggerFinder : Singleton<CubeTriggerFinder>
    {
        public Vector3 GetTriggerIndexDelta(CubeLetterGridIndex index)
        {
            Vector3 childPosition = transform.GetChild(index.column).GetChild(0).GetChild(index.row).position;

            // use this to test all letters manually: 
            // Vector3 childPosition = transform.GetChild(5).GetChild(0).GetChild(4).position;

            Vector3 result = childPosition - transform.position;
            return result;
        }
    }
}


