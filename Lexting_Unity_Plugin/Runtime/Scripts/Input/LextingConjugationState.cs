using SpatialGestureText.Input;
using SpatialGestureText.LetterCube;
using SpatialGestureText.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText
{
    /// <summary>
    /// Maps the orientation of both hands in tandem to a conjugation type, used to conjugate a word during finalization
    /// with the two-zone cube controller.
    /// </summary>
    public class LextingConjugationState : Singleton<LextingConjugationState>
    {
        #region vars
        #region public vars
        public ConjugationState conjugationState;

        public bool IsVerb => conjugationState == ConjugationState.firstSingularVerb
            || conjugationState == ConjugationState.thirdSignularVerb || conjugationState == ConjugationState.pastVerb
            || conjugationState == ConjugationState.presentProgressiveVerb;

        public bool IsNoun => conjugationState == ConjugationState.pluralNoun || conjugationState == ConjugationState.possessiveNoun
            || conjugationState == ConjugationState.singularNoun;
        #endregion public vars
        #region private vars
        /// <summary>
        /// When rotation state is refreshed, cache it here so we can determine left from right as we compare both hands
        /// </summary>
        private CubeRotationState _currentRotationState;

        /// <summary>
        /// Whether current is left or right, we need refs to both hands. This is the non-current other hand.
        /// </summary>
        private CubeRotationState _otherRotationState => (_currentRotationState != Lexting.rhRotState) ? Lexting.rhRotState : Lexting.lhRotState;

        /// <summary>
        /// A cached reference to current state when state is potentially about to change, used to execute OnConjugationChanged
        /// </summary>
        private ConjugationState _previousState;
        #endregion private vars
        #endregion vars

        #region init
        #endregion init

        #region event handlers
        #endregion event handlers

        private void Start()
        {
            RegisterAllEventsAndActions();
        }

        private void RegisterAllEventsAndActions()
        {
            this.GetGestures((right, current, previous) => { }, 
                (right, current, previous) => {
                    _currentRotationState = Lexting.input.currentRotationState(right);

                    if (_currentRotationState == CubeRotationState.relax && _otherRotationState == CubeRotationState.relax)
                        conjugationState = ConjugationState.firstSingularVerb;
                    else if (_currentRotationState == CubeRotationState.supinate && _otherRotationState == CubeRotationState.supinate)
                        conjugationState = ConjugationState.pastVerb;
                    else if (_currentRotationState == CubeRotationState.pronate && _otherRotationState == CubeRotationState.pronate)
                        conjugationState = ConjugationState.pluralNoun;
                    else if (right) UpdateStateWhenHandOrientationDoesNotMatch(_otherRotationState, _currentRotationState);
                    else UpdateStateWhenHandOrientationDoesNotMatch(_currentRotationState, _otherRotationState);

                    //Debug.Log($"Conjugation State for " +
                    //    ((right) ? "Right Hand" : "Left Hand" ) + $" Updated to: {conjugationState}");
                });
        }

        private void UpdateStateWhenHandOrientationDoesNotMatch(CubeRotationState leftState, CubeRotationState rightState)
        {
            if (leftState == CubeRotationState.pronate && rightState == CubeRotationState.relax)
                conjugationState = ConjugationState.thirdSignularVerb;
            else if (leftState == CubeRotationState.supinate && rightState == CubeRotationState.relax)
                conjugationState = ConjugationState.presentProgressiveVerb;
            else if (leftState == CubeRotationState.relax && rightState == CubeRotationState.pronate)
                conjugationState = ConjugationState.singularNoun;
            else if (leftState == CubeRotationState.supinate && rightState == CubeRotationState.pronate)
                conjugationState = ConjugationState.possessiveNoun;
            else if (leftState == CubeRotationState.relax && rightState == CubeRotationState.supinate)
                conjugationState = ConjugationState.adjective;
            else if (leftState == CubeRotationState.pronate && rightState == CubeRotationState.supinate)
                conjugationState = ConjugationState.adverb;
        }
    }


    public enum ConjugationState
    { 
        firstSingularVerb,          // left & right both palm in
        thirdSignularVerb,          // left palm down, right palm in
        presentProgressiveVerb,     // left palm up, right palm in
        pastVerb,                   // left & right both palm up
        singularNoun,               // left palm in, right palm down
        pluralNoun,                 // left & right both palm down
        possessiveNoun,             // left palm up, right palm down
        adjective,                  // left palm in, right palm up
        adverb                      // left palm down, right palm up
    }
}
