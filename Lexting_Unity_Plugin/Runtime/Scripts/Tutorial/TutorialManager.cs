using Pixelplacement;
using SpatialGestureText.Input;
using SpatialGestureText.Interfaces;
using SpatialGestureText.LetterCube;
using SpatialGestureText.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace SpatialGestureText.Tutorial
{
    public class TutorialManager : Singleton<TutorialManager>
    {
        #region vars
        [Range(0,1)]
        [SerializeField] public float tutorialOpacity = 0.4f;
        public bool tutorialCompleted;
        public int totalPages {
            get {
                return pageCanvasGroups.Count;
            }
        }
        public int currentPage;

        //public UnityAction OnTutorialCompleted = new UnityAction(() => { });
        public SpatialGestureEvent OnTutorialCompleted = new SpatialGestureEvent();


        private SpatialGestureInput _input;
        private CanvasGroup canvasGroup;
        private List<CanvasGroup> pageCanvasGroups = new List<CanvasGroup>();
        private TutorialPageHandler pageHandler;
        private TutorialPersistentElementsHandler persistentElementsHandler;
        private TutorialHandHandler[] handHandlers;
        private TutorialCheckMarkHandler[] checkmarkHandlers;
        private BullseyeHandler bullseyeHandler;
        private bool leftHandReady, rightHandReady;
        #endregion vars

        #region init
        private void Awake()
        {
            _input = SpatialGestureInput.Instance;
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            
            this.Ch(ref pageHandler, ref persistentElementsHandler, ref bullseyeHandler);
            this.Chs(ref handHandlers, ref checkmarkHandlers);
            pageCanvasGroups = pageHandler.GetComponentsInChildren<CanvasGroup>().ToList();
            this.Subscribe(_input.OnSinglePopGunActive, OnSinglePopGunActive);
            this.Subscribe(_input.OnDualPopGunActive, OnDualPopGunActive);

            this.WaitS(1, () => {
                if (LextingController.Instance.isInputStreamReceiver)
                {
                    Dismiss(() => ResetPages());
                    OnTutorialCompleted.I();
                }
            });
        }

        private void Start()
        {
            Introduce();
        }

        //private void OnEnable()
        //{
        //    _input.OnSinglePopGunActive += OnSinglePopGunActive;
        //    _input.OnDualPopGunActive += OnDualPopGunActive;
        //}

        //private void OnDisable()
        //{
        //    if (_input != null)
        //    {
        //        _input.OnSinglePopGunActive -= OnSinglePopGunActive;
        //        _input.OnDualPopGunActive -= OnDualPopGunActive;
        //    }
        //}
        #endregion init

        #region event handlers
        private void OnSinglePopGunActive(bool on, bool right)
        {
            Debug.Log($"TutorialManager received OnSinglePopGunActive: on/off? {on} " + ((right) ? "right" : "left"));
            if (right) rightHandReady = on;
            else leftHandReady = on;

            if (right && rightHandReady && !checkmarkHandlers[1].checkOn)
                checkmarkHandlers[1].DrawCheckmark(true);

            if (!right && leftHandReady && !checkmarkHandlers[0].checkOn)
                checkmarkHandlers[0].DrawCheckmark(true);

            if (currentPage == 0 && leftHandReady && rightHandReady)
            {
                Lexting.audio.Play("Success");
                bullseyeHandler.Bump();
                StartCoroutine(TurnFirstTwoPages());
            }
        }

        private void OnDualPopGunActive(bool on)
        {
            if (currentPage == 2)
            {
                if (!checkmarkHandlers[1].checkOn)
                    checkmarkHandlers[1].DrawCheckmark(true);
                if (!checkmarkHandlers[0].checkOn)
                    checkmarkHandlers[0].DrawCheckmark(true);
                Lexting.audio.Play("Success_Mid");
                bullseyeHandler.Bump();
                StartCoroutine(TurnLastTwoPages());
            }
        }
        #endregion event handlers

        #region helper methods
        public void FadePage(int pageNumber, bool on)
        {
            Tween.CanvasGroupAlpha(pageCanvasGroups[pageNumber], (on) ? 1 : 0, 1.0f, 0);
        }

        public void TurnPage()
        {
            FadePage(currentPage, false);
            if (totalPages <= currentPage + 1)
            {
                OnTutorialCompleted.I();
                tutorialCompleted = true;
                checkmarkHandlers[0].DrawCheckmark(false);
                checkmarkHandlers[1].DrawCheckmark(false);

                Dismiss(()=>ResetPages());
            }
            else
            {
                currentPage++;
                FadePage(currentPage, true);
            }
        }

        private IEnumerator TurnFirstTwoPages()
        {
            TurnPage();
            yield return new WaitForSeconds(5);
            checkmarkHandlers[0].DrawCheckmark(false);
            checkmarkHandlers[1].DrawCheckmark(false);
            MoveThumbs();
            TurnPage();
        }

        private IEnumerator TurnLastTwoPages()
        {
            TurnPage();
            yield return new WaitForSeconds(3.5f);
            TurnPage();
        }

        private void Introduce()
        {
            Tween.CanvasGroupAlpha(canvasGroup, tutorialOpacity, 0.5f, 0);
            FadeHands(true);
        }

        private void Dismiss(UnityAction callback)
        {
            Tween.CanvasGroupAlpha(canvasGroup, 0, 0.5f, 0, Tween.EaseInOut, Tween.LoopType.None, null, () => { callback.I(); });
            FadeHands(false);
        }

        private void ResetPages()
        {
            pageCanvasGroups[0].alpha = 1;
            if (pageCanvasGroups.Count > 0)
                for (int i = 1; i < pageCanvasGroups.Count; i++)
                    pageCanvasGroups[i].alpha = 0;
        }

        private void FadeHands(bool on)
        {
            foreach (TutorialHandHandler handHandler in handHandlers)
                handHandler.Fade(on);
        }

        private void MoveThumbs()
        {
            foreach (TutorialHandHandler handHandler in handHandlers)
                handHandler.MoveThumb();
        }
        #endregion helper methods
    }
}
