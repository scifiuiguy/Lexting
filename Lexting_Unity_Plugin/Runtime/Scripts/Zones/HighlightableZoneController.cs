using Pixelplacement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpatialGestureText.Input;
using SpatialGestureText.LetterCube;
using System;
using UnityEngine.Events;
using SpatialGestureText.Tutorial;
using UniRx.Triggers;
using UniRx;
using Pixelplacement.TweenSystem;
using SpatialGestureText.Utilities;
using SpatialGestureText.Interfaces;

namespace SpatialGestureText.Zones
{
    /// <summary>
    /// Controls display, highlight, and selection for activation zones that float in mid-air (3D buttons)
    /// </summary>
    public class HighlightableZoneController : ZoneController
    {
        #region vars
        private ZoneMotionHandler[] _motionHandlers;
        private CanvasGroup[] canvasGroups;
        private TextDepthAccordionHandler[] accordionHandlers;
        private float zoneWidthApart = 0.07f;
        private GameObject _currentUISpawnPosition;
        private TweenBase[] _textAlphaTweens;
        [HideInInspector] public SpatialGestureEvent OnActivateTypingTutorial = new SpatialGestureEvent();
        [HideInInspector] public SpatialGestureEvent OnActivateSpeedTest = new SpatialGestureEvent();
        [HideInInspector] public SpatialGestureEvent OnActivateFreestyle = new SpatialGestureEvent();

        public bool menuIsReady;
        public AnimationCurve zoneEaseBounce;
        public static Pingleton<HighlightableZoneController> _ = new Pingleton<HighlightableZoneController>();
        #endregion vars

        #region init
        protected override void Awake()
        {
            base.Awake();
            this.Wait(() => TutorialManager.Instance.Ready(), () => {
                //if (CubeController.Instance.isInputStreamReceiver)
                //    HandleSelection();
                //else
                //{
                    this.Subscribe(TutorialManager.Instance.OnTutorialCompleted, Introduce);
                    _currentUISpawnPosition = new GameObject("UISpawnPosition");
                    _currentUISpawnPosition.transform.SetParent(transform.parent);
                    _currentUISpawnPosition.transform.localScale = Vector3.one;
                //}
            });

            this.Chs(ref canvasGroups, ref accordionHandlers);
            _textAlphaTweens = new TweenBase[canvasGroups.Length];
            this.ChsFE(ref _motionHandlers, (handler) => InitMotionHandler(handler));

            ToggleAllText(false);
            transform.localPosition = new Vector3(0, -1 * zoneWidthApart, 0);
            transform.localScale = Vector3.zero;
        }
        
        private void InitMotionHandler(ZoneMotionHandler handler)
        {
            handler.zoneWidthApart = zoneWidthApart;
            handler.zoneEaseBounce = zoneEaseBounce;
            handler.Initialize();
        }
        #endregion init

        #region event handlers
        public override void OnZoneEnter(bool right, int index)
        {
            if (_isTransitioning) return;
            base.OnZoneEnter(right, index);
            Lexting.audio.Play("Hoverclick_Soft");
            FadeText(index, true, null);
            LerpAccordion(index, true);
        }

        public override void OnZoneExit(bool right, int index)
        {
            if (index != hoverIndex) return;
            base.OnZoneExit(right, index);
            this.Wait(() => !_isTransitioning, () => {
                FadeText(index, false, null);
                LerpAccordion(index, false);
            });
        }

        protected override void OnGesturesChanged(bool right, HandGestureState current, HandGestureState previous)
        {
            //Debug.Log($"HighlighatbleZoneController - OnGestureChanged - right {right} - current {current} - previous {previous} - hoverIndex {hoverIndex}");
            if (hoverIndex >= 0 && current == HandGestureState.Insert)
                HandleStateChange();
        }
        
        private void HandleStateChange()
        {
            _transitionIndex = hoverIndex;
            HandleSelection();
            Lexting.audio.Play("SelectionClick");
            Bump(()=>FadeText(_transitionIndex, false, ()=> Dismiss()));
        }

        private void HandleSelection()
        {
            switch (hoverIndex)
            {
                case 0: OnActivateTypingTutorial.I();   break;
                case 1: OnActivateSpeedTest.I();        break;
                case 2: OnActivateFreestyle.I();        break;
            }
        }
        #endregion event handlers

