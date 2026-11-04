//using SpatialGestureText.Input;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace SpatialGestureText.LetterCube
//{
//    /// <summary>
//    /// DEPRECATED
//    /// </summary>
//    public class CubeSuffixTrigger : MonoBehaviour
//    {
//        #region vars
//        [HideInInspector] public static Action<string> SuffixHighlightActivated;
//        [HideInInspector] public CubeController controller;
//        public bool areSuffixesActive;
//        public static string currentRightHighlightedSuffix, currentLeftHighlightedSuffix;
//        public bool isHighlighted;
//        [SerializeField] public string suffix;

//        private int collisionTally;     
//        private SpatialGestureInput _input;
//        #endregion vars

//        #region init
//        private void Awake()
//        {
//            _input = SpatialGestureInput.Instance;
//        }

//        private void OnDisable()
//        {
//            if (isHighlighted)
//                ResetTrigger();
//        }
//        #endregion init
        

//        #region event handlers
//        public void OnTriggerEnter(Collider other)
//        {
//            collisionTally++;
//            if (isHighlighted) return;
//            if (other.tag == "Activator_Right")
//            {
//                if (controller.hand == CubeHand.Left) return;
//                ToggleHighlightOn(true);
//                currentRightHighlightedSuffix = suffix;
//            }
//            else if (other.tag == "Activator_Left")
//            {
//                if (controller.hand == CubeHand.Right) return;
//                ToggleHighlightOn(false);
//                currentLeftHighlightedSuffix = suffix;
//            }

//            string audioSourceName = (transform.name == "111") ? "HoverClick_hard" : "HoverClick_soft";
//            CubeAudioManager.Instance.Play(audioSourceName);
//        }

//        public void OnTriggerExit(Collider other)
//        {
//            if (collisionTally > 0)
//                collisionTally--;

//            if (collisionTally == 0 && isHighlighted && !CubeWordAssemblyController.Instance.isWordScrollActive)
//            {
//                if ((controller.hand == CubeHand.Right && other.tag.Equals("Activator_Right")))
//                {
//                    ResetTrigger();
//                    currentRightHighlightedSuffix = "";
//                }
//                else if (controller.hand == CubeHand.Left && other.tag.Equals("Activator_Left"))
//                {
//                    ResetTrigger();
//                    currentLeftHighlightedSuffix = "";
//                }
//            }
//        }
//        #endregion event handlers

//        #region helpers
//        private void ToggleHighlightOn(bool right)
//        {
//            isHighlighted = true;
//            if (right)
//            {
//                //if (CubeController.currentRightTrigger != null)
//                //    CubeController.currentRightTrigger.ResetCharDisplay();
//                CubeController.currentRightSuffixTrigger = this;
//                currentRightHighlightedSuffix = suffix;
//                SuffixHighlightActivated?.Invoke(suffix);
//            }
//            else
//            {
//                //if (CubeController.currentLeftTrigger != null)
//                //    CubeController.currentLeftTrigger.ResetCharDisplay();
//                CubeController.currentLeftSuffixTrigger = this;
//                SuffixHighlightActivated?.Invoke(suffix);
//                currentLeftHighlightedSuffix = suffix;
//            }
//            transform.GetChild(0).gameObject.SetActive(true); // highlight mesh
//            transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
//        }

//        public void ResetTrigger()
//        {
//            transform.GetChild(0).gameObject.SetActive(false); // highlight mesh
//            //Debug.Log("Turning off " + transform.GetChild(0).name);
//            transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
//            isHighlighted = false;
//            collisionTally = 0;
//        }
//        #endregion helpers
//    }
//}
