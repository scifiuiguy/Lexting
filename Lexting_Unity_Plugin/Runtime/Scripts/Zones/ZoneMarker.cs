using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.Zones
{
    /// <summary>
    /// This marker designates the top-level node of a single UI zone placed somwhere 
    /// in hierarchy under a controller that is capable of being parent to multiple zones.
    /// ZoneHighlightHandler needs this to pass itself down for reference
    /// </summary>
    public class ZoneMarker : MonoBehaviour
    {
        private ZoneHighlightHandler _highlightHandler;

        private void Awake()
        {
            this.Ch(ref _highlightHandler);
            _highlightHandler.marker = this;
        }
    }
}

