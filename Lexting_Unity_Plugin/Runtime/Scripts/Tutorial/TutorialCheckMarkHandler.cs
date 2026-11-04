using Pixelplacement;
using SpatialGestureText.LetterCube;
using SpatialGestureText.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SpatialGestureText.Tutorial
{
    public class TutorialCheckMarkHandler : MonoBehaviour
    {
        #region vars
        private LineRenderer lineRenderer;
        private Transform knuckle;
        private Transform end;
        private GameObject knuckleAnchor;
        private GameObject endAnchor;
        private bool _isMoving = false;
        public bool checkOn;
        #endregion vars

        #region init
        private void Awake()
        {
            InitLineRenderer();
            knuckle = transform.GetChild(0);
            end = transform.GetChild(1);
            InitAnchor(ref knuckleAnchor, "KnuckleAnchor", knuckle);
            InitAnchor(ref endAnchor, "EndAnchor", end);
        }

        private void InitLineRenderer()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.SetPositions(
                    new Vector3[] {
                        Vector3.zero,
                        Vector3.zero,
                        Vector3.zero
                    }
                );
        }

        private void InitAnchor(ref GameObject go, string name, Transform parent)
        {
            go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.localPosition = Vector3.zero;
        }
        #endregion init

        #region loops
        private void Update()
        {
            if (_isMoving)
            {
                lineRenderer.SetPositions(
                    new Vector3[] {
                        transform.position,
                        knuckleAnchor.transform.position,
                        endAnchor.transform.position
                    }
                );
            }
        }
        #endregion loops

        #region helper methods
        public void DrawCheckmark(bool on)
        {
            checkOn = on;
            if (_isMoving) return;
            _isMoving = true;
            if (on)
                DrawKnuckle(on, () => DrawEnd(on, () => {
                    _isMoving = false;
                    
                }));
            else DrawEnd(on, () => DrawKnuckle(on, () => {
                _isMoving = false;
            }));
        }

        private void DrawKnuckle(bool on, UnityAction callback)
        {
            if (on)
            {
                Lexting.audio.Play("Whoosh_1");
                knuckleAnchor.transform.position = endAnchor.transform.position = transform.position;
            }
            else
            {
                Lexting.audio.Play("Whoosh_2");
                knuckleAnchor.transform.position = endAnchor.transform.position = knuckle.position;
            }
            
            Tween.Position(knuckleAnchor.transform, (on)?knuckle.position:transform.position, 0.15f, 0, (on)?Tween.EaseIn:Tween.EaseOut, 
                Tween.LoopType.None, null, () => { callback.I(); });
            Tween.Position(endAnchor.transform, (on) ? knuckle.position : transform.position, 0.15f, 0, (on) ? Tween.EaseIn : Tween.EaseOut);
        }

        private void DrawEnd(bool on, UnityAction callback)
        {
            if (on)
            {
                Lexting.audio.Play("Whoosh_2");
                endAnchor.transform.position = knuckle.position;
            }
            else
            {

                Lexting.audio.Play("Whoosh_1");
                endAnchor.transform.position = end.position;
            }
            Tween.Position(endAnchor.transform, (on)?end.position:knuckle.position, 0.15f, 0, (on)?Tween.EaseOut:Tween.EaseIn, 
                Tween.LoopType.None, null, () => { callback.I(); });
        }
        #endregion helper methods

        #region Unit Tests
        public IEnumerator UnitTest_DrawCheckmarkOn()
        {
            DrawCheckmark(true);
            yield break;
        }

        public IEnumerator UnitTest_DrawCheckmarkOff()
        {
            DrawCheckmark(false);
            yield break;
        }
        #endregion Unit Tests
    }
}
