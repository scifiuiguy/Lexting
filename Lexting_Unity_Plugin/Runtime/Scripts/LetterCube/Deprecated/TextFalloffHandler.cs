using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpatialGestureText.LetterCube
{
    public class TextFalloffHandler : MonoBehaviour
    {
        private Material mat;

        public void SetFalloff()
        {
            mat = GetComponent<Text>().material;
            mat.SetFloat("_UpperFalloffHeight", transform.parent.position.y + 0.1f);
            mat.SetFloat("_LowerFalloffHeight", transform.parent.position.y - 0.4f);
        }

    }
}

