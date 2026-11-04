using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SpatialGestureText.Input.ControllerOverrides.Editor
{
	 [CustomEditor(typeof(ControllerOverride), true)]

    public class ShowHideControllerOverrideProperties : UnityEditor.Editor
    {
        SerializedProperty toggle;
        SerializedProperty leftHandObject;
        SerializedProperty rightHandObject;
        SerializedProperty headObject;

        private void OnEnable()
        {
            toggle = serializedObject.FindProperty("iWantToUseMyOwnController");
            leftHandObject = serializedObject.FindProperty("leftHand");
            rightHandObject = serializedObject.FindProperty("rightHand");
            headObject = serializedObject.FindProperty("head");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            toggle.boolValue = EditorGUILayout.Toggle(toggle.displayName, toggle.boolValue);

            if (toggle.boolValue)
            {
                leftHandObject.objectReferenceValue = EditorGUILayout.ObjectField(leftHandObject.displayName, leftHandObject.objectReferenceValue, typeof(Transform), true);
                rightHandObject.objectReferenceValue = EditorGUILayout.ObjectField(rightHandObject.displayName, rightHandObject.objectReferenceValue, typeof(Transform), true);
                headObject.objectReferenceValue = EditorGUILayout.ObjectField(headObject.displayName, headObject.objectReferenceValue, typeof(Transform), true);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
