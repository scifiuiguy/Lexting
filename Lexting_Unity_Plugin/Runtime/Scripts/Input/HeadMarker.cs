using SpatialGestureText.LetterCube;
using SpatialGestureText.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.Input
{
    /// <summary>
    /// Enables classes to quickly identify headpose
    /// Place this on any stereoscopic camera rig intended for use with SpatialGestureText
    /// </summary>
    public class HeadMarker : Singleton<HeadMarker>
    {
        /// <summary>
        /// if true, there is no controller because this client is streaming input to another client elsewhere
        /// </summary>
        [SerializeField] private bool _isInputStream;

        /// <summary>
        /// The absolute-value angle in degrees at which the headset yaws away from the screen toward the hands when streaming input to PC
        /// </summary>
        [SerializeField] private float _calibrationAngle = 45;

        /// <summary>
        /// Cached position between the hands
        /// </summary>
        private Vector3 _averageHandPos = Vector3.zero;

        [HideInInspector] public GameObject refFrameOrigin;

        public Vector3 userRightDirection = Vector3.zero;

        public Vector3 flatLookDirection => Vector3.Cross(Vector3.up, -1 * transform.right).normalized;

        public Vector3 headRight => Vector3.Cross(Vector3.up, flatLookDirection);

        public Vector3 averageHandPos => (Lexting.rHandMarker.transform.position + Lexting.lHandMarker.transform.position) / 2;

        public Vector3 bodyForward => (averageHandPos - new Vector3(transform.position.x, averageHandPos.y, transform.position.z)).normalized;

        public Vector3 bodyRight => Vector3.Cross(Vector3.up, bodyForward).normalized;

        private void Awake()
        {
            refFrameOrigin = new GameObject("refFrameOrigin");
            //if (_isInputStream)
            //    StartCoroutine(CalibrationLoop());

            if (_isInputStream)
            {
                this.GetGestures((right, current, previous) => { }, (right, current, previous) => {
                    if (current == CubeRotationState.other &&
                        Lexting.lhRotState == CubeRotationState.other && Lexting.rhRotState == CubeRotationState.other)
                    {
                        CalibrateStreamOrigin();
                    }
                });
            }
        }

        #region input stream calibration
        private bool _rapidCalibration = false; 
        private float fastCalibrationInterval = 1;
        private float slowCalibrationInterval = 4;
        Vector3 _avgHandPosTest = Vector3.zero;
        private IEnumerator CalibrationLoop()
        {
            yield return new WaitForSeconds(5);
            while (true)
            {
                // recalibrate
                CalibrateStreamOrigin();
                yield return new WaitForSeconds((_rapidCalibration) ? fastCalibrationInterval : slowCalibrationInterval);
                yield return new WaitUntil(() => Lexting.input.inputActive);
            }
        }

        private void CalibrateStreamOrigin()
        {
            _avgHandPosTest = Lexting.lHandMarker.transform.position + (Lexting.rHandMarker.transform.position - Lexting.lHandMarker.transform.position) / 2;
            _rapidCalibration = Mathf.Abs((_averageHandPos - _avgHandPosTest).magnitude) > 0.5f;
            CalculateRefFrameOrigin();
        }

        /// <summary>
        /// Procedurally determine position of the proxy origin that should match the PC client such that the hands will end up inside the cube
        /// when input is streamed to the PC with hand positions local to that reference.
        /// </summary>
        public void CalculateRefFrameOrigin()
        {
            // The headset is mounted on a stand beside the user, angled at _calibrationAngle away from the PC screen.
            // The target reference frame where we want to place refFrameOrigin is approximately the position of the user's naval.
            // This position must be calculated procedurally because we don't know if user will set the headset up to their right or left.

            _averageHandPos = Lexting.lHandMarker.transform.position + (Lexting.rHandMarker.transform.position - Lexting.lHandMarker.transform.position) / 2;
 
            // get head pos at hand height
            Vector3 _headPos = new Vector3(transform.position.x, _averageHandPos.y, transform.position.z);
            // get hypotenuse of right triangle viewed from above, created by 3 points: head pos, avg. hand pos, and ref frame pos
            float magnitudeNearestHand2Head = Mathf.Abs((_headPos - _averageHandPos).magnitude);
            // hypotenuse magnitude
            float refFrameDistance = /*Mathf.Sin(45) **/ magnitudeNearestHand2Head*2;
            // Find the direction of the user's body from hand pos to prepare to yaw leftward if HMD is to user's left and rightward if HMD is to user's right
            float turnDirection = (RightIsNearest()) ? 1 : -1;
            // From the average hand position, turn _calibrationAngle degrees in turnDirection to face the user
            Quaternion lookRotation = Quaternion.LookRotation(_headPos - _averageHandPos); /** Quaternion.Euler(0, turnDirection * _calibrationAngle, 0);*/

            // The user's naval should be slightly lower than straight ahead of lookRotation, at a range of ~ refFrameDistance... place the refFrameOrigin there.
            refFrameOrigin.transform.position = _averageHandPos + (lookRotation * Vector3.forward).normalized * refFrameDistance + new Vector3(0, 0.3f, 0);
            // Turn refFrameOrigin to face forward
            refFrameOrigin.transform.rotation = lookRotation * Quaternion.Euler(0, 180, 0);

            userRightDirection = Vector3.Cross(Vector3.up, (_averageHandPos - refFrameOrigin.transform.position).normalized);
        }

        /// <summary>
        /// Returns true if right is closer to head, otherwise false
        /// </summary>
        public bool RightIsNearest()
        {
            return Mathf.Abs((Lexting.rHandMarker.transform.position - transform.position).magnitude) <
                Mathf.Abs((Lexting.lHandMarker.transform.position - transform.position).magnitude);
        }
        #endregion input stream calibration
    }
}
