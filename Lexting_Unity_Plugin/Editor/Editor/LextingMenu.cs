using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SpatialGestureText.Editor
{
	public class LextingMenu : EditorWindow
	{
		[MenuItem("Lexting/Settings")]
		static void Init()
		{
			LextingMenu window = (LextingMenu)EditorWindow.GetWindow(typeof(LextingMenu), true, "Lexting Settings");
			window.Show();
		}

		void OnGUI()
		{
			EditorGUILayout.LabelField("this is the Lexting Settings Menu - need to create a toggle for making descriptors writable here");
			this.Repaint();
		}

		#region vars
		#region public vars

		#endregion public vars
		#region private vars

		#endregion private vars
		#endregion vars

		#region init
		private void Awake()
		{
			
		}
		#endregion init
	
		#region loops
		
		#endregion loops
		
		#region event handlers
		
		#endregion event handlers
		
		#region helper methods
		
		#endregion helper methods
		
		#region Unit Tests
		
		#endregion Unit Tests
	}
}
