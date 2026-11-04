using SpatialGestureText.Input;
using SpatialGestureText.LetterCube;
using SpatialGestureText.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.TwoZoneCube
{
    /// <summary>
    /// DEPRECATED
    /// </summary>
	public class TwoZoneCubeSpaceHandler : Singleton<TwoZoneCubeSpaceHandler>
	{
        //#region vars
        //private SpatialGestureInput _input;
        //private LextingOutputManager _outputManager;
        
        ///// <summary>
        ///// A cached reference to the rotation state of the current hand
        ///// </summary>
        //private CubeRotationState _currentRotationState;
        //#endregion vars

        //#region init
        //private void Awake()
        //{
        //    _outputManager = LextingOutputManager.Instance;
        //    _input = SpatialGestureInput.Instance;
        //}
        //#endregion init

        //#region event handlers
        //#endregion event handlers

        //#region helpers
        //public void Insert(bool right)
        //{
        //    Debug.Log($"TwoZoneCubeSpaceHandler - " + ((right) ? "right -" : "left -") + $" begin Insert");
        //    _currentRotationState = (right) ? _input.currentRightRotationState : _input.currentLeftRotationState;
        //    //_otherRotationState = (right) ? _input.currentLeftRotationState : _input.currentRightRotationState;


        //    //Debug.Log($"TwoZoneCubeSpaceHandler - cube rotation state: {_currentRotationState} " +
        //    //        $"currentOutputValue.Length: {_outputManager.CurrentOutputValue.Length}");

        //    if (_currentRotationState == CubeRotationState.relax)
        //    {
        //        _outputManager.AppendOutputs(" ", true);
        //        //Debug.Log($"TwoZoneCubeSpaceHandler - just appended space to outputs.");
        //    }
        //    else if (_currentRotationState == CubeRotationState.pronate)
        //    {
        //        if (Lexting.printer.CurrentOutputValue.Length == 0) return;
        //        if (_input.currentRightHandGestureState == HandGestureState.Insert 
        //            && _input.currentLeftHandGestureState == HandGestureState.Insert)
        //            _outputManager.SubtractWord(false);
        //        else _outputManager.BackSpace();
        //    }
        //}
        //#endregion helpers

        //#region Unit Tests
        //#endregion Unit Tests
    }
}
