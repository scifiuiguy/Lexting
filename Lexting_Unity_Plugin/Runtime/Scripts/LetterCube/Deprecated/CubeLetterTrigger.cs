//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//namespace SpatialGestureText.LetterCube
//{
//    // deprecated
//    public class CubeLetterTrigger : MonoBehaviour
//    {
//        #region vars
//        [HideInInspector] public static Action<string> LetterHighlightActivated;

//        public bool isCurrentHighlight;
//        public static Dictionary<int, Dictionary<int, string>> letters;

//        private CubeSlideRail _slideRail;
//        private Vector3 _offset;
//        [SerializeField] private CubeLetterGridIndex _gridIndex;
//        private string _letter
//        {
//            get
//            {
//                Debug.Log(_gridIndex.row + " " + _gridIndex.column);

//                return letters[_gridIndex.column][_gridIndex.row];
//            }
//        }
//        public string currentLetter
//        {
//            get
//            {
//                return _letter;
//            }
//        }
//        private Vector3 _activatorPos;
//        #endregion vars

//        #region init
//        private void Awake()
//        {
//            if (letters == null)
//                letters = new Dictionary<int, Dictionary<int, string>>();
//        }

//        private void Start()
//        {
//            if (letters.Count == 0)
//            {
//                Dictionary<int, string> column0 = new Dictionary<int, string>();
//                Dictionary<int, string> column1 = new Dictionary<int, string>();
//                Dictionary<int, string> column2 = new Dictionary<int, string>();
//                Dictionary<int, string> column3 = new Dictionary<int, string>();
//                Dictionary<int, string> column4 = new Dictionary<int, string>();
//                Dictionary<int, string> column5 = new Dictionary<int, string>();

//                column0.Add(0, "A");
//                column0.Add(1, "E");
//                column0.Add(2, "T");
//                column0.Add(3, "O");
//                column0.Add(4, " ");
//                column1.Add(0, "S");
//                column1.Add(1, "I");
//                column1.Add(2, "N");
//                column1.Add(3, "H");
//                column1.Add(4, " ");
//                column2.Add(0, "L");
//                column2.Add(1, "R");
//                column2.Add(2, "D");
//                column2.Add(3, "C");
//                column2.Add(4, " ");
//                column3.Add(0, "W");
//                column3.Add(1, "U");
//                column3.Add(2, "M");
//                column3.Add(3, "F");
//                column3.Add(4, " ");
//                column4.Add(0, "V");
//                column4.Add(1, "P");
//                column4.Add(2, "G");
//                column4.Add(3, "Y");
//                column4.Add(4, "B");
//                column4.Add(5, " ");
//                column5.Add(0, "Z");
//                column5.Add(1, "X");
//                column5.Add(2, "K");
//                column5.Add(3, "J");
//                column5.Add(4, "Q");
//                column5.Add(5, " ");


//                letters.Add(0, column0);
//                letters.Add(1, column1);
//                letters.Add(2, column2);
//                letters.Add(3, column3);
//                letters.Add(4, column4);
//                letters.Add(5, column5);
//            }

//            _slideRail = CubeSlideRail.Instance;
//            if (_slideRail == null)
//                Debug.LogError("CubeLetterTrigger couldn't locate CubeSlideRail.");
//            _offset = transform.position - _slideRail.transform.position;
//        }

//        #endregion init

//        #region loops
//        private void Update()
//        {
//            if (_activatorPos == null) return;

//            if (isCurrentHighlight)
//            {
//                Vector3 upperParentPos = transform.parent.parent.position;
//                Vector3 parentOffset = upperParentPos - transform.position;
//            }
//        }
//        #endregion loops

//        #region event handlers
//        private void OnTriggerEnter(Collider other)
//        {
//            if (other.tag == "Activator")
//            {
//                LetterHighlightActivated?.Invoke(currentLetter);
//                isCurrentHighlight = true;
//                _activatorPos = other.transform.position;
//                Vector3 upperParentPos = transform.parent.parent.position;
//                Vector3 parentOffset = upperParentPos - transform.position;
//                _slideRail.currentLetterOffset = _offset;
//                _slideRail.currentIndex = _gridIndex;
//                Debug.Log(string.Format("slide currentIndex: {0} " +
//                                        "{1} " +
//                                        "transform name: {2}",
//                                        _slideRail.currentIndex.row,
//                                        _slideRail.currentIndex.column,
//                                        transform.name));
//            }
//        }

//        private void OnTriggerExit(Collider other)
//        {
//            if (other.tag == "Activator")
//            {
//                isCurrentHighlight = false;
//            }
//        }
//        #endregion event handlers

//        #region helpers
//        public static CubeLetterGridIndex GetIndexByLetter(string letter)
//        {
//            foreach (KeyValuePair<int, Dictionary<int, string>> pair in letters)
//            {
//                if (pair.Value.ContainsValue(letter))
//                {
//                    CubeLetterGridIndex result = new CubeLetterGridIndex(0, pair.Key);
//                    result.row = pair.Value.FirstOrDefault(x => x.Value == letter).Key;
//                    return result;
//                }
//            }
//            return new CubeLetterGridIndex(0, 0);
//        }
//        #endregion helpers
//    }
//}


