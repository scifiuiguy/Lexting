using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.LetterCube
{
    public enum HandGestureState
    {
        Neutral,
        Insert,
        Selection,
        NonPivotSelection,
        DragSelection,
        Click,
        SqueezeAll,
        Deselection,
        DeselectAll,
        Stop,
        PopGun
    }
}

