//using SpatialGestureText.Input;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using static OVRSkeleton;
//using UniRx;
//using UniRx.Triggers;
//using System;
//using static UnityEngine.Application;
//using SpatialGestureText.TwoZoneCube;
//using SpatialGestureText.Interfaces;
//using SpatialGestureText.Utilities;

//namespace SpatialGestureText.LetterCube
//{
//    /// <summary>
//    /// DEPRECATED
//    /// A component that attaches to the user's right hand for real-time reference in the Lexting system.
//    /// </summary>
//    public class CubeRightMarker : Singleton<CubeRightMarker>
//    {
//        public GameObject triggerActivatorPrefab;
//        public GameObject palmHoverNode;
//        public GameObject palmNode;
//        public Vector3 palmNormal;
//        public const float handNoiseDampeningFactor = 0.2f;
//        public Vector3 rightAverageHandPos
//        {
//            get
//            {
//#if !UNITY_EDITOR && !UNITY_STANDALONE
//                return (_rightHandSkeleton.Bones[(int)BoneId.Hand_IndexTip].Transform.position +
//                        _rightHandSkeleton.Bones[(int)BoneId.Hand_ThumbTip].Transform.position +
//                        _rightHandSkeleton.Bones[(int)BoneId.Hand_Pinky1].Transform.position +
//                        _rightHandSkeleton.Bones[(int)BoneId.Hand_WristRoot].Transform.position) / 4;
//#else
//                return Vector3.zero;
//#endif
//            }
//        }


//        [SerializeField] public float horizontalOffset;
//        [HideInInspector] public IController controller;
//        [HideInInspector] public GameObject triggerActivator;


//        [SerializeField] private bool _renderPalmNormalVisualizer;
//        /*[HideInInspector]*/ public OculusRightFingerIconSyncController iconSync;
//        private LineRenderer _palmNormalVisualizer;
//        private OVRHand _rightHand;
//        private OVRSkeleton _rightHandSkeleton;
//        private Vector3 _knuckleLineDirection, _wrist2FingertipDirection;

//        private Vector3 _newSmoothDestination;

//        /// <summary>
//        /// if true, no controller is present because this instance is just the input stream for a different client
//        /// </summary>
//        [SerializeField] private bool _remoteControlActive;

//        [SerializeField] private bool _isUsingTwoZone;
        
//        private void Awake()
//        {
//            controller = LextingController.Instance;
//            //CreateTriggerActivator();
//            //this.Ch(ref _rightHand, ref _rightHandSkeleton);
//            _rightHand = transform.GetComponentInChildren<OVRHand>();
//            _rightHandSkeleton = transform.GetComponentInChildren<OVRSkeleton>();
//            _palmNormalVisualizer = GetComponent<LineRenderer>();
//            _palmNormalVisualizer.enabled = _renderPalmNormalVisualizer;

//#if UNITY_EDITOR
//            // Allow WASD+up+down control of right hand for testing in editor
//            //HandleDebugButtons();
//#endif
//        }
        
//        private void Update()
//        {
//#if !UNITY_EDITOR && !UNITY_STANDALONE
//            if (!_remoteControlActive && triggerActivator != null)
//            {
//                triggerActivator.transform.position =
//                Vector3.Lerp(
//                    triggerActivator.transform.position,
//                    rightAverageHandPos + -1 * controller.transform.right * horizontalOffset + controller.transform.forward * 0.2f,
//                    handNoiseDampeningFactor
//                );
//            }
//            _knuckleLineDirection = (_rightHandSkeleton.Bones[(int)BoneId.Hand_Index1].Transform.position - 
//                                        _rightHandSkeleton.Bones[(int)BoneId.Hand_Pinky1].Transform.position).normalized;
//            _wrist2FingertipDirection = (_rightHandSkeleton.Bones[(int)BoneId.Hand_WristRoot].Transform.position - 
//                                            _rightHandSkeleton.Bones[(int)BoneId.Hand_Middle1].Transform.position).normalized;
//            palmNormal = Vector3.Cross(_knuckleLineDirection, _wrist2FingertipDirection);

//            if (_renderPalmNormalVisualizer)
//                _palmNormalVisualizer.SetPositions(new Vector3[2] { transform.position, transform.position + palmNormal });

