using Pixelplacement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.Tutorial
{
    /// <summary>
    /// Handles the visibility of the hand pose displays used during the tutorial
    /// </summary>
    public class TutorialHandHandler : MonoBehaviour
    {
        #region vars
        private SkinnedMeshRenderer skinnedMeshRenderer;
        private Color colorOn = new Color(1,1,1,0.6f), colorOff = new Color(1,1,1,0);
        private Animator thumbController;
        #endregion vars

        #region init
        private void Awake()
        {
            skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            skinnedMeshRenderer.material.color = colorOff;
            thumbController = GetComponentInChildren<Animator>();
        }
        #endregion init

        #region helper methods
        public void Fade(bool on)
        {
            Tween.Color(skinnedMeshRenderer.material, (on) ? colorOn : colorOff, 0.5f, 0);
            //Tween.ShaderColor(skinnedMeshRenderer.material, "_MainColor", (on) ? colorOn : colorOff, 0.5f, 0);
        }

        public void MoveThumb()
        {
            thumbController.SetTrigger("MoveThumb");
        }
        #endregion helper methods
    }
}
