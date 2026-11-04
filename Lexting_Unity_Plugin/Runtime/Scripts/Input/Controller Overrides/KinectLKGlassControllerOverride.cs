using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.Input.ControllerOverrides
{
	public class KinectLKGlassControllerOverride : HandsOnlyControllerOverride
	{
        private void Awake()
        {
            if (!iWantToUseMyOwnController)
            {
                GameObject instance = Instantiate(Resources.Load("Prefabs/InputControllers/Kinect_Input_Module", typeof(GameObject))) as GameObject;
                instance.transform.SetParent(transform);
            }
            else if (leftHand == null)
                Debug.Log("Failed to initialize Kinect Controller because left hand reference was not found.");
            else if (rightHand == null)
                Debug.Log("Failed to initialize Kinect Controller because right hand reference was not found.");
        }
    }
}
