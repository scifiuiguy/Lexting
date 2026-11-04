using Pixelplacement;
using SpatialGestureText.Input;
using SpatialGestureText.Zones;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using SpatialGestureText.Utilities;
using SpatialGestureText.Interfaces;

namespace SpatialGestureText.LetterCube
{
    /// <summary>
    /// Controls the draggable axis handles that allow the user to edit the position of the CubeController
    /// </summary>
    public class CubePositionController : ZoneController, IPositionController
    {
        #region vars
        private Dictionary<Axis, CubePositionRail> _axisRails = new Dictionary<Axis, CubePositionRail>();
        private Axis _currentDragAxis;
        private int _currentDragIndex;

        private Transform _currentHandMarkerTransform;
        private GameObject _startingHandMarker;
        private float _axisDelta;

        [SerializeField] Transform _targetCubeMotionTransform;
        private Vector3 _startingCubePosition;
        private Vector3 _lookTarget;
        private Vector3 _lookDirection;
        private Vector3 _targetPosition;

        /// <summary>
        /// default distance between the cube and the user's chest
        /// </summary>
        [SerializeField] private float _zDistance;

        private Vector3 _startingHeadOffsetDirection;
        private float _startingHeadOffsetDistance;

        /// <summary>
        /// offset created by user dragging the X position handle
        /// </summary>
        private float _xOffset;


        /// <summary>
        /// offset created by user dragging the Y position handle
        /// </summary>
        private float _yOffset;

        /// <summary>
        /// offset created by user dragging the Z position handle
        /// </summary>
        private float _zOffset;

        /// <summary>
        /// Cached value of _xOffset from the last time y handle was dragged
        /// </summary>
        private float _previousXOffset;


        /// <summary>
        /// Cached value of _yOffset from the last time y handle was dragged
        /// </summary>
        private float _previousYOffset;

        /// <summary>
        /// Cached value of _zOffset from the last time z handle was dragged
        /// </summary>
        private float _previousZOffset;

        /// <summary>
        /// A loop that updates the position of the cube each frame while the cube controller is activated
        /// </summary>
        private IDisposable bodyTrackingLoop;

        /// <summary>
        /// the interfaceOrigin's initial y offset from the height of the user's head
        /// </summary>
        [SerializeField] private float _headHeightOffset = -0.4f;

        /// <summary>
        /// the interfaceOrigin's initial x offset relative to the head
        /// </summary>
        [SerializeField] private float _headLateralOffset = -0.05f;

        [SerializeField] private float _lookDirectionDampeningFactor = 0.1f;
        [SerializeField] private float _cubeMotionDampeningFactor = 0.05f;


        /// <summary>
        /// True if the cube controller is within a 20 degree angular range of gaze direction
        /// </summary>
        [SerializeField] private bool _isInGazeRange;

        /// <summary>
        /// Angular gaze range percentage where 1.0f is user looking at the cube controller straight on,
        /// 0f is 90 degrees looking away, and -1.0f is 180 degrees looking away
        /// </summary>
        private ReactiveProperty<float> _gazeRange = new ReactiveProperty<float>();
        //public float gazeRangeVisualizer;



        /// <summary>
        /// the threshold across which gaze is considered in/out of range 
        /// </summary>
        [Range(-1.0f, 1.0f)]
        [SerializeField] private float _gazeRangeThreshold = 0.001f;



        private const float _gazeThresholdMin = 0.001f, _gazeThresholdMax = 0.2f;

        private IDisposable _rangeListener;

        private float _distanceFromHead;

        // [SerializeField] private bool unitTest_RangeUp, unitTest_RangeDown;

        public bool hasHover {
            get {
                return hoverIndex >= 0;
            }
        }

        public static Pingleton<CubePositionController> _ = new Pingleton<CubePositionController>();

        /// <summary>
        /// the point in front of the body that always represents the cube's position if it has no y or z offsets
        /// </summary>
        [HideInInspector] public GameObject interfaceOrigin;

        public bool isDragging {
            get {
                return _isRightDragging || _isLeftDragging;
            }
            private set { isDragging = value; }
        } 

        public SpatialGestureEvent<bool,bool,Transform> OnCubeDragging = new SpatialGestureEvent<bool,bool,Transform>();
        #endregion vars

