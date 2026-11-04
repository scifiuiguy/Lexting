using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.LetterCube
{
    //deprecated
    /// <summary>
    /// Receives a callback each time the hand moves out of the letter grid to the right,
    /// acting as a letter insertion function for the the phrase editor.
    /// </summary>
    //public class CubeForwardTrigger : Singleton<CubeForwardTrigger>
    //{
    //    [HideInInspector] public Action<string> LetterSelectionActivated;
    //    private CubeLetterTrigger _letterTrigger;
    //    private string _currentHighlightedLetter;

    //    private void Awake()
    //    {
    //        _currentHighlightedLetter = "";
    //    }

    //    private void OnEnable()
    //    {
    //        CubeLetterTrigger.LetterHighlightActivated += OnLetterHighlightActivated;
    //    }

    //    private void OnDisable()
    //    {
    //        CubeLetterTrigger.LetterHighlightActivated -= OnLetterHighlightActivated;
    //    }

    //    private void OnTriggerEnter(Collider other)
    //    {
    //        // need to upgrade this to support an Activator object unique to each hand
    //        // this one defaults to right hand
    //        if (other.tag == "CubeActivator")
    //        {
    //            Debug.Log(string.Format("ForwardTrigger activated with current index: " +
    //                                    "row {0} " +
    //                                    "column {1}",
    //                                    CubeSlideRail.Instance.currentIndex.row,
    //                                    CubeSlideRail.Instance.currentIndex.column));
    //            LetterSelectionActivated?.Invoke(_currentHighlightedLetter);
    //            // "true" is right hand only - fix this later
    //            CubeController.Instance.UpdateGridPosition(
    //                                            // right
    //                                            true,
    //                                            // delta between grid start pos and current letter grid pos
    //                                            CubeTriggerFinder.Instance.GetTriggerIndexDelta(CubeSlideRail.Instance.currentIndex)
    //                                        );
    //        }
    //    }

    //    private void OnLetterHighlightActivated(string highlightedLetter)
    //    {
    //        _currentHighlightedLetter = highlightedLetter;
    //    }
    //}
}


