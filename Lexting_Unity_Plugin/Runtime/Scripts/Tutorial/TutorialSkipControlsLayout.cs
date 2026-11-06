using SpatialGestureText.TwoZoneCube;
using SpatialGestureText.Utilities;
using UnityEngine;

namespace SpatialGestureText.Tutorial
{
    /// <summary>
    /// Keeps skip / don't-show 3D controls centered under the live two-zone widget, using the widget's
    /// current layout bounds. Syncs on <see cref="TwoZoneCubeControllers.ControllerActivated"/> and
    /// <see cref="TwoZoneCubePositionController.OnWidgetRepositioned"/> instead of polling.
    /// </summary>
    public class TutorialSkipControlsLayout : MonoBehaviour
    {
        [SerializeField] private float marginBelowWidget = 0.1f;
        [SerializeField] private float horizontalSpacing = 0.25f;

        private Transform _widget;
        private static readonly Vector3[] _corners = new Vector3[4];
        private bool _hooksRegistered;

        public float MarginBelowWidget
        {
            get => marginBelowWidget;
            set => marginBelowWidget = value;
        }

        private void Awake()
        {
            this.Wait(() => TwoZoneCubeControllers.Instance != null, RegisterWidgetHooks);
        }

        private void OnEnable()
        {
            ResolveWidget();
            SyncToWidgetIfActive();
        }

        private void RegisterWidgetHooks()
        {
            if (_hooksRegistered)
                return;
            _hooksRegistered = true;

            var controllers = TwoZoneCubeControllers.Instance;
            this.Subscribe(controllers.ControllerActivated, OnControllerActivated);

            if (controllers.positionController != null)
                this.Subscribe(controllers.positionController.OnWidgetRepositioned, SyncToWidgetIfActive);
        }

        private void OnControllerActivated(bool on)
        {
            if (on)
                SyncToWidgetIfActive();
        }

        private void SyncToWidgetIfActive()
        {
            if (isActiveAndEnabled)
                SyncToWidget();
        }

        private void ResolveWidget()
        {
            if (TwoZoneCubeControllers.Instance != null)
                _widget = TwoZoneCubeControllers.Instance.transform;
        }

        public void SyncToWidget()
        {
            if (_widget == null)
                ResolveWidget();
            if (_widget == null)
                return;

            if (transform.parent != _widget)
                transform.SetParent(_widget, false);

            var bottomLocalY = GetWidgetBottomLocalY(_widget);
            transform.localRotation = Quaternion.identity;
            transform.localPosition = new Vector3(0f, bottomLocalY - marginBelowWidget, 0f);
            LayoutDirectChildrenSideBySide();
        }

        private void LayoutDirectChildrenSideBySide()
        {
            var count = transform.childCount;
            if (count == 0)
                return;

            for (var i = 0; i < count; i++)
            {
                var child = transform.GetChild(i);
                var x = (i - (count - 1) * 0.5f) * horizontalSpacing;
                child.localPosition = new Vector3(x, 0f, 0f);
                child.localRotation = Quaternion.identity;
            }
        }

        private static float GetWidgetBottomLocalY(Transform widget)
        {
            var minY = float.PositiveInfinity;
            var found = false;

            foreach (var rt in widget.GetComponentsInChildren<RectTransform>(true))
            {
                rt.GetWorldCorners(_corners);
                for (var i = 0; i < _corners.Length; i++)
                {
                    var local = widget.InverseTransformPoint(_corners[i]);
                    if (local.y < minY)
                        minY = local.y;
                    found = true;
                }
            }

            if (found)
                return minY;

            foreach (var renderer in widget.GetComponentsInChildren<Renderer>(true))
            {
                var bounds = renderer.bounds;
                var sample = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
                var local = widget.InverseTransformPoint(sample);
                if (local.y < minY)
                    minY = local.y;
                found = true;
            }

            return found ? minY : -0.35f;
        }
    }
}
