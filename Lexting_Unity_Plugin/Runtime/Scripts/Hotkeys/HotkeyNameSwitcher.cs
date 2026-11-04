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
    public class HotkeyNameSwitcher : MonoBehaviour
    {
        #region vars
        #region public vars
        [SerializeField] public List<string> names;
        [HideInInspector] public TwoZoneCubeController controller;
        public bool IsCenter {
            get {
                if (names.Count > 0)
                    return names[0] == "ALPHA CHARACTERS" || names[0] == "NUMBERS" || names[0] == "SPECIAL CHARACTERS";
                else return false;
            }
        }
        #endregion public vars
        #region private vars
        private Text _nameDisplay;
        private CanvasGroup _group;
        #endregion private vars
        #endregion vars

        #region init
        private void Awake()
        {
            if (names == null || names.Count == 0)
                Debug.LogError("The names var of this script needs three values assigned in inspector.");

            PrepareNameData();

            this.Ch(ref _nameDisplay);
            _nameDisplay.text = names[0];
            this.Wait(()=>controller.Ready(), ()=>RegisterAllEventsAndActions());
        }
        #endregion init

        #region event handlers
        private void RegisterAllEventsAndActions()
        {
            Lexting.finalizer.ObserveEveryValueChanged(x => x.isSpatialListActive)
                .Subscribe(active => _group.alpha = active ? 0 : 1);

            this.GetGestures(
                (right, current, previous) =>
                {
                    // gesture state
                    if (right != controller.IsRight) return;
                },
                (right, current, previous) =>
                {
                    // rotation state
                    if (right != controller.IsRight || (int)current > 2) return;
                    _nameDisplay.text = names[(int)current];
                }
            );
        }
        #endregion event handlers

        #region helper methods
        /// <summary>
        /// Unity text objects are terrible at handling newline rules, so we convert spaces to newlines manually.
        /// </summary>
        private void PrepareNameData()
        {
            for (int i = 0; i < names.Count; i++)
            {
                if (names[i].Contains(" "))
                {
                    string output = "";
                    int j = 0;
                    foreach (string subname in names[i].Split(' '))
                    {
                        j++;
                        output += subname;
                        if (j < names[i].Split(' ').Length - 1)
                            output += "\n";
                    }
                    names[i] = output;
                }
            }
        }
        #endregion helper methods
    }
}
