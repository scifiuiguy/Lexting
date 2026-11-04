//using LookingGlass;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.Input.ControllerOverrides
{
    public class LeapLKGlassControllerOverride : HandsOnlyControllerOverride
	{
        private void Awake()
        {
            if (!iWantToUseMyOwnController)
            {
                GameObject instance = Instantiate(Resources.Load("Prefabs/InputControllers/Leap_LKGlass_Input_Module", typeof(GameObject))) as GameObject;
                instance.transform.SetParent(transform);
            }
            else if (leftHand == null)
            {
                Debug.LogError("Failed to initialize Leap Controller: You have chosen to use your own controller, but a leftHand reference has not yet been assigned to this script (LeapLKGlassControllerOverride) in the Inspector.");
                Debug.Break();
            }
            else if (rightHand == null)
            {
                Debug.LogError("Failed to initialize Leap Controller: You have chosen to use your own controller, but a rightHand reference has not yet been assigned to this script (LeapLKGlassControllerOverride) in the Inspector.");
                Debug.Break();
            }

        }
    }
}
