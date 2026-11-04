using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.LetterCube
{
    public class CubeTouchStates : MonoBehaviour
    {
        [Flags]
        public enum TouchStates
        {
            none = 0,
            pointer = 1 << 0,
            thumb = 1 << 1,
            middle = 1 << 2

        }

        public enum TouchStateIndex
        {
            pointer,
            thumb,
            middle
        }
    }
}


