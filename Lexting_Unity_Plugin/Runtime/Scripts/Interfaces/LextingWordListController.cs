using SpatialGestureText.TwoZoneCube;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.Interfaces
{
	public class LextingWordListController : MonoBehaviour
	{
        public static IWordListController Instance
        {
            get
            {
                if (LextingController.ControllerType == ControllerType.ThreeZone)
                    return SpatialListController.Instance;
                else return TwoZoneWordListController.Instance;
            }
        }
    }
}
