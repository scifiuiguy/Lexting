using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpatialGestureText.Debugging
{
	public class LogOutputReceiver : Singleton<LogOutputReceiver>
	{
		#region vars
		#region public vars

		#endregion public vars
		#region private vars
		private Text _text;
		#endregion private vars
		#endregion vars
		
		#region init
		private void Awake()
		{
			this.Ch(ref _text);
		}
		#endregion init

		#region loops

		#endregion loops

		#region event handlers
		public void Log(string message)
		{
			_text.text = message;
		}
		#endregion event handlers
		
		#region helper methods
		
		#endregion helper methods
		
		#region Unit Tests
		
		#endregion Unit Tests
	}
}
