using SpatialGestureText.LetterCube;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.Transformation
{
    public interface IListenForTransformation
    {
        void OnBeginDrag(CubeHand hand);
        void OnEndDrag(CubeHand hand);
    }
}


