using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText
{
    /// <summary>
    /// Handles visibility for the toggle keys (alt, shift, and ctrl) at the bottom of thUI
    /// </summary>
    public class ToggleKeyHandler : Singleton<ToggleKeyHandler>
    {
        private CanvasGroup[] _groups;

        public enum ToggleKeyType
        { 
            alt, shift, ctrl
        }

        private void Awake()
        {
            this.Chs(ref _groups);
        }

        public void ToggleVisibility(ToggleKeyType type, bool on)
        {
            _groups[(int)type].alpha = (on) ? 1 : 0;
        }
    }
}
