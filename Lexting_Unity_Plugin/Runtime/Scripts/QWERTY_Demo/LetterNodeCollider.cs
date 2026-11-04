using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using SpatialGestureText.Utilities;
using Pixelplacement;
using UnityEngine.Events;

namespace SpatialGestureText.QWERTY_Demo
{
	/// <summary>
	/// Used as an identifier to allow the input system to access the letter's data quickly
	/// </summary>
	public class LetterNodeCollider : MonoBehaviour
	{
		public string letter => _letterDisplay.text;
		private Text _letterDisplay;
		private Image _letterBackground;
		private Color _neutral = new Color(255,255,255,100);
		private Color _highlight = new Color(0, 177, 255, 100);

        private void Awake()
        {
			_letterDisplay = transform.parent.GetComponent<Text>();
			_letterBackground = _letterDisplay.transform.parent.GetComponent<Image>();
			Highlight(false, null);
        }

		public void Highlight(bool on, UnityAction callback)
		{
			Tween.Color(_letterBackground, (on) ? _highlight : _neutral, 0.1f, 0,
				Tween.EaseInOut, Tween.LoopType.None, null, ()=>callback?.I());
		}
    }
}
