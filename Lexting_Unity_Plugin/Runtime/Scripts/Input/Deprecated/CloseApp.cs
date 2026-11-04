using SpatialGestureText.LetterCube;
using SpatialGestureText.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.Input
{
    public class CloseApp : MonoBehaviour
    {
        private SpatialGestureInput _input;

        private void Awake()
        {
            _input = Lexting.input;
            this.Subscribe(_input.OnRightStateChanged, OnRightStateChanged);
            this.Subscribe(_input.OnLeftStateChanged, OnLeftStateChanged);
        }

        private void OnRightStateChanged(HandGestureState current, HandGestureState previous)
        {
            if (current == HandGestureState.DragSelection && Lexting.lhState.Equals(HandGestureState.DragSelection))
                this.WaitS(0.5f, () => CloseCheck());
        }

        private void OnLeftStateChanged(HandGestureState current, HandGestureState previous)
        {
            if (current == HandGestureState.DragSelection && Lexting.rhState.Equals(HandGestureState.DragSelection))
                this.WaitS(0.5f, () => CloseCheck());
        }

        private void CloseCheck()
        {
            if (Lexting.lhState == HandGestureState.DragSelection && Lexting.rhState == HandGestureState.DragSelection)
                Application.Quit();
        }
    }
}