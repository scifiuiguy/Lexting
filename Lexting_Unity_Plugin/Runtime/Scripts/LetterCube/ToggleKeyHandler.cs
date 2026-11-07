using SpatialGestureText.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace SpatialGestureText
{
    /// <summary>
    /// Handles visibility for the toggle keys (alt, shift, and ctrl) at the bottom of thUI
    /// </summary>
    public class ToggleKeyHandler : Singleton<ToggleKeyHandler>
    {
        private const string ModifierKeysResourceRoot = "ModifierKeys";

        private CanvasGroup[] _groups;

        public enum ToggleKeyType
        { 
            alt, shift, ctrl
        }

        private void Awake()
        {
            this.Chs(ref _groups);
            BindModifierKeySprites();
        }

        /// <summary>
        /// Scene Image refs used legacy sprite GUIDs that are not reliable in the UPM layout; load from Resources at runtime.
        /// </summary>
        private void BindModifierKeySprites()
        {
            foreach (var image in GetComponentsInChildren<Image>(true))
            {
                var sprite = image.gameObject.name switch
                {
                    "Alt" => Resources.Load<Sprite>($"{ModifierKeysResourceRoot}/alt"),
                    "Shift" => Resources.Load<Sprite>($"{ModifierKeysResourceRoot}/shift"),
                    "Ctrl" => Resources.Load<Sprite>($"{ModifierKeysResourceRoot}/ctrl"),
                    _ => null
                };
                if (sprite != null)
                    image.sprite = sprite;
            }
        }

        public void ToggleVisibility(ToggleKeyType type, bool on)
        {
            _groups[(int)type].alpha = (on) ? 1 : 0;
        }
    }
}
