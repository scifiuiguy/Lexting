using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SpatialGestureText.Interfaces
{

    /// <summary>
    /// Enables classes to access the trigger whether it is for a two-by-two-by-two or a three-by-three-by-three cube
    /// </summary>
    public interface ITrigger : IComponent
    {
        IController controller { get; set; }
        Transform center { get; }
        bool isHighlightedByRight { get; set; }
        bool isHighlightedByLeft { get; set; }
        int Index { get; }
        bool IsHotkey { get; }
        bool IsMatrixState { get; }
        string matrixDisplayState { get; set; }
        void SetStartingIndex(bool right);
        void ToggleHighlightOn(bool right);
        void HandleTriggerEnter(bool right);
        void HandleExit(bool right);
        void Bump(UnityAction callback);
        void ResetTrigger();
    }
}