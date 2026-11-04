using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SpatialGestureText.Descriptors.Editor
{
//#if !LEXTING_DESCRIPTORS_EDITABLE
    [CustomEditor(typeof(Descriptor), true)] //  <---- DISABLE THIS LINE TO MAKE DESCRIPTORS TEMPORARILY EDITABLE
//#endif
    public class ReadOnlyDescriptor : UnityEditor.Editor
	{
        SerializedProperty description;
        
        private void OnEnable()
        {
            description = serializedObject.FindProperty("description");
        }
        public override void OnInspectorGUI()
        {                
            EditorGUILayout.LabelField(description.stringValue, EditorStyles.textArea);
        }
    }
}