//            if (!_remoteControlActive)
//                iconSync.percentage = Mathf.Abs(
//                                        (
//                                            _rightHandSkeleton.Bones[(int)BoneId.Hand_ThumbTip].Transform.position -
//                                            _rightHandSkeleton.Bones[(int)BoneId.Hand_IndexTip].Transform.position
//                                        ).magnitude
//                                    ) / 0.1f;
//#endif
            

//            if (_remoteControlActive || Lexting.inputManager.isDesktop)
//            {
//                transform.position = Vector3.Lerp(transform.position, _newSmoothDestination, 0.01f);
//                triggerActivator.transform.position = transform.position;
//            }
            
//        }

//        //private void CreateTriggerActivator()
//        //{
//        //    triggerActivator = Instantiate(triggerActivatorPrefab);
//        //}

//#region helper methods

//        public void UpdateSmoothDestination(Vector3 newDestination)
//        {
//            _newSmoothDestination = newDestination;
//        }

//#endregion helper methods

//#region debugging
//        private void HandleDebugButtons()
//        {
//#if UNITY_EDITOR
//            GameObject debugCamObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
//            Destroy(debugCamObject.GetComponent<Collider>());
//            debugCamObject.GetComponent<MeshRenderer>().enabled = false;
//            Camera debugCam = debugCamObject.AddComponent<Camera>();
//            debugCamObject.name = "DebugCamera";
//            debugCam.depth = 10;
//            float debugCamSpeed = 1;

//            StartCoroutine(HandleMoveButton(debugCamObject, KeyCode.W, debugCamSpeed));
//            StartCoroutine(HandleMoveButton(debugCamObject, KeyCode.A, debugCamSpeed));
//            StartCoroutine(HandleMoveButton(debugCamObject, KeyCode.S, debugCamSpeed));
//            StartCoroutine(HandleMoveButton(debugCamObject, KeyCode.D, debugCamSpeed));
//            StartCoroutine(HandleMoveButton(debugCamObject, KeyCode.UpArrow, debugCamSpeed));
//            StartCoroutine(HandleMoveButton(debugCamObject, KeyCode.DownArrow, debugCamSpeed));
//            StartCoroutine(HandleActivation(debugCamObject,KeyCode.Space));
//#endif
//        }

//#if UNITY_EDITOR
//        IEnumerator HandleMoveButton(GameObject obj, KeyCode key, float speed)
//        {
//            IDisposable update;
//            while (true)
//            {
//                update = this.UpdateAsObservable().Subscribe(_ => {/*while button up*/});
//                yield return SpatialGestureInput.GetKeyDown(key).Take(1).ToYieldInstruction();
//                update?.Dispose();
//                update = this.UpdateAsObservable().Subscribe(_ => /*while button down*/ DebugMove(key, obj, speed));
//                yield return SpatialGestureInput.GetKeyUp(key).Take(1).ToYieldInstruction();
//                update.Dispose();
//            }
//        }

//        private void DebugMove(KeyCode key, GameObject obj, float speed)
//        {
//            int direction = (key == KeyCode.W || key == KeyCode.D || key == KeyCode.UpArrow) ? 1 : -1;
//            int axis = (key == KeyCode.W || key == KeyCode.S) ? 0 : ((key == KeyCode.A || key == KeyCode.D) ? 1 : 2);
//            TranslateFromButtonInput(obj, direction, speed, axis);
//        }

//        private void TranslateFromButtonInput(GameObject obj, int direction, float speed, int axis)
//        {
//            switch (axis)
//            {
//                case 0:
//                    obj.transform.Translate(0, 0, direction * speed * Time.deltaTime, Space.Self);
//                    break;
//                case 1:
//                    obj.transform.Translate(direction * speed * Time.deltaTime, 0, 0, Space.Self);
//                    break;
//                case 2:
//                    obj.transform.Translate(0, direction * speed * Time.deltaTime, 0, Space.Self);
//                    break;
//            }
//            transform.position = obj.transform.position + obj.transform.forward;
//        }

//        private IEnumerator HandleActivation(GameObject obj, KeyCode key)
//        {
//            IDisposable update;
//            while (true)
//            {
//                update = this.UpdateAsObservable().Subscribe(_ => {});
//                yield return SpatialGestureInput.GetKeyDown(key).Take(1).ToYieldInstruction();
//                update?.Dispose();
//                //OculusHandInput.Instance.ActivateInEditor();
//            }
//        }

//#endif
//#endregion debugging
//    }
//}