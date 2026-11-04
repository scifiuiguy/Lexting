using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.LetterCube
{
    // deprecated
    //public class CubeBackTrigger : Singleton<CubeBackTrigger>
    //{
    //    [HideInInspector] public Action MoveSenderCursorBack;

    //    private void OnTriggerEnter(Collider other)
    //    {
    //        if (other.tag == "CubeActivator")
    //        {
    //            Debug.Log(string.Format("BackTrigger activated with current index: " +
    //                                    "row {0} " +
    //                                    "column {1}",
    //                                    CubeSlideRail.Instance.currentIndex.row,
    //                                    CubeSlideRail.Instance.currentIndex.column));
    //            MoveSenderCursorBack?.Invoke();
    //            CubeController.Instance.UpdateGridPosition(
    //                                                // right hand
    //                                                true,
    //                                                // delta between grid start pos and current letter grid pos
    //                                                CubeTriggerFinder.Instance.GetTriggerIndexDelta(CubeSlideRail.Instance.currentIndex)
    //                                            );
    //        }

    //    }
    //}
}
