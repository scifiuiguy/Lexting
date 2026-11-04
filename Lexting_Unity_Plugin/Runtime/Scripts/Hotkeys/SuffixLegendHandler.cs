using SpatialGestureText.TwoZoneCube;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using SpatialGestureText.Utilities;

namespace SpatialGestureText.Hotkeys
{
	public class SuffixLegendHandler : Singleton<SuffixLegendHandler>
    {
        private Text _display;
        private CanvasGroup _group;

        private void Awake()
        {
            this.Ch(ref _display, ref _group);

            this.GetGestures((right, current, previous) => {}, 
                (right, current, previous) => {
                    if (_group.alpha == 0) return;
                    this.WaitF(1, () => {
                        _display.text = Lexting.conjugator.conjugationState switch
                        {
                            ConjugationState.firstSingularVerb => "v 1st",
                            ConjugationState.thirdSignularVerb => "v 3rd (s)",
                            ConjugationState.presentProgressiveVerb => "v prog. (ing)",
                            ConjugationState.pastVerb => "v past (ed)",
                            ConjugationState.singularNoun => "n single",
                            ConjugationState.pluralNoun => "n plural (s)",
                            ConjugationState.possessiveNoun => "n possess ('s)",
                            ConjugationState.adjective => "adj (ive, al, etc.)",
                            ConjugationState.adverb => "adv (ly, ally, etc.)",
                            _ => throw new NotImplementedException(),
                        };
                    });
                }
            );

            this.Wait(() => Lexting.finalizer.Ready(), () => {
                Lexting.finalizer.ObserveEveryValueChanged(x => x.isSpatialListActive)
                    .Subscribe(active => ToggleVisibility(active));
            });
        }

        public void ToggleVisibility(bool on)
        {
            _group.alpha = (on) ? 1 : 0; 
        }
    }
}
