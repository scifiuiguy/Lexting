using SpatialGestureText.LetterCube;
using SpatialGestureText.TwoZoneCube;
using SpatialGestureText.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.Interfaces
{
	public class LextingController : MonoBehaviour, ISerializationCallbackReceiver
	{
        public float depthOffset;
        public float heightOffset;
        public Vector3 cursorOffset;

        /*[SerializeField]*/[HideInInspector] public ControllerType controllerType;
        public static ControllerType ControllerType = ControllerType.ThreeZone;
        public static IController Instance
        {
            get
            {
                // old controller is now deprecated
                //if (ControllerType == ControllerType.ThreeZone)
                //    return CubeController.Instance;
                //else 
                return TwoZoneCubeControllers.Instance;
            }
        }

        /// <summary>
        /// returns true once both hands have stopped dragging their line tracers
        /// </summary>
        public static bool dragOver => 
            !Instance.rightDragLineHandler.isLineTracingActive && !Instance.leftDragLineHandler.isLineTracingActive
            && !Lexting.finalizer.isSpatialListActive;

        public void OnAfterDeserialize()
        {
            ControllerType = controllerType;
        }

        public void OnBeforeSerialize()
        {
            
        }

        //public static float GetDepthOffset()
        //{
        //    return depthOffset;
        //}
    }
    public enum ControllerType {
        ThreeZone,
        TwoZone
    }
}

