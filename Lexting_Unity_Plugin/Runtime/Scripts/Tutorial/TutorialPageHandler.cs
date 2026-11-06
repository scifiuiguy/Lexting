using UnityEngine;
using UnityEngine.UI;

namespace SpatialGestureText.Tutorial
{
    /// <summary>
    /// Lives on <c>TutorialGroup/Pages</c>. Updates <c>step1</c> body + canvas <c>Title</c> copy only.
    /// All alpha fades use the <c>TutorialGroup</c> CanvasGroup on <see cref="TutorialManager"/> (master opacity).
    /// </summary>
    public class TutorialPageHandler : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text stepBodyText;

        public void BindToExistingHierarchy()
        {
            if (stepBodyText == null)
            {
                var step1 = transform.Find("step1");
                if (step1 != null)
                    stepBodyText = step1.GetComponent<Text>();
            }

            if (titleText == null)
            {
                var canvas = GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    var title = canvas.transform.Find("Title");
                    if (title != null)
                        titleText = title.GetComponent<Text>();
                }
            }

            HideUnusedLegacySteps();
        }

        public void HideUnusedLegacySteps()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.name == "step1")
                    continue;

                var group = child.GetComponent<CanvasGroup>();
                if (group != null)
                    group.alpha = 0f;
                else
                    child.gameObject.SetActive(false);
            }
        }

        /// <summary>Swap step copy only; does not tween alpha (manager owns CanvasGroup fades).</summary>
        public void ApplyStepCopy(int stepIndex)
        {
            if (titleText != null)
            {
                titleText.gameObject.SetActive(stepIndex == 0);
                if (stepIndex == 0)
                    titleText.text = TutorialStepContent.Title;
            }

            if (stepBodyText != null)
            {
                stepBodyText.gameObject.SetActive(true);
                stepBodyText.text = TutorialStepContent.Body(stepIndex);
                var stepGroup = stepBodyText.GetComponent<CanvasGroup>();
                if (stepGroup != null)
                    stepGroup.alpha = 1f;
            }
        }
    }
}
