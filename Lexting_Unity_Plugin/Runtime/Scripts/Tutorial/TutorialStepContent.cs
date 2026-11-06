namespace SpatialGestureText.Tutorial
{
    /// <summary>Authoritative copy from docs/in_app_tutorial.md (17 steps, index 0–16).</summary>
    public static class TutorialStepContent
    {
        public const int StepCount = 17;

        public static readonly string Title = "Lexting tutorial";

        public static string Body(int stepIndex)
        {
            switch (stepIndex)
            {
                case 0:
                    return "Hold your hands in front of you, palms facing each other, fingers open and relaxed.";
                case 1:
                    return "Great! Now close the left hand like this.";
                case 2:
                    return "Great! Now the right hand.";
                case 3:
                    return "Locate the letter T with either hand. Hover your cursor into it and grab–release to type it.";
                case 4:
                    return "Great! Now supinate your wrist (turn palm upward). The letters changed. Find H and grab it.";
                case 5:
                    return "Great! Now pronate your wrist (turn palm downward). The letters changed again. Find E and grab it.";
                case 6:
                    return "Great! You typed a word! Now finish FOX. Turn your palm forward toward the screen. Find X and grab it.";
                case 7:
                    return "Great! Delete the word fox by tapping your left wrist with your right middle finger.";
                case 8:
                    return "Great! You've learned the whole alphabet. Typing one letter at a time is slow — let's go faster with Word Mode!";
                case 9:
                    return "Word Mode is grab + hold + drag with both hands. Start the: grab T with the left and H with the right. Hold both.";
                case 10:
                    return "Now let go with both hands — Lexting will autofill the word.";
                case 11:
                    return "Great! Try a longer word: tantalize. Grab T with the left and A with the right. Hold both.";
                case 12:
                    return "Great! Keep holding. Now drag your left hand to N.";
                case 13:
                    return "Great! Now drag your right hand to T.";
                case 14:
                    return "Great! Keep holding with the right, but open the left hand to enter finalization.";
                case 15:
                    return "Great! You're in Finalization Mode. Pick the word tantalize on the left. Rotate your wrists to explore your suffix choices. Choose any suffix and punctuation, then release.";
                case 16:
                    return "Amazing! You're ready to freestyle. Think of a sentence and type it with Word Mode.";
                default:
                    return "";
            }
        }
    }
}
