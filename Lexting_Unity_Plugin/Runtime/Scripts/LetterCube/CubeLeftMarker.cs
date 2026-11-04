//using SpatialGestureText.Input;
//using SpatialGestureText.Interfaces;
//using SpatialGestureText.TwoZoneCube;
//using SpatialGestureText.Utilities;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using static OVRSkeleton;

//namespace SpatialGestureText.LetterCube
//{
//    /// <summary>
//    /// DEPRECATED
//    /// </summary>
//    public class CubeLeftMarker : Singleton<CubeLeftMarker>
//    {
//        public GameObject triggerActivatorPrefab;
//        public Vector3 palmNormal;
//        public Vector3 indexThumbMidpoint;
//        public Vector3 leftAverageHandPos
//        {
//            get
//            {
//#if !UNITY_EDITOR && !UNITY_STANDALONE
//                return (_leftHandSkeleton.Bones[(int)BoneId.Hand_IndexTip].Transform.position +
//                        _leftHandSkeleton.Bones[(int)BoneId.Hand_ThumbTip].Transform.position +
//                        _leftHandSkeleton.Bones[(int)BoneId.Hand_Pinky1].Transform.position +
//                        _leftHandSkeleton.Bones[(int)BoneId.Hand_WristRoot].Transform.position) / 4;
//#else
//                return Vector3.zero;
//#endif
//            }
//        }

//        [HideInInspector] public OculusLeftFingerIconSyncController iconSync;
//        [HideInInspector] public GameObject triggerActivator;

//        [SerializeField] public float horizontalOffset;

//        public IController controller;


//        private Vector3 _knuckleLineDirection, _wrist2FingertipDirection;

//        private OVRHand _leftHand;
//        private OVRSkeleton _leftHandSkeleton;
        
//        private Vector3 _newSmoothDestination;
        
//        /// <summary>
//        /// if true, no controller is present because this instance is just the input stream for a different client
//        /// </summary>
//        [SerializeField] private bool _remoteControlActive;

//        [SerializeField] private bool _isUsingTwoZone;
        
//        private void Awake()
//        {
//            controller = LextingController.Instance;
//            triggerActivator = Instantiate(triggerActivatorPrefab);
//            //this.Ch(ref _leftHand, ref _leftHandSkeleton);
//            _leftHand = transform.GetComponentInChildren<OVRHand>();
//            _leftHandSkeleton = transform.GetComponentInChildren<OVRSkeleton>();
//        }
        
//        public void UpdateSmoothDestination(Vector3 newDestination)
//        {
//            _newSmoothDestination = newDestination;
//        }

//        private void Update()
//        {
//#if !UNITY_EDITOR  && !UNITY_STANDALONE
//            if (!_remoteControlActive && triggerActivator != null)
//            {
//                triggerActivator.transform.position = Vector3.Lerp(
//                triggerActivator.transform.position,
//                leftAverageHandPos + controller.transform.right * horizontalOffset + controller.transform.forward * 0.2f,
//                CubeRightMarker.handNoiseDampeningFactor);

//                triggerActivator.transform.position = Vector3.Lerp(
//                    triggerActivator.transform.position,
//                    leftAverageHandPos + controller.transform.right * horizontalOffset + controller.transform.forward * 0.2f,
//                    CubeRightMarker.handNoiseDampeningFactor);   
//            }

//            _knuckleLineDirection = (_leftHandSkeleton.Bones[(int)BoneId.Hand_Index1].Transform.position - _leftHandSkeleton.Bones[(int)BoneId.Hand_Pinky1].Transform.position).normalized;
//            _wrist2FingertipDirection = (_leftHandSkeleton.Bones[(int)BoneId.Hand_WristRoot].Transform.position - _leftHandSkeleton.Bones[(int)BoneId.Hand_Middle1].Transform.position).normalized;
//            palmNormal = -1 * Vector3.Cross(_knuckleLineDirection, _wrist2FingertipDirection);

//            if (!_remoteControlActive)
//                iconSync.percentage = Mathf.Abs(
//                                                (
//                                                    _leftHandSkeleton.Bones[(int)BoneId.Hand_ThumbTip].Transform.position - 
//                                                    _leftHandSkeleton.Bones[(int)BoneId.Hand_IndexTip].Transform.position
//                                                ).magnitude
//                                            )/0.1f;
//#endif
            
//            if (_remoteControlActive || Lexting.inputManager.isDesktop)
//            {
//                transform.position = Vector3.Lerp(transform.position, _newSmoothDestination, 0.01f);
//                triggerActivator.transform.position = transform.position;
//            }
//        }
//    }
//}

