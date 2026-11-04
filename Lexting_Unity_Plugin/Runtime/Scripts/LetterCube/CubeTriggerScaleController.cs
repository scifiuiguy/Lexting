using SpatialGestureText.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace SpatialGestureText.LetterCube
{
    public class CubeTriggerScaleController : MonoBehaviour
    {
        private CubeTriggerScaler[] _scalerAssembler;
        private Dictionary<int, CubeTriggerScaler> _scalers = new Dictionary<int, CubeTriggerScaler>();
        [SerializeField] private float timeInterval = 0.002f;
        [SerializeField] private float initScale = 0.5f;
        [SerializeField] private AnimationCurve curve;

        [SerializeField] private bool debugOverride;

        private void Awake()
        {
            if (debugOverride) return;
            this.ChsFE(ref _scalerAssembler, (scaler) => {
                scaler.InitAtScale(initScale);
                _scalers.Add(scaler.index, scaler);
            });

            RegisterEvents();
        }


        private void RegisterEvents()
        {
            this.Subscribe(LextingController.Instance.ControllerActivated, (on) => {
                if (on) StartCoroutine(DelayedIntro(()=> {
                    LextingController.Instance.ToggleCharDisplay(true);
                }));
                else DismissController();
            });
        }

        private IEnumerator DelayedIntro(UnityAction callback)
        {
            yield return new WaitForSeconds(0.5f);
            IntroduceController(callback);
        }

        public void IntroduceController(UnityAction callback)
        {
            StartCoroutine(IntroCoroutine(timeInterval, callback));
        }

        private IEnumerator IntroCoroutine(float interval, UnityAction callback)
        {
            for (int i = 0; i < _scalers.Count; i++)
            {
                _scalers[i].Introduce(curve, () => { });
                yield return new WaitForSeconds(interval);
            }
            yield return new WaitForSeconds(2 * interval);
            callback.I();
        }

        public void DismissController()
        {
            StartCoroutine(DismissCoroutine(timeInterval));
        }

        private IEnumerator DismissCoroutine(float interval)
        {
            for (int i = _scalers.Count - 1; i >= 0; i--)
            {
                _scalers[i].Dismiss(curve, () => { });
                yield return new WaitForSeconds(interval);
            }
        }
    }
}
