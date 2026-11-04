using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.LetterCube
{
    /// <summary>
    /// DEPRECATED
    /// Informs all classes whether the hand is within the bounds of its letter cube
    /// </summary>
    public class CubeBoundary : MonoBehaviour
    {
        //[SerializeField] private CubeHand hand;
        //private BoxCollider collider;

        //private void Awake()
        //{
        //    collider = GetComponent<BoxCollider>();
        //}

        //private void OnDisable()
        //{
        //    if (CubeController.Instance != null)
        //    {
        //        if (hand==CubeHand.Right)
        //            CubeController.Instance.rightHandInBounds = false;

        //        if (hand==CubeHand.Left)
        //            CubeController.Instance.leftHandInBounds = false;
        //    }
        //}

        //private void OnTriggerEnter(Collider other)
        //{
        //    if (other.tag == "Activator_Right")
        //        CubeController.Instance.rightHandInBounds = true;

        //    if (other.tag == "Activator_Left")
        //        CubeController.Instance.leftHandInBounds = true;
        //}

        //private void OnTriggerExit(Collider other)
        //{
        //    if (other.tag == "Activator_Right")
        //        CubeController.Instance.rightHandInBounds = false;

        //    if (other.tag == "Activator_Left")
        //        CubeController.Instance.leftHandInBounds = false;
        //}

        //public void UpdateBoundaryHeightOffset(float newOffsetAddition)
        //{
        //    collider.center += new Vector3(0, newOffsetAddition*GetComponentInChildren<CubeTrigger>().transform.localScale.y, 0);
        //}
    }


}