        #region init
        protected override void Awake()
        {
            if (LextingController.Instance.isInputStreamReceiver)
            {
                foreach (Transform child in transform)
                    transform.gameObject.SetActive(false);
                return;
            }
            base.Awake();
            CubePositionRail[] rails = new CubePositionRail[3];
            this.ChsFE(ref rails, (rail) => _axisRails.Add(rail.axis, rail));
            _startingHandMarker = new GameObject("CubePositionController_StartingHandMarker");
            ResetStartingHandMarker();
#if UNITY_EDITOR || UNITY_STANDALONE
            InitStaticPosition();
#else 
            SyncWithTarget();
#endif
            _lookDirection = HeadMarker.Instance.transform.forward;

            interfaceOrigin = new GameObject("LextingInterfaceOrigin");
            //AddOriginDebugVisualizer();

            RegisterEvents();



            //_targetCubeMotionTransform.ObserveEveryValueChanged(x=>x.forward).Where
        }

        private void InitStaticPosition()
        {
            _targetCubeMotionTransform.position = transform.position;
        }
        #endregion init

        #region loops
        //private void Update()
        //{
        //    if (unitTest_RangeUp)
        //    {
        //        unitTest_RangeUp = false;
        //        _gazeRange.Value = 0.3f;
        //    }

        //    if (unitTest_RangeDown)
        //    {
        //        unitTest_RangeDown = false;
        //        _gazeRange.Value = 0.1f;
        //    }
        //}
        #endregion loops

        #region event handlers
        private void RegisterEvents()
        {
            this.Subscribe(LextingController.Instance.ControllerActivated, (on) => {
                if (on)
                    bodyTrackingLoop = this.UpdateAsObservable().Subscribe(_ => {
                        _lookDirection = new Vector3(HeadMarker.Instance.transform.forward.x, 0, HeadMarker.Instance.transform.forward.z);
                        FollowUser();
                        _gazeRange.Value = 
                            Vector3.Dot(
                                (new Vector3(_targetCubeMotionTransform.position.x, HeadMarker.Instance.transform.position.y, _targetCubeMotionTransform.position.z) 
                                    - HeadMarker.Instance.transform.position).normalized, _lookDirection.normalized
                            );
                        //gazeRangeVisualizer = _gazeRange.Value;
                        _distanceFromHead = Mathf.Abs((HeadMarker.Instance.transform.position - _targetCubeMotionTransform.position).magnitude);
                    });
                else bodyTrackingLoop.Dispose();
            });

            StartCoroutine(ListenForRangeChange());
        }

        private IEnumerator ListenForRangeChange()
        {
            while (true)
            {
                yield return new WaitForSeconds(2);
                _rangeListener?.Dispose();
                _rangeListener = _gazeRange.Where(x => x >= 1 - _gazeRangeThreshold).Take(1).Subscribe(_ => _isInGazeRange = true);
                yield return new WaitUntil(() => _isInGazeRange);
                Tween.Value(_gazeRangeThreshold, _gazeThresholdMax, (currentThreshold) => _gazeRangeThreshold = currentThreshold, 0.5f, 0, Tween.EaseInOut);
                yield return new WaitForSeconds(2);
                _rangeListener.Dispose();
                _rangeListener = _gazeRange.Where(x => x < 1 - _gazeRangeThreshold).Take(1).Subscribe(_ => _isInGazeRange = false);
                yield return new WaitUntil(() => !_isInGazeRange && _distanceFromHead > 0.3f);
                _gazeRangeThreshold = _gazeThresholdMin;
            }
        }

        protected override void OnGesturesChanged(bool right, HandGestureState current, HandGestureState previous)
        {
            if (hoverIndex >= 0 && current == HandGestureState.Insert)
            {
                this.WaitS(0.1f, () => {
                    if (ShouldBeginDrag(right))
                        OnZoneDragBegan(right);
                });
            }
            else if (ShouldEndDrag(current))
                OnZoneDragEnded();
        }

        public override void OnZoneDragBegan(bool right)
        {
#if !UNITY_STANDALONE
            base.OnZoneDragBegan(right);
            //if (_isRightDragging || _isLeftDragging) return;
            //_startingCubePosition = _targetCubeMotionTransform.position;
            _currentHandMarkerTransform = Lexting.hMarker(right).transform;
            _startingHandMarker.transform.SetParent(null);
            _startingHandMarker.transform.position = _currentHandMarkerTransform.position;
            _startingHandMarker.transform.SetParent(interfaceOrigin.transform);
            _currentDragIndex = hoverIndex;
            _currentDragAxis = GetAxisByIndex();
            Debug.Log($"CubePositionController - OnZoneDragBegan - _currentDragAxis: {_currentDragAxis} - _currentDragIndex: {_currentDragIndex}");
            _startingHandMarker.transform.rotation = _axisRails[_currentDragAxis].transform.rotation;
            //if (_currentDragAxis == Axis.x)
            //_zDistance = Mathf.Abs((_targetCubeMotionTransform.position - 
            //    new Vector3(HeadMarker.Instance.transform.position.x, _targetCubeMotionTransform.position.y, HeadMarker.Instance.transform.position.z)).magnitude);
            OnCubeDragging?.I(true, right, _axisRails[_currentDragAxis].transform);
#endif
        }

