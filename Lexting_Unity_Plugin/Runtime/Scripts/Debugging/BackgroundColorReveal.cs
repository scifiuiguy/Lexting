using Pixelplacement;
using SpatialGestureText.Streaming;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundColorReveal : MonoBehaviour
{
    private CanvasGroup imageCanvasGroup, mouseModeTextCanvasGroup;

    private void Awake()
    {
        imageCanvasGroup = transform.GetChild(0).GetComponent<CanvasGroup>();
        mouseModeTextCanvasGroup = transform.GetChild(1).GetComponent<CanvasGroup>();
        this.WaitS(3, () => {
            Tween.CanvasGroupAlpha(imageCanvasGroup, 1, 1, 0);
        });

        this.Wait(()=>MouseController._.Instance.Ready(), () => {
            this.Subscribe(MouseController._.Instance.OnMouseControllerActive, (right, on) => {
                Tween.CanvasGroupAlpha(imageCanvasGroup, (on) ? 0 : 1, 1, 0);
                Tween.CanvasGroupAlpha(mouseModeTextCanvasGroup, (on) ? 1 : 0, 1, 0);
            });
        });
    }

    
}
