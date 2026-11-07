using SpatialGestureText.Hotkeys;
using SpatialGestureText.TwoZoneCube;
using UnityEngine;
using UnityEngine.UI;

namespace SpatialGestureText.Tutorial
{
    /// <summary>
    /// Clones live widget center-cell geometry for tutorial Skip (CenterZone mesh + Center_Scaler rotation).
    /// </summary>
    public static class TutorialSkipZoneFactory
    {
        public static Transform FindWidgetCenterZoneTransform() => FindWidgetCenterZone();

        /// <summary>World anchor between left and right center cells (index 8), or a single center if only one exists.</summary>
        public static bool TryGetWidgetCenterMidpoint(out Vector3 midpoint)
        {
            midpoint = default;
            var cubes = TwoZoneCubeControllers.Instance;
            if (cubes == null)
                return false;

            var rightCenter = FindCenterZoneUnder(cubes.right != null ? cubes.right.transform : null);
            var leftCenter = FindCenterZoneUnder(cubes.left != null ? cubes.left.transform : null);

            if (rightCenter != null && leftCenter != null)
            {
                midpoint = (rightCenter.position + leftCenter.position) * 0.5f;
                return true;
            }

            var single = rightCenter ?? leftCenter ?? FindWidgetCenterZone();
            if (single == null)
                return false;

            midpoint = single.position;
            return true;
        }

        public static GameObject TryCreateFromCenterTemplate(Transform parent)
        {
            var zoneTemplate = FindWidgetCenterZone();
            if (zoneTemplate == null || parent == null)
                return null;

            var scalerTemplate = zoneTemplate.parent;
            var root = new GameObject("Skip_Scaler");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            var scaler = root.AddComponent<TwoZoneTriggerScaler>();
            scaler.initScale = 1f;
            scaler.InitAtScale(1f);

            if (scalerTemplate != null && scalerTemplate.GetComponent<TwoZoneTriggerScaler>() != null)
            {
                root.transform.localRotation = scalerTemplate.localRotation;
                ApplyTemplateWorldScale(scalerTemplate, root.transform, parent);
            }

            var zoneObject = Object.Instantiate(zoneTemplate.gameObject, root.transform, false);
            zoneObject.name = "Skip";
            ApplyTemplateWorldScale(zoneTemplate, zoneObject.transform, root.transform);
            zoneObject.transform.localRotation = zoneTemplate.localRotation;
            zoneObject.transform.localPosition = zoneTemplate.localPosition;

            EnsureSkipLabelCanvas(zoneObject, zoneTemplate);
            StripWidgetInteraction(zoneObject, zoneTemplate);
            foreach (var renderer in zoneObject.GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = true;
                if (zoneTemplate != null)
                    renderer.gameObject.layer = zoneTemplate.gameObject.layer;
            }

            zoneObject.AddComponent<TutorialDismissControls>();
            zoneObject.AddComponent<TutorialSkipGrabZone>();
            root.SetActive(true);
            zoneObject.SetActive(true);
            return root;
        }

        private static Transform FindCenterZoneUnder(Transform handRoot)
        {
            if (handRoot == null)
                return null;

            Transform fallback = null;
            foreach (var trigger in handRoot.GetComponentsInChildren<TwoZoneTrigger>(true))
            {
                if (!trigger.IsCenter)
                    continue;
                if (trigger.index == 8)
                    return trigger.transform;
                fallback ??= trigger.transform;
            }

            return fallback;
        }

        private static Transform FindWidgetCenterZone()
        {
            var cubes = TwoZoneCubeControllers.Instance;
            if (cubes == null)
                return null;

            var rightCenter = FindCenterZoneUnder(cubes.right != null ? cubes.right.transform : null);
            if (rightCenter != null)
                return rightCenter;

            var leftCenter = FindCenterZoneUnder(cubes.left != null ? cubes.left.transform : null);
            if (leftCenter != null)
                return leftCenter;

            Transform fallback = null;
            foreach (var trigger in cubes.GetComponentsInChildren<TwoZoneTrigger>(true))
            {
                if (!trigger.IsCenter)
                    continue;
                if (trigger.index == 8)
                    return trigger.transform;
                fallback ??= trigger.transform;
            }

            return fallback;
        }

        private static void ApplyTemplateWorldScale(Transform template, Transform clone, Transform parent)
        {
            var world = template.lossyScale;
            var p = parent.lossyScale;
            clone.localScale = new Vector3(
                SafeDivide(world.x, p.x),
                SafeDivide(world.y, p.y),
                SafeDivide(world.z, p.z));
        }

        private static float SafeDivide(float a, float b) => Mathf.Approximately(b, 0f) ? a : a / b;

        /// <summary>CenterZone clone has no matrix Canvas ancestor; UI text will not render without one.</summary>
        private static void EnsureSkipLabelCanvas(GameObject zoneClone, Transform zoneTemplate)
        {
            var text = zoneClone.GetComponentInChildren<Text>(true);
            if (text == null || text.GetComponentInParent<Canvas>() != null)
                return;

            var templateText = zoneTemplate.GetComponentInChildren<Text>(true);
            var templateCanvas = templateText != null ? templateText.GetComponentInParent<Canvas>() : null;

            var canvasGo = new GameObject("SkipLabelCanvas");
            var canvasRect = canvasGo.AddComponent<RectTransform>();
            canvasGo.transform.SetParent(zoneClone.transform, false);
            canvasGo.transform.localPosition = Vector3.zero;
            canvasGo.transform.localRotation = Quaternion.identity;
            canvasGo.transform.localScale = Vector3.one;

            if (templateCanvas != null)
            {
                var templateRect = templateCanvas.GetComponent<RectTransform>();
                if (templateRect != null)
                    canvasRect.sizeDelta = templateRect.sizeDelta;
            }
            else
                canvasRect.sizeDelta = new Vector2(1000, 1000);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            if (templateCanvas != null)
            {
                canvas.sortingLayerID = templateCanvas.sortingLayerID;
                canvas.sortingOrder = templateCanvas.sortingOrder;
            }

            text.gameObject.SetActive(true);
            text.enabled = true;
            var canvasRenderer = text.GetComponent<CanvasRenderer>();
            if (canvasRenderer != null)
                canvasRenderer.cullTransparentMesh = false;

            text.rectTransform.SetParent(canvasGo.transform, true);
        }

        private static void StripWidgetInteraction(GameObject clone, Transform zoneTemplate)
        {
            foreach (var trigger in clone.GetComponentsInChildren<TwoZoneTrigger>(true))
                Object.Destroy(trigger);

            var templateSwitcher = zoneTemplate != null
                ? zoneTemplate.GetComponentInChildren<LetterSwitcher>(true)
                : null;
            var letterDisplay = clone.GetComponentInChildren<Text>(true);
            var cloneSwitcher = clone.GetComponentInChildren<LetterSwitcher>(true);
            if (letterDisplay != null)
            {
                var skipLabel = letterDisplay.GetComponent<TutorialSkipCellLabel>()
                                ?? letterDisplay.gameObject.AddComponent<TutorialSkipCellLabel>();
                skipLabel.Configure(letterDisplay, templateSwitcher ?? cloneSwitcher);
            }

            if (cloneSwitcher != null)
                Object.Destroy(cloneSwitcher);

            foreach (var switcher in clone.GetComponentsInChildren<LetterSwitcher>(true))
                Object.Destroy(switcher);

            foreach (var hotkeySwitcher in clone.GetComponentsInChildren<HotkeyNameSwitcher>(true))
                Object.Destroy(hotkeySwitcher);
        }
    }
}
