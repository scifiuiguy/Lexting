using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.LetterCube
{
    public static class CubeLetterIndexConverter 
    {
        public static char[][][] lettersByIndex = new char[3][][] {
            new char[3][]{ new char[3] { 'l', 'v', 'k' }, new char[3] { 'm', 'a', 's' }, new char[3] { 'g', 'n', 'h' } },
            new char[3][]{ new char[3] { 'c', 'i', 'w' }, new char[3] { 'y', ' ', 'o' }, new char[3] { 'r', 'e', 't' } },
            new char[3][]{ new char[3] { 'q', 'x', 'z' }, new char[3] { 'p', 'u', 'f' }, new char[3] { 'j', 'd', 'b' } }
        };

        public static int[] CharToIndex(char letter)
        {
            switch (letter)
            {
                case 'a': return new int[3] { 0, 1, 1 };
                case 'b': return new int[3] { 2, 2, 2 };
                case 'c': return new int[3] { 1, 0, 0 };
                case 'd': return new int[3] { 2, 2, 1 };
                case 'e': return new int[3] { 1, 2, 1 };
                case 'f': return new int[3] { 2, 1, 2 };
                case 'g': return new int[3] { 0, 2, 0 };
                case 'h': return new int[3] { 0, 2, 2 };
                case 'i': return new int[3] { 1, 0, 1 };
                case 'j': return new int[3] { 2, 2, 0 };
                case 'k': return new int[3] { 0, 0, 2 };
                case 'l': return new int[3] { 0, 0, 0 };
                case 'm': return new int[3] { 0, 1, 0 };
                case 'n': return new int[3] { 0, 2, 1 };
                case 'o': return new int[3] { 1, 1, 2 };
                case 'p': return new int[3] { 2, 2, 0 };
                case 'q': return new int[3] { 2, 0, 0 };
                case 'r': return new int[3] { 1, 2, 0 };
                case 's': return new int[3] { 0, 1, 2 };
                case 't': return new int[3] { 1, 2, 2 };
                case 'u': return new int[3] { 2, 1, 1 };
                case 'v': return new int[3] { 0, 0, 1 };
                case 'w': return new int[3] { 1, 0, 2 };
                case 'x': return new int[3] { 2, 0, 1 };
                case 'y': return new int[3] { 1, 1, 0 };
                case 'z': return new int[3] { 2, 0, 2 };
            }
            return new int[3] { 1, 1, 1 };
        }

        public static char IndexToChar(int[] index)
        {
            return lettersByIndex[index[0]][index[1]][index[2]];
        }
    }
}


