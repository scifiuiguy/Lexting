using SpatialGestureText.TwoZoneCube;
using SpatialGestureText.Utilities;
using UnityEngine;

namespace SpatialGestureText.Tutorial
{
    /// <summary>
    /// Keeps skip controls midway between left and right center keys, with a fixed downward offset.
    /// Syncs on <see cref="TwoZoneCubeControllers.ControllerActivated"/> and
    /// <see cref="TwoZoneCubePositionController.OnWidgetRepositioned"/>.
    /// </summary>
    public class TutorialSkipControlsLayout : MonoBehaviour
    {
        [Tooltip("World-space meters below the TwoZoneCube origin (Unity world −Y).")]
        [SerializeField] private float marginBelowWidget = 0.12f;

        [SerializeField] private float horizontalSpacing = 0.25f;

        private Transform _cubeRoot;
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
            ResolveCubeRoot();
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

        private void ResolveCubeRoot()
        {
            var cubes = TwoZoneCubeControllers.Instance;
            if (cubes != null)
                _cubeRoot = cubes.transform;
        }

        public void SyncToWidget()
        {
            if (_cubeRoot == null)
                ResolveCubeRoot();
            if (_cubeRoot == null)
                return;

            Vector3 anchor;
            if (!TutorialSkipZoneFactory.TryGetWidgetCenterMidpoint(out anchor))
                anchor = _cubeRoot.position;

            var down = _cubeRoot.TransformDirection(Vector3.down) * marginBelowWidget;
            transform.SetPositionAndRotation(anchor + down, _cubeRoot.rotation);
            transform.localScale = Vector3.one;

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
                var keepRotation = child.localRotation;
                child.localPosition = new Vector3(x, 0f, 0f);
                child.localRotation = keepRotation;
            }
        }
    }
}
