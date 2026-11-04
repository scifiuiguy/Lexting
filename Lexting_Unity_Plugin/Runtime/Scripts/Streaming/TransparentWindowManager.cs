using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.Streaming
{
    public class TransparentWindowManager : MonoBehaviour
    {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        private struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        #region DLL Import

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("Dwmapi.dll")]
        private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

        [DllImport("user32.dll")]
        private static extern int SetWindowPos(IntPtr hWnd, IntPtr hWindInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern int SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);

        #endregion DLL Import

        const int GWL_EXSTYLE = -20;
        const uint WS_EX_LAYERED = 0x00080000;
        const uint WS_EX_TRANSPARENT = 0x00000020;
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        const uint LWA_COLOR_KEY = 0x00000001;
        private IntPtr windowHandle;

        protected virtual void Start()
        {

            windowHandle = GetActiveWindow();
            MARGINS margins = new MARGINS()
            {
                cxLeftWidth = -1
            };
            DwmExtendFrameIntoClientArea(windowHandle, ref margins);
            SetWindowLong(windowHandle, GWL_EXSTYLE, WS_EX_LAYERED);
            SetWindowPos(windowHandle, HWND_TOPMOST, 0, 0, 0, 0, 0);

        }

        private void Update()
        {
            SetClickthrough(!Physics.Raycast(Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition)));
        }

        private void SetClickthrough(bool clickThrough)
        {
            if (clickThrough)
                SetWindowLong(windowHandle, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT);
            else SetWindowLong(windowHandle, GWL_EXSTYLE, WS_EX_LAYERED);
        }
#endif // !UNITY_EDITOR && UNITY_STANDALONE_WIN
    }
}
