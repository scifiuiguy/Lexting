using SpatialGestureText.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpatialGestureText.LetterCube
{
    [RequireComponent(typeof(Text))]
    public class ZoneTextOutputReceiver : MonoBehaviour
    {
        private Text receiver;
        private void Awake()
        {
            receiver = GetComponent<Text>();
        }

        private void OnEnable()
        {
            Lexting.printer.RegisterOutput(ref receiver);
        }

        private void OnDisable()
        {
            Lexting.printer.UnregisterOutput(ref receiver);
        }
    }
}