        #region helper methods
        public override void Introduce()
        {
            if (LextingController.Instance.isInputStreamReceiver)
            {
                hoverIndex = 1;
                HandleSelection();
            }
            else {
                UpdateUISpawnPosition();
                Tween.LocalPosition(transform, Vector3.zero, 0.2f, 0, zoneEaseBounce, Tween.LoopType.None, null, () => { });
                Tween.LocalScale(transform, Vector3.one, 0.2f, 0, zoneEaseBounce, Tween.LoopType.None, null, () => {
                    _motionHandlers[0].Introduce(() => {/*callback*/});
                    _motionHandlers[1].Introduce(() => {/*callback*/});
                });
            }
        }

        public override void Dismiss()
        {
            menuIsReady = false;
            hoverIndex = -1;
            FadeAllText(false);
            _motionHandlers[0].Dismiss(()=> {
                Tween.LocalPosition(transform, new Vector3(0, -0.07f, 0), 0.1f, 0.1f, Tween.EaseOut, Tween.LoopType.None, null, () => { /*callback*/ });
                Tween.LocalScale(transform, Vector3.zero, 0.1f, 0.1f, Tween.EaseOut, Tween.LoopType.None, null, () => base.Dismiss());
            });
            _motionHandlers[1].Dismiss(()=>{/*callback*/});
        }
        
        private void FadeAllText(bool on)
        {
            for (int i = 0; i < canvasGroups.Length; i++)
                if (canvasGroups[i].alpha > 0)
                    FadeText(i, on, null);
        }

        private void FadeText(int index, bool on, UnityAction callback)
        {
            Tween.CanvasGroupAlpha(canvasGroups[(_isTransitioning)?_transitionIndex:index], (on) ? 1 : 0, 0.5f, 0,
                Tween.EaseInOut,Tween.LoopType.None,null,()=>callback?.I());
        }

        private void ToggleAllText(bool on)
        {
            for (int i = 0; i < canvasGroups.Length; i++)
                ToggleText(i, on);
        }

        private void ToggleText(int index, bool on)
        {
            canvasGroups[index].alpha = (on)?1:0;
        }

        protected override void ToggleAll(bool on)
        {
            base.ToggleAll(on);
            ToggleAllText(on);
        }

        public void UpdateUISpawnPosition()
        {
            if (!Lexting.head.Ready()) return;
            float heightOffsetRelativeToHead = -0.1f;
            float depthOffsetRelativeToHead = 0.6f;
            Vector3 rightwardHorizontal = -1 * Vector3.Cross(Lexting.head.transform.forward, Vector3.up).normalized;
            Vector3 forwardParallelToFloor = Vector3.Cross(rightwardHorizontal, Vector3.up);
            _currentUISpawnPosition.transform.position = Lexting.head.transform.position + Lexting.head.transform.forward * depthOffsetRelativeToHead;
            
            _currentUISpawnPosition.transform.position = 
                new Vector3(
                    _currentUISpawnPosition.transform.position.x, 
                    Lexting.head.transform.position.y + heightOffsetRelativeToHead, 
                    _currentUISpawnPosition.transform.position.z
                );
            
            _currentUISpawnPosition.transform.position -= 0.05f * rightwardHorizontal; 
            if (transform.parent != _currentUISpawnPosition)
                transform.SetParent(_currentUISpawnPosition.transform);
            transform.localPosition = Vector3.zero;
            transform.rotation = Quaternion.Euler(new Vector3(0, Quaternion.LookRotation(-1 * Lexting.head.transform.forward).eulerAngles.y, 0));
        }

        private void LerpAccordion(int index, bool on)
        {
            accordionHandlers[index].LerpAccordion(!on, 0.3f);
        }

        protected override void ResetCurrentHover()
        {
            base.ResetCurrentHover();
            FadeAllText(false);
        }
        #endregion helper methods

        #region Unit Tests
        private IEnumerator UnitTest_Dismiss()
        {
            yield return new WaitForSeconds(4);
            Dismiss();
        }

        private IEnumerator UnitTest_BumpThenDismiss()
        {
            yield return new WaitForSeconds(4);
            hoverIndex = 1;
            Bump(()=>Dismiss());
        }
        #endregion Unit Tests
    }
}
