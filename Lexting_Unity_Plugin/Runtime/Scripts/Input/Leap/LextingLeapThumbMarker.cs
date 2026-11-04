#if LEXTING_FOUND_LEAP_SDK
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using SpatialGestureText.Debugging;
using Leap.Unity.Attachments;
using SpatialGestureText.LetterCube;
using SpatialGestureText.Utilities;
using SpatialGestureText.Interfaces;

namespace SpatialGestureText.Input.Leap
{
	/// <summary>
    /// Mostly deprecated - Thumb out trigger was not reliable, too many misfires,
    /// but we're keeping this class in place in case we need to create more thumb-based
    /// gesture recognizers in the future
	/// </summary>
	[System.Obsolete("Leap reference template only; not a shipping path.")]
	public class LextingLeapThumbMarker : MonoBehaviour, IHandPointNode
	{
        #region vars
        #region public vars
        /// <summary>
        /// if true, this is the right hand's thumbe, otherwise left
        /// </summary>
        public bool right { get; set; }
        #endregion public vars
        #region private vars


        /// <summary>
        /// A ref to the tip of the index finger on the same hand
        /// </summary>
        private Transform _leapIndexTip;

        /// <summary>
        /// A ref to the base knuckle of the index finger on the same hand
        /// </summary>
        private Transform _leapIndexBase;

        /// <summary>
        /// The threshold beyond which the thumb's distance from the palm will trigger a thumb-open event
        /// </summary>
        [SerializeField] private float _magnitudeThreshold = 5f;

        /// <summary>
        /// The time interval that must elapse after the ThumbOut even triggers before it can be triggered again
        /// </summary>
        [SerializeField] private float _triggerResetInterval = 0.01f;

        private bool _triggerConditions => _indexThumbGreater && _gestureState != HandGestureState.Insert 
            && Lexting.input.inputActive;

        private bool _triggerReady => _indexThumbLess && _gestureState != HandGestureState.Insert;

        private HandGestureState _gestureState => (right) ? Lexting.rhState : Lexting.lhState;
       
        private float _indexThumbDistance => Mathf.Abs((transform.position - _leapIndexTip.position).magnitude);

        private bool _indexThumbGreater => _indexExtensionRatio > _magnitudeThreshold;

        private bool _indexThumbLess => _indexExtensionRatio < _magnitudeThreshold;

        private float _indexToBaseLength => Mathf.Abs((_leapIndexTip.position - _leapIndexBase.position).magnitude);

        /// <summary>
        /// Should range between ~0.5 to infinite, 0.5 when palm closes, infinite when index and them touch each other 
        /// while away from the index base knuckle - used to distinguish between finger tap and full fist closure
        /// </summary>
        private float _indexExtensionRatio => _indexToBaseLength / _indexThumbDistance;

        #endregion private vars
        #endregion vars

        #region init
        private void Awake()
        {
            _leapIndexTip = Lexting.input.hand(right).indexTip;
            _leapIndexBase = Lexting.input.hand(right).indexBase;

            //this.Wait(() => SpatialGestureInput.Instance.inputActive, () =>
            //    this.WaitS(1, () =>
            //         this.Wait(() => _triggerReady, () => CheckForTrigger())));
        }
        #endregion init

        private void CheckForTrigger()
        {
            //this.Wait(() => _triggerConditions, () => IndexThumbTouch());
        }

        private void IndexThumbTouch()
        {
            //this.WaitS(0.05f, () => { // final accidental trigger failsafe 
                //if (_triggerConditions) // if _triggerConditions stayed true for 0.1 seconds, it prob wasn't accidental
                //{
                //    Debug.Log($"Index point triggered @ time: {Time.time}");
                //    LeapHandInput.Instance.IndexThumbTouch?.I(right);
                //}
                //this.WaitS(_triggerResetInterval, () => this.Wait(() => _triggerReady, () => CheckForTrigger()));
           // });

        }
    }
}
#endif
