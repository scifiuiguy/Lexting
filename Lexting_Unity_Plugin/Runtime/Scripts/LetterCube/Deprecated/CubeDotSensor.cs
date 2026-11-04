using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.LetterCube
{
    public class CubeDotSensor : Singleton<CubeDotSensor>
    {
        private float _dotX, _dotY, _dotZ;

        // start must be level with the ground plane before becoming an input here
        public int[] ConvertDirectionToGridPosition(Vector3 handDirection, Transform start)
        {
            int[] output = new int[3] { 0, 0, 0 };

            _dotX = Vector3.Dot(start.right, handDirection);
            _dotY = Vector3.Dot(start.up, handDirection);
            _dotZ = Vector3.Dot(start.forward, handDirection);
            output = new int[3] {
            ConformDotToIndex(_dotX),ConformDotToIndex(_dotY),ConformDotToIndex(_dotZ)
        };

            return output;
        }

        private int ConformDotToIndex(float dotProduct)
        {
            return Mathf.RoundToInt((dotProduct + 1) * 3 / 2);
        }
    }
}

