using UnityEngine;

namespace SpatialGestureText.Tutorial
{
    /// <summary>Hook for 3D Skip / Don't-show-again nodes (widget-style triggers).</summary>
    public class TutorialDismissControls : MonoBehaviour
    {
        public void SkipThisSession()
        {
            if (TutorialManager.Instance != null)
                TutorialManager.Instance.EndTutorialFromSkip(dontShowAgain: false);
        }

        public void DontShowAgain()
        {
            if (TutorialManager.Instance != null)
                TutorialManager.Instance.EndTutorialFromSkip(dontShowAgain: true);
        }
    }
}
