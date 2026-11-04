using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SpatialGestureText.Zones
{
    
    /// <summary>
    /// A component solely for trigger entry/exit detection that serves as a 
    /// child of HighlightableZoneContoller, to which entry/exit detection info is passed upward.
    /// </summary>
    public class ZoneHighlightHandler : MonoBehaviour
    {
        #region vars
        private MeshRenderer[] renderers;
        public int zoneIndex;
        [HideInInspector] public ZoneController controller;
        [HideInInspector] public ZoneMarker marker;
        #endregion vars

        #region init
        private void Awake()
        {
            this.Chs(ref renderers);
            ToggleVisibility(false);
            this.Wait(()=>marker.Ready(), ()=>zoneIndex = marker.transform.GetSiblingIndex());
        }
        #endregion init

        
        #region helper methods
        public void ToggleVisibility(bool on)
        {
            //Debug.Log($"ZoneHighlightHandler - zoneIndex: {zoneIndex} - about to ToggleVisibility to: {on}");
            foreach (MeshRenderer renderer in renderers)
                renderer.enabled = on;
            //Debug.Log($"ZoneHighlightHandler - zoneIndex: {zoneIndex} - just finished ToggleVisibility to: {on}");
        }
        #endregion helper methods
    }
}
