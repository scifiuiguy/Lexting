using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using SpatialGestureText.Utilities;

namespace SpatialGestureText.TwoZoneCube
{
	/// <summary>
	/// Controls the instantiation and display of a number of paging nodes under the currently displayed
	/// conjugated word to signify whether there are more word choices available for the current conjugation.
	/// </summary>
	public class WordPagingController : MonoBehaviour
	{
		#region vars
		#region public vars

		#endregion public vars
		#region private vars
		/// <summary>
		/// A prefab one paging node that will be instantiated on demand by this class whenever appropriate
		/// </summary>
		[SerializeField] private GameObject _pagingNodePrefab;

		/// <summary>
		/// A quick reference list of all the currently instantiated paging nodes so this class won't need
		/// to search for each in child hierarchy upon each getter or setter instruction.
		/// </summary>
		private List<WordPagingNode> _childNodes = new List<WordPagingNode>();

		/// <summary>
		/// used to temporarily reference a given node during instantiation and allocation
		/// </summary>
		private GameObject _currentNodeObject;

		/// <summary>
		/// the node that is currently on
		/// </summary>
		private WordPagingNode _currentNode;

		private int _index => _currentNode.transform.GetSiblingIndex();

		/// <summary>
		/// cached interator
		/// </summary>
		private int _i;

		private HorizontalLayoutGroup _layout;

		/// <summary>
		/// The TwoZoneWordListController receives data from the LextingDictionary indicating whether the 
		/// current conjugation state for the current word has multiple word options.
		/// If it does, the TwoZoneWordListController uses a cycle index for each cycleable category (verb/noun/adj).
		/// Rather than ref each of those indices multiple times here, we redefine them all as a single property.
		/// </summary>
		private int _cycleIndex => (Lexting.conjugator.IsVerb) ? Lexting.finalizer.verbCycleIndex
			: (Lexting.conjugator.IsNoun) ? Lexting.finalizer.nounCycleIndex
				: (Lexting.conjugator.conjugationState == ConjugationState.adjective)
					? Lexting.finalizer.adjCycleIndex : 0;

		private bool _isPossessive => Lexting.conjugator.conjugationState == ConjugationState.possessiveNoun;
		private bool _isMultiPossessive => _isPossessive && Lexting.finalizer.nounHasMultipleRecords;
		#endregion private vars
		#endregion vars

		#region init
		private void Awake()
		{
			this.Ch(ref _layout);
			InitNewNode();
			_currentNode = _childNodes[0];
			HideAll();
			RegisterAllEventsAndActions();
		}

		private void InitNewNode()
		{
			_currentNodeObject = Instantiate(_pagingNodePrefab);
			_currentNodeObject.transform.SetParent(transform.GetChild(0), false);
			_currentNodeObject.transform.localScale = Vector3.one;
			_childNodes.Add(_currentNodeObject.GetComponent<WordPagingNode>());
		}

		private void InitNewNode(bool show)
		{
			_currentNodeObject = Instantiate(_pagingNodePrefab);
			_currentNodeObject.transform.SetParent(transform.GetChild(0), false);
			_currentNodeObject.transform.localScale = Vector3.one;
			_childNodes.Add(_currentNodeObject.GetComponent<WordPagingNode>());
			_currentNodeObject.SetActive(false);
		}

		private void InitNewNodes(int numberOfNewNodes)
		{
			for (_i = 0; _i < numberOfNewNodes; _i++)
				InitNewNode();
		}
		#endregion init

		#region loops

		#endregion loops

		#region event handlers
		private void RegisterAllEventsAndActions()
		{
			Lexting.conjugator.ObserveEveryValueChanged(x=>x.conjugationState)
				.Where(_=>Lexting.finalizer.isSpatialListActive)
				.Subscribe(state => SetUpPaginingForNewConjugation());
			this.Subscribe(Lexting.finalizer.OnRefreshWord, (word) => SetUpPaginingForNewConjugation());
			this.Subscribe(Lexting.finalizer.OnCompleteWordSubmitted, (word) => this.WaitF(2,()=>HideAll()));
		}
		#endregion event handlers

		#region helper methods
		private void SetUpPaginingForNewConjugation()
		{
			
			ToggleAll(false);
			HideAll();
			this.WaitF(1, () => {
				if (Lexting.finalizer.currentHasMultipleRecords || _isMultiPossessive)
				{
					ShowNodes(Lexting.finalizer.currentCycleRange);
					if (!_childNodes[_cycleIndex].nodeOn)
					{
						_currentNode = _childNodes[_cycleIndex];
						_currentNode.ToggleNodeDisplay();
					}
				}
			});
		}

		private void PageForward()
		{
			_currentNode.ToggleNodeDisplay();
			_currentNode = _childNodes[(_index+1)%_childNodes.Count];
			_currentNode.ToggleNodeDisplay();
		}

		private void PageBackward()
		{
			_currentNode.ToggleNodeDisplay();
			_currentNode = _childNodes[(_index + ((_index==0)?_childNodes.Count-1:-1 % _childNodes.Count))];
			_currentNode.ToggleNodeDisplay();
		}

		private void ShowNodes(int numberToShow)
		{
			while (_childNodes.Count < numberToShow)
				InitNewNode(false);

			for (_i = 0; _i < numberToShow; _i++)
				_childNodes[_i].gameObject.SetActive(true);
		}

		private void HideAll()
		{
			foreach (var child in _childNodes)
				child.gameObject.SetActive(false);
		}

		private void ToggleAll(bool on)
		{
			foreach (var child in _childNodes)
				if (on != child.nodeOn)
					child.ToggleNodeDisplay(on);
		}
        #endregion helper methods

        #region Unit Tests
        private void TestInstantiationLoop()
		{
			this.WaitS(1, () => {
				InitNewNode();
				TestInstantiationLoop();
			});
		}

		int iterator = 0;

		private void TestSwitchLoop()
		{
			InitNewNodes(3);
			SwitchLoop();
		}

		private void SwitchLoop()
		{
			this.WaitS(1, () => {
				PageBackward();
				SwitchLoop();
			});
		}
		#endregion Unit Tests
	}
}
