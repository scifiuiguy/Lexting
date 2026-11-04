using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

namespace SpatialGestureText.TwoZoneCube
{
	public class WordPagingNode : MonoBehaviour
	{
		#region vars
		#region public vars
		public bool nodeOn;
		#endregion public vars
		#region private vars
		[SerializeField] private Sprite _offImage;
		[SerializeField] private Sprite _onImage;
		private Image _nodeDisplay;
		#endregion private vars
		#endregion vars

		#region init
		private void Awake()
		{
			this.Ch(ref _nodeDisplay);
			RegisterAllEventsAndActions();
		}
		#endregion init

		#region loops

		#endregion loops

		#region event handlers
		private void RegisterAllEventsAndActions()
		{
			
		}
		#endregion event handlers

		#region helper methods
		public void ToggleNodeDisplay(bool on)
		{
			_nodeDisplay.sprite = (on) ? _onImage : _offImage;
			nodeOn = on;
		}

		public void ToggleNodeDisplay()
		{
			nodeOn = !nodeOn;
			_nodeDisplay.sprite = (nodeOn) ? _onImage : _offImage;
		}
		#endregion helper methods
		
		#region Unit Tests
		
		#endregion Unit Tests
	}
}
