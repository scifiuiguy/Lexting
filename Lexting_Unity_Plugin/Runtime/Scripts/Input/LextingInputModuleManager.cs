#if LEXTING_FOUND_LEAP_SDK
using SpatialGestureText.Input.Leap;
#endif
using SpatialGestureText.LetterCube;
using SpatialGestureText.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.Input
{
	public enum InputMode 
	{
        // All platforms except OpenXR have the ability to work in two distinct ways:
        // 1) as an interface embedded inside a stand-alone app.
        // 2) as a background app that replaces a keyboard and mouse.

        // These enumerated items must match the children of the [INPUTS] object in Hierarchy:
        QUEST_HANDS, // use this to run the Lexting interface in your Quest app w/ hand tracking
        QUEST_HANDS_PASSTHROUGH,  // same as QUEST_HANDS except it uses a wireless network to forward input to PC
        QUEST_TOUCH, // use this to run the Lexting interface in your Quest app w/ Touch controllers
        QUEST_TOUCH_PASSTHROUGH, // same as QUEST_TOUCH except it uses a wireles network to forward input to PC
        OPEN_XR,    // use this to run the Lexting interface in your app on STEAM_VR, Windows MR, Oculus Rift, etc.
        LEAP, // use this to run the Lexting interface in your PC app w/ Leap hand tracking
        LEAP_KEYBOARD, // use this to build your scene as a Windows overlay where Leap replaces keyboard and mouse.
        LEAP_LKGLASS, // use this to to run your PC app while the Lexting interface renders on Looking Glass w/ Leap hand tracking.
        LEAP_LKGLASS_KEYBOARD // use this to run the Lexting interface in Looking Glass as a replacement for PC mouse & keyboard.
        // future platforms: Magic Leap, Hololens, Snap Spectacles, NReal, Apple...?
	}


    public class LextingInputModuleManager : Singleton<LextingInputModuleManager>
    {
        #region vars
        #region public vars

        public InputMode mode = InputMode.QUEST_HANDS_PASSTHROUGH;

        /// <summary>
        /// This is every tracking node Lexting needs to track all variations of hands & controllers.
        /// There must be separate refs for each platform even though there's overlap because you may
        /// have two different controller instances and you don't want assignment of one to overwrite 
        /// assignment of the other, e.g. even if the various controllers have identical hand transforms, they
        /// each have unique instances of their own that each may need to be connected here.
        /// </summary>
        public Transform head_QH;
        public Transform rightHand_QH;
        public Transform leftHand_QH;

        public Transform head_QHP;
        public Transform rightHand_QHP;
        public Transform leftHand_QHP;

        public Transform head_QT;
        public Transform rightHand_QT;
        public Transform leftHand_QT;

        public Transform head_QTP;
        public Transform rightHand_QTP;
        public Transform leftHand_QTP;

        public Transform head_OXR;
        public Transform rightHand_OXR;
        public Transform leftHand_OXR;

        public Transform rightHand_L;
        public Transform leftHand_L;
        
        public Transform rightHand_LK;
        public Transform leftHand_LK;

        public Transform rightHand_LG;
        public Transform leftHand_LG;

        public Transform rightHand_LGK;
        public Transform leftHand_LGK;


        [HideInInspector]
        public Transform head(InputMode mode) => mode switch
        {
            InputMode.QUEST_HANDS => head_QH,
            InputMode.QUEST_HANDS_PASSTHROUGH => head_QHP,
            InputMode.QUEST_TOUCH => head_QT,
            InputMode.QUEST_TOUCH_PASSTHROUGH => head_QTP,
            InputMode.OPEN_XR => head_OXR,
            InputMode.LEAP => null,
            InputMode.LEAP_KEYBOARD => null,
            InputMode.LEAP_LKGLASS => null,
            InputMode.LEAP_LKGLASS_KEYBOARD => null,
            _ => throw new NotImplementedException()
        };

        public Transform hand(bool right, InputMode mode) => mode switch { 
            InputMode.QUEST_HANDS => hand(rightHand_QH,leftHand_QH,right),
            InputMode.QUEST_HANDS_PASSTHROUGH => hand(rightHand_QHP, leftHand_QHP, right),
            InputMode.QUEST_TOUCH => hand(rightHand_QT, leftHand_QT, right),
            InputMode.QUEST_TOUCH_PASSTHROUGH => hand(rightHand_QTP, leftHand_QTP, right),
            InputMode.OPEN_XR => hand(rightHand_OXR, leftHand_OXR, right),
            InputMode.LEAP => hand(rightHand_L, leftHand_L, right),
            InputMode.LEAP_KEYBOARD => hand(rightHand_LK, leftHand_LK, right),
            InputMode.LEAP_LKGLASS => hand(rightHand_LG, leftHand_LG, right),
            InputMode.LEAP_LKGLASS_KEYBOARD => hand(rightHand_LGK, leftHand_LGK, right),
            _ => throw new NotImplementedException()
        };

        [HideInInspector] public Transform rightHand => hand(true, mode);
        [HideInInspector] public Transform leftHand => hand(false, mode);

        [HideInInspector] public Transform hand(bool right) => hand(right, mode);

        [HideInInspector]
        public bool isAnyLeapMode => mode == InputMode.LEAP || mode == InputMode.LEAP_KEYBOARD
            || mode == InputMode.LEAP_LKGLASS || mode == InputMode.LEAP_LKGLASS_KEYBOARD;

        [HideInInspector]

        public bool isAnyQuestMode => mode == InputMode.QUEST_HANDS || mode == InputMode.QUEST_HANDS_PASSTHROUGH || mode == InputMode.QUEST_TOUCH
        || mode == InputMode.QUEST_TOUCH_PASSTHROUGH;

        [HideInInspector]
        public bool isDesktop => mode == InputMode.QUEST_HANDS_PASSTHROUGH || mode == InputMode.QUEST_TOUCH_PASSTHROUGH ||
            isAnyLeapMode;
        #endregion public vars
        #region private vars

        public Transform hand(Transform rCandidate, Transform lCandidate, bool right) =>
            right ? rCandidate : lCandidate;

        public HandMarker rightMarker;
        public HandMarker leftMarker;

        public bool hasHead => head(mode) != null && (mode == InputMode.QUEST_HANDS || mode == InputMode.QUEST_HANDS_PASSTHROUGH || mode == InputMode.QUEST_TOUCH
           || mode == InputMode.QUEST_TOUCH_PASSTHROUGH || mode == InputMode.OPEN_XR);

        /// <summary>
        /// If true, the Lexting.printer is sending all output an external computer's keyboard over Wifi rather than displaying it 
        /// in an input field within the same app.
        /// </summary>
        public bool isStreaming => mode == InputMode.QUEST_HANDS_PASSTHROUGH || mode == InputMode.QUEST_TOUCH_PASSTHROUGH 
            || mode == InputMode.LEAP_KEYBOARD || mode == InputMode.LEAP_LKGLASS_KEYBOARD;

        /// <summary>
        /// Needed to distinguish between all modes that satisfy isStreaming by filtering those that send 
        /// ALL 6DOF data to drive an external UI over WiFi vs 
        /// Passthrough which renders the UI locally and sends only text output data over WiFi.
        /// </summary>
        public bool isPassthrough => mode == InputMode.QUEST_HANDS_PASSTHROUGH || mode == InputMode.QUEST_TOUCH_PASSTHROUGH;

        private Transform _activeChild;

        private LextingHand _input(bool right) => Lexting.input.hand(right);

       
        private bool _hasFullHands => _hasFullHand(true) && _hasFullHand(false);

        private bool _hasFullHand(bool right) => hand(right) != null && _input(right).wrist != null
            && _input(right).indexBase != null && _input(right).indexTip != null && _input(right).middleTip != null
            && _input(right).thumbTip != null && (isAnyQuestMode || isAnyLeapMode);
        
#endregion private vars
        #endregion vars

        #region init
        private void Awake()
        {
            //transform.GetChild((int)mode).gameObject.SetActive(true);
            InitControllerComponents();
        }

        /// <summary>
        /// Assigns head and hand markers appropriately for any given platform, then instantiates that platform's
        /// [PLATFORM]Input class, which will handle most initialization unique to that platform.
        /// </summary>
        private void InitControllerComponents()
        {
            if (hasHead)
            {
                try {
                    head(mode).gameObject.AddComponent<HeadMarker>();
                }
                catch (Exception e) {
                    Debug.LogError("");
                };
                if (head(mode) == null)
                {
                    Debug.LogError("Lexting Input Module Manager needs a reference to the head transform of the current controller." +
                        "Assign it in the appropriate module's Inspector (see the desired child object of this component).");
                    return;
                }
                else head(mode).gameObject.AddComponent<HeadMarker>();
            }
            
            if (hand(true, mode) != null)
            {
                rightMarker = hand(true, mode).gameObject.AddComponent<HandMarker>();
                rightMarker.right = true;
            }
            else Debug.LogError("Lexting Input Module Manager needs a reference to the right hand transform of the current controller." +
                "Assign it in the appropriate module's Inspector (see the desired child object of this component).");

            if (hand(false, mode) != null)
            {
                leftMarker = hand(false, mode).gameObject.AddComponent<HandMarker>();
                leftMarker.right = false;
            }
            else Debug.LogError("Lexting Input Module Manager needs a reference to the left hand transform of the current controller." +
                "Assign it in the appropriate module's Inspector (see the desired child object of this component).");

            if (isAnyQuestMode)
            {
                if (head(mode) != null)
                    head(mode).gameObject.AddComponent<OculusHandInput>();
                else Debug.LogError("Lexting Input Module Manager was unable to initialize Oculus Hand Input. Has the head" +
                "transform been assigned in the active Lexting input module?");
            }

#if LEXTING_FOUND_LEAP_SDK
            if (isAnyLeapMode)
            {
                if (hand(true, mode) != null && hand(true, mode).parent != null)
                    hand(true, mode).parent.parent.gameObject.AddComponent<LeapHandInput>();
                else Debug.LogError("Lexting Input Module Manager's Leap module needs hands to be assigned to the same" +
                "transforms that contain each of Leap Motion's AttachmentHand scripts. See Leap Motion documentation" +
                "for details about the AttachmentHand class.");
            }
#endif


        }
        #endregion init

        #region helper methods
        //private void Update()
        //{
        //    Debug.Log("Right hand QH name: " + rightHand_QH.name);
        //}
        #endregion helper methods
    }
}
