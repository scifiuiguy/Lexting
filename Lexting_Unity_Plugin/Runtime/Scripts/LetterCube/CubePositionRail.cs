using SpatialGestureText.Zones;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.LetterCube
{
    [RequireComponent(typeof(ZoneHighlightHandler))]
    public class CubePositionRail : MonoBehaviour
    {
        #region vars
        private ZoneHighlightHandler _highlightHandler;
        
        [SerializeField] public Axis axis;
        public int zoneIndex => _highlightHandler.zoneIndex;
        #endregion vars

        #region init
        private void Awake()
        {
            _highlightHandler = GetComponent<ZoneHighlightHandler>();
        }
        #endregion init
    }

    
}
