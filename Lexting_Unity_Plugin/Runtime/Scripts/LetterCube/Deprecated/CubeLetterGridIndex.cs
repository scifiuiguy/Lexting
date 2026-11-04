using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.LetterCube
{
    public struct CubeLetterGridIndex
    {
        public CubeLetterGridIndex(int _row, int _column)
        {
            row = _row;
            column = _column;
        }
        public int row;
        public int column;
    }
}


