using SpatialGestureText.LetterCube;
using SpatialGestureText.TwoZoneCube;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.Interfaces
{
    /// <summary>
    /// Enables classes to access the controller whether it is for a two-by-two-by-two or a three-by-three-by-three cube
    /// </summary>
    public interface IController : IComponent
    {
        bool textCubeActive { get; set; }
        bool rightHandInBounds { get; set; }
        bool leftHandInBounds { get; set; }
        bool isInputStreamReceiver { get; set; }
        ITrigger currentRightTrigger { get; set; }
        ITrigger currentLeftTrigger { get; set; }
        Vector3 centerPosition { get; }
        SpatialGestureEvent<bool> ControllerActivated { get; set; }
        SpatialGestureEvent<bool, string> OutputBegan { get; set; }
        CubeDragLineVisualizer rightDragLineHandler { get; set; }
        CubeDragLineVisualizer leftDragLineHandler { get; set; }
        void ToggleCharDisplay(bool on);
        void ResetAllTriggers();
        void ResetAllTriggers(bool right);
        void FlashCubeHighlight(bool correct);
        Vector3 GetLetterPosition(char letter, bool right);
    }
}
