using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpatialGestureText.LetterCube
{
    public class ScrollMarkerDisplayHandler : Singleton<ScrollMarkerDisplayHandler>
    {
        [SerializeField] Image singleUp, doubleUp, singleDown, doubleDown;
        private Image[] images;

        private void Awake()
        {
            images = new Image[4] { singleUp,singleDown,doubleUp,doubleDown };
        }

        public void UpdateImage(ScrollState newState)
        {
            ToggleAllOff();
            switch (newState)
            {
                case ScrollState.Down:
                    singleDown.enabled = true;
                    break;
                case ScrollState.DownDouble:
                    doubleDown.enabled = true;
                    break;
                case ScrollState.Up:
                    singleUp.enabled = true;
                    break;
                case ScrollState.UpDouble:
                    doubleUp.enabled = true;
                    break;
            }
        }

        private void ToggleAllOff()
        {
            foreach (Image image in images)
                image.enabled = false;
        }
    }

    /// <summary>
    /// the state of the scroll marker display arrows based on all input variations
    /// </summary>
    public enum ScrollState
    {
        Neutral,
        Up,
        UpDouble,
        Down,
        DownDouble
    }
}
