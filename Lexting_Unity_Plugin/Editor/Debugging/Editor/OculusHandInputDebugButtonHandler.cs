using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using SpatialGestureText.Input;

namespace SpatialGestureText
{
    // INSTRUCTIONS: Use this for quick unit tests. It puts a button per test on your script in inspector.
    // Prep your script: make a method that runs your test. You'll call it from here.

    // 1) Duplicate and rename this file. 
    // 2) Make sure the new copy stays in the Editor folder and its class name matches its file name.
    // 3) Uncomment the disabled lines.
    // 4) Replace the stuff in CAPS. You can add more buttons if you like. Just copy/paste the "buttons.Add(..." line multiple times.
    // 5) Disable/Hide any button by switching true to false.

    [CustomEditor(typeof(OculusHandInput))]
    public class OculusHandInputDebugButtonHandler : UnityEditor.Editor
    {
        private List<DebugButton> buttons;

        private void OnEnable()
        {
            buttons = new List<DebugButton>();
            OculusHandInput component = (OculusHandInput)target;
            buttons.Add(new DebugButton("Test Reposition UI", true, () =>  component.UnitTest_Reposition()));
            buttons.Add(new DebugButton("Test Right State Change", true, () => component.UnitTest_RightStateChanged()));

            //buttons.Add(new DebugButton("BUTTON_NAME", true, () => component.StartCoroutine(component.YOUR_IENUMERATOR_TEST())));
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            foreach (DebugButton button in buttons)
                if (button.enabled && GUILayout.Button(button.buttonName))
                    button.OnButtonPressed.I();
        }
    }
    
}
