using SpatialGestureText.Input;
using SpatialGestureText.LetterCube;
using SpatialGestureText.TwoZoneCube;
using SpatialGestureText.Utilities;
using UnityEngine;

namespace SpatialGestureText.Tutorial
{
    /// <summary>
    /// SKIP cell: hover and activation follow <see cref="HandCursor"/> (same affordance as cube zones).
    /// While a hand hovers skip (after arm delay), widget zone hover is suspended for that hand until exit.
    /// </summary>
    public class TutorialSkipGrabZone : MonoBehaviour
    {
        private const float ArmDelaySeconds = 0.5f;
        private const float CursorHitMaxDistance = 0.03f;

        private static bool _rightBlocksWidget;
        private static bool _leftBlocksWidget;
        private static bool _cursorOverSkipRight;
        private static bool _cursorOverSkipLeft;

        private MeshRenderer _cellRenderer;
        private TwoZoneTriggerHighlight _highlight;
        private TutorialSkipCellLabel _cellLabel;
        private TwoZoneTriggerScaler _scaler;
        private bool _highlightShown;
        private bool _skipConfirmPending;
        private float _armedAt;
        private bool _grabBeganOnSkipRight;
        private bool _grabBeganOnSkipLeft;

        public static bool BlocksWidgetHover(bool right) => right ? _rightBlocksWidget : _leftBlocksWidget;

        public static bool SuppressesCubeHover(bool right) =>
            right ? _cursorOverSkipRight : _cursorOverSkipLeft;

        public static void ReleaseAllWidgetBlocks()
        {
            _rightBlocksWidget = false;
            _leftBlocksWidget = false;
            _cursorOverSkipRight = false;
            _cursorOverSkipLeft = false;
        }

        private void Awake()
        {
            _cellRenderer = GetComponent<MeshRenderer>();
            _highlight = GetComponentInChildren<TwoZoneTriggerHighlight>(true);
            _cellLabel = GetComponentInChildren<TutorialSkipCellLabel>(true);
            _scaler = GetComponentInParent<TwoZoneTriggerScaler>();
        }

        private void OnEnable()
        {
            _armedAt = Time.time + ArmDelaySeconds;
            _grabBeganOnSkipRight = false;
            _grabBeganOnSkipLeft = false;
            _skipConfirmPending = false;
            _cursorOverSkipRight = false;
            _cursorOverSkipLeft = false;
            _highlightShown = false;
            ClearWidgetBlocks();
            _highlight?.Toggle(false);
            _cellLabel?.SetHovered(false);
        }

        private void OnDisable()
        {
            _cursorOverSkipRight = false;
            _cursorOverSkipLeft = false;
            _grabBeganOnSkipRight = false;
            _grabBeganOnSkipLeft = false;
            ClearWidgetBlocks();
            _highlight?.Toggle(false);
            _highlightShown = false;
            _cellLabel?.SetHovered(false);
        }

        private static void ClearWidgetBlocks()
        {
            _rightBlocksWidget = false;
            _leftBlocksWidget = false;
        }

        private void Start()
        {
            this.Wait(() => Lexting.inputManager != null && Lexting.inputManager.hasHead, RegisterGestures);
        }

        private void Update()
        {
            if (!IsCellInteractable())
                return;

            if (Time.time < _armedAt)
                return;

            var bounds = _cellRenderer.bounds;
            UpdateCursorHover(true, CursorOverSkipCell(bounds, HandCursor.rightPosition));
            UpdateCursorHover(false, CursorOverSkipCell(bounds, HandCursor.leftPosition));
        }

        private bool IsCellInteractable()
        {
            return _cellRenderer != null
                   && _cellRenderer.enabled
                   && isActiveAndEnabled
                   && Lexting.controllers != null
                   && Lexting.controllers.gameObject.activeInHierarchy;
        }

        private void RegisterGestures()
        {
            this.GetGestures(OnGestureChanged);
        }

