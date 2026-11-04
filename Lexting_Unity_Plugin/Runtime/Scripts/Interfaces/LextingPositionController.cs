using SpatialGestureText.LetterCube;
using SpatialGestureText.TwoZoneCube;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.Interfaces
{
	public static class LextingPositionController
	{
        public static IPositionController Instance
        {
            get
            {
                if (LextingController.ControllerType == ControllerType.ThreeZone)
                    return CubePositionController._.Instance;
                else return TwoZoneCubePositionController.Instance;
            }
        }
    }
}
