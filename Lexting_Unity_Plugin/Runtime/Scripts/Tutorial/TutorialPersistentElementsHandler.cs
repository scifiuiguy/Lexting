using Pixelplacement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPersistentElementsHandler : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    public void ToggleVisibility(bool on, bool transition)
    {
        if (transition)
            Tween.CanvasGroupAlpha(canvasGroup, (on) ? 1 : 0, 1, 0);
        else canvasGroup.alpha = (on) ? 1 : 0;
    }
}
