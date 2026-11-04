using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SpatialGestureText.Input.Editor
{
	[CustomEditor(typeof(LextingInputModuleManager), true)]
	public class LextingInputModuleManagerEditor : UnityEditor.Editor
	{
        public Texture leapConfigTexture;

        //InputMode mode;

        //SerializedProperty modeProperty;

       
        /// <summary>
        /// Quest Hands
        /// </summary>
        SerializedProperty leftHand_QH_Object;
        SerializedProperty rightHand_QH_Object;
        SerializedProperty head_QH_Object;

        /// <summary>
        /// Quest Hands Passthrough
        /// </summary>
        SerializedProperty leftHand_QHP_Object;
        SerializedProperty rightHand_QHP_Object;
        SerializedProperty head_QHP_Object;

        /// <summary>
        /// Quest Touch
        /// </summary>
        SerializedProperty leftHand_QT_Object;
        SerializedProperty rightHand_QT_Object;
        SerializedProperty head_QT_Object;

        /// <summary>
        /// Quest Touch Passthrough
        /// </summary>
        SerializedProperty leftHand_QTP_Object;
        SerializedProperty rightHand_QTP_Object;
        SerializedProperty head_QTP_Object;

        /// <summary>
        /// Open XR
        /// </summary>
        SerializedProperty leftHand_OXR_Object;
        SerializedProperty rightHand_OXR_Object;
        SerializedProperty head_OXR_Object;

        /// <summary>
        /// Leap Motion
        /// </summary>
        SerializedProperty leftHand_L_Object;
        SerializedProperty rightHand_L_Object;

        /// <summary>
        /// Leap Motion to PC keyboard
        /// </summary>
        SerializedProperty leftHand_LK_Object;
        SerializedProperty rightHand_LK_Object;

        /// <summary>
        /// Leap Motion to Looking Glass
        /// </summary>
        SerializedProperty leftHand_LG_Object;
        SerializedProperty rightHand_LG_Object;

        /// <summary>
        /// Leap Motion to Looking Glass to PC keyboard
        /// </summary>
        SerializedProperty leftHand_LGK_Object;
        SerializedProperty rightHand_LGK_Object;

        GUIStyle redText, leapExtraText;

        LextingInputModuleManager manager;

        private void OnEnable()
        {
            //modeProperty = serializedObject.FindProperty("mode");
            FindHLR(ref head_QH_Object, "head_QH", ref leftHand_QH_Object, "leftHand_QH", ref rightHand_QH_Object, "rightHand_QH");
            FindHLR(ref head_QHP_Object, "head_QHP", ref leftHand_QHP_Object, "leftHand_QHP", ref rightHand_QHP_Object, "rightHand_QHP");
            FindHLR(ref head_QT_Object, "head_QT", ref leftHand_QT_Object, "leftHand_QT", ref rightHand_QT_Object, "rightHand_QT");
            FindHLR(ref head_QTP_Object, "head_QTP", ref leftHand_QTP_Object, "leftHand_QTP", ref rightHand_QTP_Object, "rightHand_QTP");
            FindHLR(ref head_OXR_Object, "head_OXR", ref leftHand_OXR_Object, "leftHand_OXR", ref rightHand_OXR_Object, "rightHand_OXR");
            FindLR(ref leftHand_L_Object, "leftHand_L", ref rightHand_L_Object, "rightHand_L");
            FindLR(ref leftHand_LK_Object, "leftHand_LK", ref rightHand_LK_Object, "rightHand_LK");
            FindLR(ref leftHand_LG_Object, "leftHand_LG", ref rightHand_LG_Object, "rightHand_LG");
            FindLR(ref leftHand_LGK_Object, "leftHand_LGK", ref rightHand_LGK_Object, "rightHand_LGK");
            redText = new GUIStyle();
            redText.richText = true;
            leapExtraText = new GUIStyle();
            leapExtraText.richText = true;

            leapConfigTexture = Resources.Load("leap_config") as Texture2D;

            manager = (LextingInputModuleManager)target;

        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUILayout.Space(10);
            manager.mode = (InputMode)EditorGUILayout.EnumPopup("MODE: ", manager.mode);
            GUILayout.Space(10);
            //GUILayout.Label("<size=14><color=red>Assign these </color><color=white>from your controller:</color></size>", redText);

            if (manager.mode == InputMode.QUEST_HANDS)
            {
                GUILayout.Space(10);
                GUILayout.Label("<size=12><color=lightgrey>This mode allows a user of an Oculus Quest app to launch\n" +
                    "the Lexting UI at a default offset in front of their body\n" +
                    "when they need to input text.</color></size>", redText);
                GUILayout.Space(10);
                DisplayHLROculus(ref head_QH_Object, ref leftHand_QH_Object, ref rightHand_QH_Object);
            }
            else if (manager.mode == InputMode.QUEST_HANDS_PASSTHROUGH)
            {
                GUILayout.Space(10);
                GUILayout.Label("<size=12><color=lightgrey>This mode allows a user of an Oculus Quest Passthrough app to \n" +
                    "connect to a local PC over WIFI and use the Lexting UI\n" +
                    "to remote-control text entry on that PC.</color></size>", redText);
                GUILayout.Space(10);
                DisplayHLROculus(ref head_QHP_Object, ref leftHand_QHP_Object, ref rightHand_QHP_Object);
            }
            else if (manager.mode == InputMode.QUEST_TOUCH)
            {
                GUILayout.Space(10);
                GUILayout.Label("<size=12><color=lightgrey>This mode allows a user of an Oculus Quest app to launch\n" +
                    "the Lexting UI at a default offset in front of their body\n" +
                    "when they need to input text.</color></size>", redText);
                GUILayout.Space(10);
                DisplayHLROculus(ref head_QT_Object, ref leftHand_QT_Object, ref rightHand_QT_Object);
            }
            else if (manager.mode == InputMode.QUEST_TOUCH_PASSTHROUGH)
            {
                GUILayout.Space(10);
                GUILayout.Label("<size=12><color=lightgrey>This mode allows a user of an Oculus Quest Passthrough app to \n" +
                    "connect to a local PC over WIFI and use the Lexting UI\n" +
                    "to remote-control text entry on that PC.</color></size>", redText);
                GUILayout.Space(10);
                DisplayHLROculus(ref head_QTP_Object, ref leftHand_QTP_Object, ref rightHand_QTP_Object);
            }
            else if (manager.mode == InputMode.OPEN_XR)
                DisplayHLR(ref head_OXR_Object, ref leftHand_OXR_Object, ref rightHand_OXR_Object);
            else {
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                GUILayout.Space(100);
                GUILayout.Label("<size=12><color=white><b>NOTE:</b> The AttachmentHands class of the Leap Motion SDK \n" +
                    "must be used to expose six points on each hand as shown here. \n" +
                    "Make sure each green box is checked on your Leap controller.\n" +
                    "See Leap documentation for details about AttachmentHands.\n" +
                    "Then drag and drop the WRIST object for each hand here.</color></size>", leapExtraText);
                GUILayout.EndHorizontal();
                
                GUILayout.Space(20);

                GUI.DrawTexture(new Rect(30, 50, 80, 80), leapConfigTexture, ScaleMode.StretchToFill, true, 10.0F);
                if (manager.mode == InputMode.LEAP)
                    DisplayLR(ref leftHand_L_Object, ref rightHand_L_Object);
                else if (manager.mode == InputMode.LEAP_KEYBOARD)
                    DisplayLR(ref leftHand_LK_Object, ref rightHand_LK_Object);
                else if (manager.mode == InputMode.LEAP_LKGLASS)
                    DisplayLR(ref leftHand_LG_Object, ref rightHand_LG_Object);
                else if (manager.mode == InputMode.LEAP_LKGLASS_KEYBOARD)
                    DisplayLR(ref leftHand_LGK_Object, ref rightHand_LGK_Object);

            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Acquire references to the head, left-hand, and right-hand transforms from the input manager for the
        /// currently selected platform
        /// </summary>
        private void FindLR(ref SerializedProperty lHand, string lHandName, ref SerializedProperty rHand, string rHandName)
        {
            lHand = serializedObject.FindProperty(lHandName);
            rHand = serializedObject.FindProperty(rHandName);
        }

        /// <summary>
        /// Show the relevant head, left-hand, and right-hand properties in inspector for the selected mode.
        /// </summary>
        private void DisplayLR(ref SerializedProperty lHand, ref SerializedProperty rHand)
        {
            GUILayout.Label("<size=14><color=red>Assign these </color><color=white>from your controller:</color></size>", redText);
            GUILayout.Space(10);
            lHand.objectReferenceValue = EditorGUILayout.ObjectField("Left Wrist", lHand.objectReferenceValue, typeof(Transform), true);
            rHand.objectReferenceValue = EditorGUILayout.ObjectField("Right Wrist", rHand.objectReferenceValue, typeof(Transform), true);
        }

        /// <summary>
        /// Acquire references to the head, left-hand, and right-hand transforms from the input manager for the
        /// currently selected platform
        /// </summary>
        private void FindHLR(
            ref SerializedProperty head,  string headName,
            ref SerializedProperty lHand,  string lHandName,
            ref SerializedProperty rHand,  string rHandName)
        {
            lHand = serializedObject.FindProperty(lHandName);
            rHand = serializedObject.FindProperty(rHandName);
            head = serializedObject.FindProperty(headName);
        }

        /// <summary>
        /// Show the relevant head, left-hand, and right-hand properties in inspector for the selected mode.
        /// </summary>
        private void DisplayHLR(
            ref SerializedProperty head, ref SerializedProperty lHand, ref SerializedProperty rHand)
        {
            GUILayout.Label("<size=14><color=red>Assign these </color><color=white>from your controller:</color></size>", redText);
            GUILayout.Space(10);
            head.objectReferenceValue = EditorGUILayout.ObjectField("Head", head.objectReferenceValue, typeof(Transform), true);
            lHand.objectReferenceValue = EditorGUILayout.ObjectField("Left Hand", lHand.objectReferenceValue, typeof(Transform), true);
            rHand.objectReferenceValue = EditorGUILayout.ObjectField("Right Hand", rHand.objectReferenceValue, typeof(Transform), true);
        }

        private void DisplayHLROculus(
            ref SerializedProperty head, ref SerializedProperty lHand, ref SerializedProperty rHand)
        {
            GUILayout.Label("<size=14><color=red>Assign these </color><color=white>from your controller:</color></size>", redText);
            GUILayout.Space(10);
            head.objectReferenceValue = EditorGUILayout.ObjectField("Center Eye Anchor", head.objectReferenceValue, typeof(Transform), true);
            lHand.objectReferenceValue = EditorGUILayout.ObjectField("Left Hand Anchor", lHand.objectReferenceValue, typeof(Transform), true);
            rHand.objectReferenceValue = EditorGUILayout.ObjectField("Right Hand Anchor", rHand.objectReferenceValue, typeof(Transform), true);
        }
    }
}
