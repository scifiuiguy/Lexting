using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.ImageEffects
{
    [ExecuteInEditMode]
    public class EdgeImageEffect : MonoBehaviour
	{
        public Material material;

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            Graphics.Blit(src, dest, material);
        }
    }
}
