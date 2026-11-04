using SpatialGestureText.LetterCube;
using SpatialGestureText.Transformation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static SpatialGestureText.LetterCube.CubeTouchStates;
using SpatialGestureText.Tutorial;
using UnityEngine.Events;
using UniRx;
using UniRx.Triggers;

namespace SpatialGestureText.Assets.HandsFreeText.Scripts.Debugging
{
    class DummyRight : MonoBehaviour
    {

        public static Pingleton<DummyRight> _ = new Pingleton<DummyRight>();
    }
}
