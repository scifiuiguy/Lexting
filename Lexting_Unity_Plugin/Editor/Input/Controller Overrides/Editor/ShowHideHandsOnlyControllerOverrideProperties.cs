using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SpatialGestureText.Input.ControllerOverrides.Editor
{
    [CustomEditor(typeof(HandsOnlyControllerOverride), true)]

    public class ShowHideHandsOnlyControllerOverrideProperties : UnityEditor.Editor
	{
        SerializedProperty toggle;
        SerializedProperty leftHandObject;
        SerializedProperty rightHandObject;

        private void OnEnable()
        {
            toggle = serializedObject.FindProperty("iWantToUseMyOwnController");
            leftHandObject = serializedObject.FindProperty("leftHand");
            rightHandObject = serializedObject.FindProperty("rightHand");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            toggle.boolValue = EditorGUILayout.Toggle(toggle.displayName, toggle.boolValue);

            if (toggle.boolValue)
            {
                leftHandObject.objectReferenceValue = EditorGUILayout.ObjectField(leftHandObject.displayName, leftHandObject.objectReferenceValue, typeof(Transform), true);
                rightHandObject.objectReferenceValue = EditorGUILayout.ObjectField(rightHandObject.displayName, rightHandObject.objectReferenceValue, typeof(Transform), true);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
