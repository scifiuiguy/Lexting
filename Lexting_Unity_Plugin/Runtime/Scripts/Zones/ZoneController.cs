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

namespace SpatialGestureText.Zones
{
    /// <summary>
    /// Base class for highlightable zone objects. Any subclass of this class is the controller for a hoverable 3D button group.
    /// </summary>
    public abstract class ZoneController : MonoBehaviour
    {
        #region vars
        protected ZoneHighlightHandler[] _highlightHandlers;
        protected ZoneMarker[] _zoneMarkers;
        protected int hoverIndex = -1; // -1 if no hover
        protected IDisposable _dragLoop;
        protected bool _isRightDragging, _isLeftDragging;
        protected bool _isRightHovered, _isLeftHovered;
        protected bool _isTransitioning;
        protected int _transitionIndex;
        #endregion vars

        #region init
        protected virtual void Awake()
        {
            this.Chs(ref _zoneMarkers);
            this.ChsFE(ref _highlightHandlers, (handler) => InitHighlightHandler(handler));
            this.GetGestures(OnGesturesChanged);
        }

        private void InitHighlightHandler(ZoneHighlightHandler handler)
        {
            handler.controller = this;
            handler.HandleHandedTriggerWhoseTagContains(
                "Activator",
                (right, entryObject) => OnZoneEnter(right, handler.zoneIndex), 
                (right, exitObject) => OnZoneExit(right, handler.zoneIndex)
            );
        }
        #endregion init

        #region event handlers
        public virtual void OnZoneEnter(bool right, int index)
        {
            if (_isTransitioning) return;
            if (right) _isRightHovered = true;
            else _isLeftHovered = true;

            if (hoverIndex >= 0)
                ResetCurrentHover();

            ToggleHighlight(index, true);
            hoverIndex = index;
            //Debug.Log($"ZoneController - OnZoneEnter - hoverIndex {hoverIndex}");
        }

        public virtual void OnZoneExit(bool right, int index)
        {
            if (index != hoverIndex) return;

            if (right) _isRightHovered = false;
            else _isLeftHovered = false;

            if (_isRightHovered || _isLeftHovered) return;
            
            ToggleHighlight(index, false);
            this.Wait(() =>!_isTransitioning, () => hoverIndex = -1);
        }

        public virtual void OnZoneDragBegan(bool right)
        {
            if (_isRightDragging || _isLeftDragging) return;
            Vector3 currentScale = _zoneMarkers[hoverIndex].transform.localScale;
            BumpIn(currentScale, null);
            //if (hoverIndex >= 0)
            //    ResetCurrentHover();
            if (right) _isRightDragging = true;
            else _isLeftDragging = true;
            _dragLoop = this.UpdateAsObservable().Subscribe(_=>OnZoneDragging(right));
        }

        public virtual void OnZoneDragging(bool right)
        { }

        public virtual void OnZoneDragEnded()
        {
            _isRightDragging = false;
            _isLeftDragging = false;
            _dragLoop.Dispose();
        }

        protected virtual void OnGesturesChanged(bool right, HandGestureState current, HandGestureState previous)
        {
            //if (hoverIndex > 0 && current == HandGestureState.Insert){}
        }
        #endregion event handlers

        #region helper methods
        public virtual void Introduce()
        {
            
        }

        public virtual void Dismiss()
        {
            _isTransitioning = false;
        }

        protected void Bump(UnityAction callback)
        {
            _isTransitioning = true;
            Vector3 currentScale = _zoneMarkers[_transitionIndex].transform.localScale;
            BumpOut(currentScale, ()=>BumpIn(currentScale, ()=>callback.I()));
        }

        private void BumpOut(Vector3 currentScale, UnityAction callback)
        {
            Tween.LocalScale(_zoneMarkers[_transitionIndex].transform, currentScale * 0.8f, 0.05f, 0, 
                Tween.EaseOut, Tween.LoopType.None, null, ()=>callback.I());
        }

        private void BumpIn(Vector3 currentScale, UnityAction callback)
        {
            Tween.LocalScale(_zoneMarkers[_transitionIndex].transform, currentScale, 0.05f, 0, 
                Tween.EaseIn, Tween.LoopType.None, null, () => callback.I());
        }

        private void ToggleAllHighlights(bool on)
        {
            foreach (var highlightHandler in _highlightHandlers)
                highlightHandler.ToggleVisibility(on);
        }

        private void ToggleHighlight(int index, bool on)
        {
            _highlightHandlers[index].ToggleVisibility(on);
        }
        
        protected virtual void ToggleAll(bool on)
        {
            ToggleAllHighlights(on);
        }
        
        protected virtual void ResetCurrentHover()
        {
            foreach (var highlightHandler in _highlightHandlers)
                highlightHandler.ToggleVisibility(false);
            hoverIndex = -1;
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
