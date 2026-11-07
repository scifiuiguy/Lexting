namespace SpatialGestureText.Tutorial
{
    /// <summary>Authoritative copy from docs/in_app_tutorial.md (22 steps, index 0–21).</summary>
    public static class TutorialStepContent
    {
        public const int StepCount = 22;

        public static readonly string Title = "Lexting tutorial";

        public static string Body(int stepIndex)
        {
            switch (stepIndex)
            {
                case 0:
                    return "Hold your hands in front of you, palms facing each other, fingers open and relaxed.";
                case 1:
                    return "Great! Now do the GRAB gesture with the left hand like this.";
                case 2:
                    return "Great! Now the right hand.";
                case 3:
                    return "Locate the letter T with either hand. Hover your cursor into it and grab–release to type it.";
                case 4:
                    return "Great! Now supinate your wrist (turn palm upward).";
                case 5:
                    return "The letters changed for palm-up position. Grab H.";
                case 6:
                    return "Great! Now pronate your wrist (turn palm downward).";
                case 7:
                    return "The letters changed again for palm-down. Find E and grab it.";
                case 8:
                    return "Great! You typed a word! Now let's finish typing the word FOX.";
                case 9:
                    return "Notice the X is nowhere to be found in palm-sideways, palm-up, or palm-down. But try turning your palm forward toward the screen.";
                case 10:
                    return "X and Z just showed up in the UI! Grab X.";
                case 11:
                    return "Great! Now delete the word fox by tapping your left wrist with your right middle finger.";
                case 12:
                    return "Great job! You've accessed the whole alphabet, but typing one letter at a time is slow — let's go faster with Word Mode!";
                case 13:
                    return "Word Mode is GRAB + HOLD + DRAG with both hands. GRAB+HOLD T with the left and H with the right.";
                case 14:
                    return "Now let go with both hands — Lexting will autofill the word.";
                case 15:
                    return "Great! Try a longer word: tantalize. Grab T with the left and A with the right. Hold both.";
                case 16:
                    return "Great! Keep holding. Now drag your left hand to N.";
                case 17:
                    return "Great! Now drag your right hand to T.";
                case 18:
                    return "Great! Keep holding. Finish positioning so the stem reads tantalize and your right hand is on T.";
                case 19:
                    return "Great! Keep holding with the right, but open the left hand to enter finalization.";
                case 20:
                    return "Great! You're in Finalization Mode. Pick the word tantalize on the left. Rotate your wrists to explore your suffix choices. Choose any suffix and punctuation, then release.";
                case 21:
                    return "Amazing! You're ready to freestyle. Think of a sentence and type it with Word Mode.";
                default:
                    return "";
            }
        }
    }
}