        private void OnGestureChanged(bool right, HandGestureState current, HandGestureState previous)
        {
            if (Time.time < _armedAt)
                return;

            var hovering = IsCursorOverSkip(right);
            if (hovering && current == HandGestureState.Insert && previous != HandGestureState.Insert)
                SetGrabBeganOnSkip(right, true);
            if (!hovering)
                SetGrabBeganOnSkip(right, false);

            if (previous != HandGestureState.Insert || current == HandGestureState.Insert)
                return;
            if (!hovering || !GetGrabBeganOnSkip(right))
                return;

            SetGrabBeganOnSkip(right, false);
            ConfirmSkipSelection();
        }

        private void ConfirmSkipSelection()
        {
            if (_skipConfirmPending)
                return;
            _skipConfirmPending = true;
            Lexting.audio?.Play("SelectionClick");
            if (_scaler != null)
                _scaler.Bump(OnSkipBumpFinished);
            else
                OnSkipBumpFinished();
        }

        private void OnSkipBumpFinished()
        {
            _skipConfirmPending = false;
            GetComponent<TutorialDismissControls>()?.SkipThisSession();
        }

        private void UpdateCursorHover(bool right, bool inside)
        {
            var was = IsCursorOverSkip(right);
            if (was == inside)
                return;

            if (right)
                _cursorOverSkipRight = inside;
            else
                _cursorOverSkipLeft = inside;

            if (inside)
            {
                SetWidgetBlock(right, true);
                SuspendWidgetHoverForHand(right);
            }
            else
            {
                SetGrabBeganOnSkip(right, false);
                if (GetWidgetBlock(right))
                {
                    SetWidgetBlock(right, false);
                    RestoreWidgetHoverForHand(right);
                }
            }

            RefreshHighlight();
        }

        private void RefreshHighlight()
        {
            if (Time.time < _armedAt)
                return;

            var on = _cursorOverSkipRight || _cursorOverSkipLeft;
            if (_highlight == null || on == _highlightShown)
                return;
            _highlightShown = on;
            _highlight.Toggle(on);
            _cellLabel?.SetHovered(on);
            if (on)
                Lexting.audio?.Play("Hoverclick_Hard");
        }

        private bool IsCursorOverSkip(bool right) => right ? _cursorOverSkipRight : _cursorOverSkipLeft;

        private static bool GetWidgetBlock(bool right) => right ? _rightBlocksWidget : _leftBlocksWidget;

        private static void SetWidgetBlock(bool right, bool block)
        {
            if (right)
                _rightBlocksWidget = block;
            else
                _leftBlocksWidget = block;
        }

        private bool GetGrabBeganOnSkip(bool right) => right ? _grabBeganOnSkipRight : _grabBeganOnSkipLeft;

        private void SetGrabBeganOnSkip(bool right, bool value)
        {
            if (right)
                _grabBeganOnSkipRight = value;
            else
                _grabBeganOnSkipLeft = value;
        }

        private static bool CursorOverSkipCell(Bounds bounds, Vector3 cursorWorld)
        {
            if (cursorWorld.sqrMagnitude < 1e-8f)
                return false;

            var cube = TwoZoneCubeControllers.Instance?.transform;
            if (cube == null)
                return false;

            // Skip sits below the widget; do not steal hover while the cursor is on the cube stack.
            if (cursorWorld.y > cube.position.y - 0.05f)
                return false;

            var closest = bounds.ClosestPoint(cursorWorld);
            return (cursorWorld - closest).sqrMagnitude <= CursorHitMaxDistance * CursorHitMaxDistance;
        }

        private static void SuspendWidgetHoverForHand(bool right)
        {
            var trigger = right ? TwoZoneCubeController.currentRightTrigger : TwoZoneCubeController.currentLeftTrigger;
            if (trigger is TwoZoneTrigger zone)
                zone.HandleExit(right);
        }

        private static void RestoreWidgetHoverForHand(bool right)
        {
            var controllers = Lexting.controllers;
            if (controllers == null || !controllers.isActiveAndEnabled)
                return;
            var sub = right ? controllers.right : controllers.left;
            if (sub == null || !sub.isActiveAndEnabled)
                return;
            sub.RefreshHoverAfterDragEnded();
        }
    }
}