        public override void OnZoneDragging(bool right)
        {
            base.OnZoneDragging(right);
            _axisDelta = _startingHandMarker.transform.InverseTransformPoint(_currentHandMarkerTransform.position).z; // this z is relative to each axis

            switch (_currentDragAxis)
            {
                case Axis.x:
                    _xOffset = _axisDelta + _previousXOffset;
                    break;
                case Axis.y:
                    _yOffset = _axisDelta + _previousYOffset;
                    break;
                case Axis.z:
                    _zOffset = _axisDelta + _previousZOffset;
                    break;
            }
            UpdateCubePosition();
        }

        public override void OnZoneDragEnded()
        {
            _previousYOffset = _yOffset;
            _previousZOffset = _zOffset;
            ResetStartingHandMarker();

            OnCubeDragging?.I(false, _isRightDragging, _axisRails[_currentDragAxis].transform);
            base.OnZoneDragEnded();            
        }
#endregion event handlers

#region helper methods
        private bool ShouldBeginDrag(bool right)
        {
            return (right && SpatialGestureInput.Instance.currentRightHandGestureState == HandGestureState.Insert) ||
                        (!right && SpatialGestureInput.Instance.currentLeftHandGestureState == HandGestureState.Insert);
        }

        private bool ShouldEndDrag(HandGestureState current)
        {
            return current != HandGestureState.Insert && (_isRightDragging || _isLeftDragging);
        }

        private Axis GetAxisByIndex()
        {
            foreach (var pair in _axisRails)
            {
                if (pair.Value.zoneIndex == _currentDragIndex)
                    return pair.Value.axis;
            }
            return Axis.x;
        }

        private Vector3 GetAxisDirection()
        {
            switch (_currentDragAxis)
            {
                case Axis.x: return _targetCubeMotionTransform.right;
                case Axis.y: return _targetCubeMotionTransform.up;
                case Axis.z: return _targetCubeMotionTransform.forward;
                default: return _targetCubeMotionTransform.forward;
            }
        }

        private void ResetStartingHandMarker()
        {
            _startingHandMarker.transform.SetParent(transform);
            _startingHandMarker.transform.position = Vector3.zero;
        }

        /// <summary>
        /// Ensure that this controller's position and rotation are always in sync with the target cube controller.
        /// </summary>
        private void SyncWithTarget()
        {
            _targetCubeMotionTransform.ObserveEveryValueChanged(x => x.position).Subscribe(x => {
                transform.position = x; });
            _targetCubeMotionTransform.ObserveEveryValueChanged(x => x.rotation).Subscribe(x => {
                transform.rotation = x; });
        }

        private void FollowUser()
        {
            if (_isInGazeRange) return;
            interfaceOrigin.transform.position = 
                Vector3.Lerp(
                    interfaceOrigin.transform.position, 
                    HeadMarker.Instance.transform.position + _zDistance * _lookDirection + _headHeightOffset * Vector3.up
                     + _headLateralOffset*Vector3.Cross(Vector3.up, _lookDirection),
                    _cubeMotionDampeningFactor
                );
            interfaceOrigin.transform.rotation = Quaternion.Lerp(interfaceOrigin.transform.rotation, Quaternion.LookRotation(_lookDirection), _lookDirectionDampeningFactor);

            if (_isRightDragging || _isLeftDragging) return;
            UpdateCubePosition();
        }

        private void UpdateCubePosition()
        {
#if !UNITY_EDITOR && !UNITY_STANDALONE
            _targetCubeMotionTransform.position =
                interfaceOrigin.transform.position + _xOffset * _targetCubeMotionTransform.right + _yOffset * Vector3.up + _zOffset * _targetCubeMotionTransform.forward;
            _targetCubeMotionTransform.rotation = interfaceOrigin.transform.rotation;
#endif
        }
#endregion helper methods

#region debugging
        void AddOriginDebugVisualizer()
        {
            GameObject debugOrigin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(debugOrigin.GetComponent<Collider>());
            debugOrigin.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            debugOrigin.transform.SetParent(interfaceOrigin.transform);
        }
#endregion debugging
    }

    public enum Axis
    {
        x,
        y,
        z
    }
}
