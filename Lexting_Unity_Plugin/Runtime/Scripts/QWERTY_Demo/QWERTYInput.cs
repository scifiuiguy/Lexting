using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace SpatialGestureText.QWERTY_Demo
{
	public class QWERTYInput : MonoBehaviour
	{
		#region vars
		#region public vars
		public string currentHighlightedLetter;
		#endregion public vars
		#region private vars
#if LEXTING_FOUND_OCULUS_SDK
		private bool _triggerDown => OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) > 0;
#else
		private bool _triggerDown => false;
#endif
		private QWERTYPresentation _topPresentation;
		private GameObject _cursor;
		private DemoPointerReceiver _currentReceiver;
		private LineRenderer _lRenderer;
		#endregion private vars
		#endregion vars

		#region init
		private void Awake()
		{
			//_topPresentation = QWERTYRoot.Instance.transform.GetComponent<QWERTYPresentation>();
			this.Ch(ref _lRenderer);
			_cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			Destroy(_cursor.GetComponent<Collider>());
			_cursor.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

			this.Raycast<LetterNodeCollider>(
                (instanceHovered) =>
                {
                    currentHighlightedLetter = instanceHovered.letter;
                    instanceHovered.Highlight(true, null);
                },
                (instanceUnhovered) =>
                {
                    currentHighlightedLetter = null;
                    instanceUnhovered.Highlight(false, null);
                });

			this.Raycast<DemoPointerReceiver>((instanceHovered) => { 
				instanceHovered.highlight?.Toggle(true);
				_currentReceiver = instanceHovered;
			}, 
				(instanceUnhovered) => {
					instanceUnhovered.highlight?.Toggle(false);
					_currentReceiver = null;
				});

			RaycastHit hit;
			this.UpdateAsObservable().Subscribe(_ => {
				Physics.Raycast(transform.position, transform.forward, out hit);
				if (_currentReceiver != null)
				{
					_cursor.transform.position = hit.point;
					_lRenderer.SetPositions(new Vector3[2] { new Vector3(0, 0, 0.2f), transform.InverseTransformPoint(hit.point) });
				}
				else
				{
					_cursor.transform.position = Vector3.zero;
					_lRenderer.SetPositions(new Vector3[2] { Vector3.zero, Vector3.zero });
				}
			});
			
			HandleOculusTrigger();
		}
		#endregion init

		#region loops
		IDisposable triggerUpdate;
		private void HandleOculusTrigger()
		{
			//triggerUpdate = this.UpdateAsObservable()
			//	.Subscribe(_ => this.Wait(()=>_triggerDown, ()=> {
			//		// do stuff on trigger down
			//		_topPresentation.Forward();
			//		HandleOculusTriggerReset();
			//	}));
		}

		private void HandleOculusTriggerReset()
		{
			triggerUpdate = this.UpdateAsObservable()
				.Subscribe(_=> this.Wait(() => !_triggerDown, HandleOculusTrigger));
		}
		#endregion loops
		
		#region event handlers
		
		#endregion event handlers
		
		#region helper methods
		
		#endregion helper methods
		
		#region Unit Tests
		
		#endregion Unit Tests
	}
}
