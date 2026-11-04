using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UniRx;
using UniRx.Triggers;
using UnityEngine.InputSystem;
using SpatialGestureText.Input.OculusIntegration;

namespace SpatialGestureText.QWERTY_Demo
{
	/// <summary>
	/// A simple state machine that toggles states of the QWERTY keyboard 
	/// a la presentation slides.
	/// </summary>
	public class QWERTYPresentation : MonoBehaviour
	{
		#region vars
		#region public vars
		public int currentIndex;
		public int currentChildPresentation;
		public UnityEvent ForwardEvent = new UnityEvent();
		public QWERTYPresentation parentPresentation;
		public bool childCalledBack;
		#endregion public vars
		#region private vars
		private List<QWERTYPresentation> _childPresentations = new List<QWERTYPresentation>();
		private List<QWERTYPresentationCycleIndex> _indices = new List<QWERTYPresentationCycleIndex>();
		[SerializeField] private bool _forwardCustom;
		#endregion private vars
		#endregion vars
		
		#region init
		private void Awake()
		{
			foreach (Transform child in transform)
			{
				if (child.HasComponent<QWERTYPresentationCycleIndex>())
					_indices.Add(child.GetComponent<QWERTYPresentationCycleIndex>());
				if (child.HasComponent<QWERTYPresentation>())
					_childPresentations.Add(child.GetComponent<QWERTYPresentation>());
			}

			if (_childPresentations.Count > 0)
				foreach (QWERTYPresentation presentation in _childPresentations)
					presentation.parentPresentation = this;

			if (_indices.Count > 0)
			{
				foreach (QWERTYPresentationCycleIndex index in _indices)
					index.gameObject.SetActive(false);
				_indices[0].gameObject.SetActive(true);
			}

			if (!_forwardCustom)
			{
#if UNITY_EDITOR
				ActivateWithSpace();
#else
				//this.Subscribe(SimpleOculusHandInput.Instance.OppositeMiddleToPalmTouch, (right) => UnitTest_Forward());	
								
#endif
				this.WaitS(15, UnitTest_Forward);
			}

		}

		
#endregion init

#region helper methods
        public void Forward()
		{
			if (childCalledBack)
			{
				childCalledBack = false;
				SimpleForward();
			}
			else if (_indices.Count > 0 && _indices[currentIndex].transform.HasComponent<QWERTYPresentation>())
				_childPresentations[currentChildPresentation].Forward();
			else if (_forwardCustom)
				ForwardEvent?.I();
			else SimpleForward();
		}

		public void SimpleForward()
		{
			_indices[currentIndex].gameObject.SetActive(false);
			currentIndex = (currentIndex + 1) % _indices.Count;
			_indices[currentIndex].gameObject.SetActive(true);
		}

		public void Backward()
		{
			_indices[currentIndex].gameObject.SetActive(false);
			if (currentIndex == 0)
				currentIndex = _indices.Count;
			else currentIndex -= 1;
			_indices[currentIndex].gameObject.SetActive(true);
		}

		public void ParentCallback()
		{
			if (parentPresentation.Ready())
			{
				parentPresentation?.IncrementChildPresentationIndex();
				//parentPresentation?.SimpleForward();
				parentPresentation.childCalledBack = true;
			}
		}

		public void ParentCallbackForward()
		{
			ParentCallback();
			parentPresentation?.SimpleForward();
		}

		public void IncrementChildPresentationIndex()
		{
			currentChildPresentation = (currentChildPresentation + 1) % _childPresentations.Count;
		}
#endregion helper methods

#region unit tests
		/// <summary>
		/// This method cycles through all slides (only works if there are no child presentations
		/// </summary>
		/// <param name="interval">the time interval between each slide</param>
		public void UnitTest_AutoCycle(float interval)
		{
			this.WaitS(1, ()=> {
				Forward();
				UnitTest_AutoCycle(interval);
			});
		}

		public void UnitTest_Forward()
		{
			Forward();
		}

		private IDisposable spaceLoop;
		private void ActivateWithSpace()
		{
			spaceLoop = this.UpdateAsObservable().Where(_ => Keyboard.current.spaceKey.wasPressedThisFrame).FirstOrDefault().Subscribe(_ => Reset());
		}

		private void Reset()
		{
			UnitTest_Forward();
			spaceLoop = null;
			this.WaitS(1, ActivateWithSpace);
		}

#endregion unit tests
	}
}
