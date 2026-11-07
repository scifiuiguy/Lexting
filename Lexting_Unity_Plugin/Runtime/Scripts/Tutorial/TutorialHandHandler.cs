using Pixelplacement;
using UnityEngine;

namespace SpatialGestureText.Tutorial
{
    /// <summary>
    /// Visibility and procedural open/closed poses for non-tracked tutorial hands.
    /// </summary>
    public class TutorialHandHandler : MonoBehaviour
    {
        private SkinnedMeshRenderer[] _skinnedMeshRenderers;
        private readonly Color colorOn = new Color(1, 1, 1, 0.6f);
        private readonly Color colorOff = new Color(1, 1, 1, 0);
        private TutorialProceduralHandPose proceduralPose;
        private Animator thumbController;

        public bool IsRightHand { get; private set; }

        private void Awake()
        {
            proceduralPose = GetComponent<TutorialProceduralHandPose>();
            CacheRenderers();
            SetRenderersAlpha(colorOff.a);
            thumbController = GetComponentInChildren<Animator>();
            CacheHandSideFromName();
        }

        /// <summary>Safe before Awake (e.g. tutorial chrome resolve on inactive hierarchy).</summary>
        public void CacheHandSideFromName()
        {
            IsRightHand = TutorialHandSide.IsRightSideName(name);
        }

        private void CacheRenderers()
        {
            Transform root = proceduralPose != null ? proceduralPose.PoseRoot : transform;
            _skinnedMeshRenderers = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        }

        public void Fade(bool on)
        {
            if (_skinnedMeshRenderers == null || _skinnedMeshRenderers.Length == 0)
                CacheRenderers();
            if (_skinnedMeshRenderers == null || _skinnedMeshRenderers.Length == 0)
                return;

            foreach (var renderer in _skinnedMeshRenderers)
            {
                if (renderer == null)
                    continue;
                var color = on ? colorOn : colorOff;
                Tween.Color(renderer.material, color, 0.5f, 0);
            }
        }

        private void SetRenderersAlpha(float alpha)
        {
            if (_skinnedMeshRenderers == null)
                return;
            foreach (var renderer in _skinnedMeshRenderers)
            {
                if (renderer == null)
                    continue;
                var c = renderer.material.color;
                c.a = alpha;
                renderer.material.color = c;
            }
        }

        public void AnimateToOpen(float duration = -1f)
        {
            if (thumbController != null)
                thumbController.enabled = false;
            proceduralPose?.AnimateToOpen(duration);
        }

        public void AnimateToClosed(float duration = -1f)
        {
            if (thumbController != null)
                thumbController.enabled = false;
            proceduralPose?.AnimateToClosed(duration);
        }

        public void SetOpenImmediate()
        {
            if (thumbController != null)
                thumbController.enabled = false;
            proceduralPose?.SetClosedAmountImmediate(0f);
        }
    }

    internal static class TutorialHandSide
    {
        public static bool IsPoseReferenceName(string objectName)
        {
            return objectName.IndexOf("_Open", System.StringComparison.OrdinalIgnoreCase) >= 0
                   || objectName.IndexOf("_Closed", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool IsLeftSideName(string objectName)
        {
            return objectName.IndexOf("Left", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool IsRightSideName(string objectName)
        {
            return objectName.IndexOf("Right", System.StringComparison.OrdinalIgnoreCase) >= 0
                   && !IsLeftSideName(objectName);
        }

        /// <summary>-1 left, 1 right, 0 unknown.</summary>
        public static int SideFromTransform(Transform t)
        {
            for (var p = t; p != null; p = p.parent)
            {
                if (IsRightSideName(p.name))
                    return 1;
                if (IsLeftSideName(p.name))
                    return -1;
            }

            return 0;
        }

        /// <summary>
        /// True if any ancestor name contains <c>_Left</c> / <c>_Right</c> (tutorial hand prefabs, e.g. <c>TutorialHand_Left_Open</c>).
        /// </summary>
        public static bool HasAncestorNameContaining(Transform t, string substring)
        {
            if (t == null || string.IsNullOrEmpty(substring))
                return false;
            for (var p = t; p != null; p = p.parent)
            {
                if (p.name.IndexOf(substring, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        /// <summary>Open/closed duplicate rigs used only for pose sampling — not the faded-in tutorial hand.</summary>
        public static bool IsUnderPoseReferenceHierarchy(Transform t)
        {
            for (var p = t; p != null; p = p.parent)
            {
                if (IsPoseReferenceName(p.name))
                    return true;
            }

            return false;
        }
    }
}
