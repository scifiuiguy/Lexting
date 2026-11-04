using SpatialGestureText.LetterCube;
using SpatialGestureText.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioUnitTest : MonoBehaviour
{
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Lexting.audio.Play("Hoverclick_Soft");
    }
}
