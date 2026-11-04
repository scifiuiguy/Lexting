using SpatialGestureText.Input;
using SpatialGestureText.LetterCube;
using SpatialGestureText.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Deprecated
/// </summary>
public class OculusRightFingerIconSyncController : MonoBehaviour
{
    [SerializeField] private Transform _joint1, _joint2, _joint3;
    [SerializeField] private float _joint1LowerRange, _joint1UpperRange, _joint2LowerRange, _joint2UpperRange, _joint3LowerRange, _joint3UpperRange;

    private void Awake()
    {
        if (Lexting.inputManager.mode != InputMode.QUEST_HANDS) return;
        //Lexting.rHand.iconSync = this;
    }

    // distance between thumb and pointer will update this from the CubeLeftMarker and CubeRightMarker respectively
    public float percentage
    {
        set
        {
            _joint1.localRotation = Quaternion.Euler(_joint1.localRotation.eulerAngles.x, _joint1.localRotation.eulerAngles.y, (_joint1UpperRange - _joint1LowerRange) * value + _joint1LowerRange);
            _joint2.localRotation = Quaternion.Euler(_joint2.localRotation.eulerAngles.x, _joint2.localRotation.eulerAngles.y, (_joint2UpperRange - _joint2LowerRange) * value + _joint2LowerRange);
            _joint3.localRotation = Quaternion.Euler(_joint3.localRotation.eulerAngles.x, _joint3.localRotation.eulerAngles.y, (_joint3UpperRange - _joint3LowerRange) * value + _joint3LowerRange);
        }
    }
}
