using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using UnityEngine;
using System.Runtime.InteropServices;
using SpatialGestureText.LetterCube;
using UniRx;
using UniRx.Triggers;
using SpatialGestureText.Input;
using SpatialGestureText.Utilities;

namespace SpatialGestureText.Streaming
{
	public class MouseController : MonoBehaviour
	{
        public bool unitTest_Simulator;

        [Flags]
        public enum MouseEventFlags
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Move = 0x00000001,
            Absolute = 0x00008000,
            RightDown = 0x00000008,
            RightUp = 0x00000010
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out MousePoint lpMousePoint);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        
        public static void SetCursorPosition(MousePoint point)
        {
            SetCursorPos(point.X, point.Y);
        }

        public static MousePoint GetCursorPosition()
        {
            MousePoint currentMousePoint;
            var gotPoint = GetCursorPos(out currentMousePoint);
            if (!gotPoint) { currentMousePoint = new MousePoint(0, 0); }
            return currentMousePoint;
        }

        public static void MouseEvent(MouseEventFlags value)
        {
            MousePoint position = GetCursorPosition();

            mouse_event
                ((int)value,
                 position.X,
                 position.Y,
                 0,
                 0)
                ;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MousePoint
        {
            public int X;
            public int Y;

            public MousePoint(int x, int y)
            {
                X = x;
                Y = y;
            }
        }
        
        #region vars
        #region public vars
        public static Pingleton<MouseController> _ = new Pingleton<MouseController>();
        public SpatialGestureEvent<bool, bool> OnMouseControllerActive = new SpatialGestureEvent<bool, bool>();
        #endregion public vars
        #region private vars
        private float mouseSensitivity = 0.1f;

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [SerializeField] private bool unitTest_SetCursorPos;

        private bool _rightMouseActive, _leftMouseActive;

        private GameObject _cubeCenterMarker;
        private GameObject _currentCursorOrigin;
        private Vector2 _startingCursorPos;
        private Vector3 _currentRelativeHandPos;
        private Vector3 _previousRelativeHandPos;
        private float _noiseDampener = 0.1f;
        private Vector2 _currentRelativeCursorPos;
        private IDisposable cursorUpdateLoop;
        #endregion private vars
        #endregion vars

        #region init
        private void Awake()
		{
            _currentCursorOrigin = new GameObject("CubeCenterMouseControllerMarker");
            _startingCursorPos = UnityEngine.Input.mousePosition;

            // event handlers:
            HandleRotationState();
            HandleMousePosition();
            HandleMouseClick();
		}

        private void Start()
        {
            
        }
        #endregion init

        #region loops
        //private void Update()
        //{
        //    if (unitTest_SetCursorPos)
        //    {
        //        unitTest_SetCursorPos = false;
        //        SetCursorPos((int)(Screen.width * 0.5f), (int)(Screen.height * 0.5f));
        //    }

        //    if (unitTest_Simulator)
        //    {
        //        unitTest_Simulator = false;
        //        this.WaitS(2, () => {
        //            LextingOutputManager.Instance.windowsKeySim.Keyboard.TextEntry("test");
        //        });
                
        //    }
        //}
        #endregion loops
        
        #region event handlers
        private void HandleRotationState()
        {
            this.GetGestures(
                (right,current,previous)=> { /*gestures*/
                    mouseSensitivity = (current == HandGestureState.SqueezeAll) ? 1 : 0.1f;
                    if (current == HandGestureState.SqueezeAll)
                        _startingCursorPos = new Vector2(
                                                    _startingCursorPos.x + (_currentRelativeHandPos.x / mouseSensitivity) * (Screen.width / 2),
                                                    _startingCursorPos.y - (_currentRelativeHandPos.y / mouseSensitivity) * (Screen.height / 2));
                }, 
                (right, current, previous) => { // rotation states
                    if (((right) ? _rightMouseActive : _leftMouseActive) && MouseActivationGestureIsNotValid(right, current))
                    {
                        if (right) _rightMouseActive = false;
                        else _leftMouseActive = false;
                        OnMouseControllerActive?.I(right,false);
                    }
                    else if (MouseActivationGestureIsValid(right, current))
                    {
                        if (right) _rightMouseActive = true;
                        else _leftMouseActive = true;
                        _startingCursorPos = new Vector2(   
                                                    _startingCursorPos.x + (_currentRelativeHandPos.x / mouseSensitivity) * (Screen.width / 2),
                                                    _startingCursorPos.y - (_currentRelativeHandPos.y / mouseSensitivity) * (Screen.height / 2));
                        OnMouseControllerActive?.I(right, true);
                    }
                }
            );
        }

        private void HandleMousePosition()
        {
            this.Subscribe(OnMouseControllerActive, (right, on) => {
                _currentCursorOrigin.transform.position =
                    (right) ? Lexting.rHandMarker.cursorAnchor.transform.position : Lexting.lHandMarker.cursorAnchor.transform.position;
                if (on)
                {
                    cursorUpdateLoop = this.UpdateAsObservable().Subscribe(_ =>
                    {
                        _previousRelativeHandPos = _currentRelativeHandPos;
                        _currentRelativeHandPos = Vector3.Lerp(_previousRelativeHandPos,_currentCursorOrigin.transform.InverseTransformPoint(
                            (right) ? Lexting.rHandMarker.cursorAnchor.transform.position : Lexting.lHandMarker.cursorAnchor.transform.position), _noiseDampener);

                        SetCursorPos(
                            (int)(_startingCursorPos.x + (_currentRelativeHandPos.x / mouseSensitivity) * (Screen.width / 2)),
                            (int)(_startingCursorPos.y - (_currentRelativeHandPos.y / mouseSensitivity) * (Screen.height / 2))
                        );
                    });
                }
                else cursorUpdateLoop?.Dispose();
            });
        }

        private void HandleMouseClick()
        {
            this.GetGestures((right,current,previous) => {
                if ((right && _rightMouseActive) || (!right && _leftMouseActive))
                {
                    if (current == HandGestureState.Insert)
                    {
                        MouseEvent(MouseEventFlags.LeftDown);
                        Lexting.audio.Play("SelectionClick");
                    }
                    else if (current == HandGestureState.Neutral && previous == HandGestureState.Insert)
                    {
                        MouseEvent(MouseEventFlags.LeftUp);
                    }
                }
            });
        }
        #endregion event handlers

        #region helper methods
        private bool MouseActivationGestureIsValid(bool right, CubeRotationState current)
        {
            return !((right) ? _rightMouseActive : _leftMouseActive) && current == CubeRotationState.forward &&
                ((right && Lexting.lhRotState == CubeRotationState.other) || (!right && Lexting.lhRotState == CubeRotationState.other));
        }

        private bool MouseActivationGestureIsNotValid(bool right, CubeRotationState current)
        {
            return current != CubeRotationState.forward || Lexting.lhRotState != CubeRotationState.other;
        }
        #endregion helper methods
    }
}
