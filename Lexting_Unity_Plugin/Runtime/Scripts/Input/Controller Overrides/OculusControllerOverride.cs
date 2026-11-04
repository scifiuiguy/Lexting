using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.Input.ControllerOverrides
{
	public class OculusControllerOverride : ControllerOverride
	{
        private void Awake()
        {
            if (!iWantToUseMyOwnController)
            {
                GameObject instance = Instantiate(Resources.Load("Prefabs/InputControllers/Lexting_OVRCameraRig", typeof(GameObject))) as GameObject;
                instance.transform.SetParent(transform);
            }
            else if (leftHand == null)
            {
                Debug.LogError("Failed to initialize Oculus Hand Input: You have chosen to use your own controller, " +
                    "but a leftHand reference has not yet been assigned to this script (OculusControllerOverride) in the Inspector.");
                Debug.Break();
            }
            else if (rightHand == null)
            {
                Debug.LogError("Failed to initialize Leap Oculus Hand Input: You have chosen to use your own controller, " +
                    "but a rightHand reference has not yet been assigned to this script (OculusControllerOverride) in the Inspector.");
                Debug.Break();
            }
            else if (head == null)
            {
                Debug.LogError("Failed to initialize Leap Oculus Hand Input: You have chosen to use your own controller, " +
                    "but a head reference has not yet been assigned to this script (OculusControllerOverride) in the Inspector.");
                Debug.Break();
            }

        }
    }
}
