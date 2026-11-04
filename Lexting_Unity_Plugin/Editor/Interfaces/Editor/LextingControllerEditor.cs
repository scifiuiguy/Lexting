using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

namespace SpatialGestureText.Interfaces.Editor
{
    /// <summary>
    /// Exposes top-level attributes to developers attempting to integrate the Lexting interface into their projects
    /// </summary>
	[CustomEditor(typeof(LextingController), true)]
	public class LextingControllerEditor : UnityEditor.Editor
	{

        private Texture2D spawnGraphic;
        private Texture2D cursorOffsetGraphic;
        private LextingController controller;

        
        GUIStyle fieldStyle = new GUIStyle();
        

        private void OnEnable()
        {
            cursorOffsetGraphic = Resources.Load("Cursor_Offset_Diagram") as Texture2D;
            spawnGraphic = Resources.Load("lexting_info_ui_spawn_point") as Texture2D;
            controller = (LextingController)target;

           
            
            //fieldStyle.alignment = TextAnchor.MiddleLeft;
            //fieldStyle.fixedWidth = 100;
            fieldStyle.alignment = TextAnchor.MiddleLeft;
        }

        public override void OnInspectorGUI()
        {
            //GUILayout.Space(650);
            GUI.DrawTexture(new Rect(20, 320, 400, 350), cursorOffsetGraphic, ScaleMode.StretchToFill, true, 10.0F);
            GUI.DrawTexture(new Rect(20, 10, 250, 300), spawnGraphic, ScaleMode.StretchToFill, true, 10.0F);

            GUILayout.BeginHorizontal();
            GUI.Label(new Rect(280, 10, 100, 20), "Depth Offset:");
            controller.depthOffset = EditorGUI.FloatField(new Rect(362, 10, 50, 20), controller.depthOffset);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUI.Label(new Rect(280, 40, 100, 20), "Height Offset:");
            controller.heightOffset = EditorGUI.FloatField(new Rect(362, 40, 50, 20), controller.heightOffset);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.Label(new Rect(280, 100, 250, 60), "*Spawn Point is\n  only relevant\n  to head-mounted\n  platforms.");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.Label(new Rect(280, 275, 100, 20), "Cursor Offset:");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            controller.cursorOffset = EditorGUI.Vector3Field(new Rect(280, 295, 140, 20), "", controller.cursorOffset);
            GUILayout.EndHorizontal();

            GUILayout.Space(680);
        }
    }
}
