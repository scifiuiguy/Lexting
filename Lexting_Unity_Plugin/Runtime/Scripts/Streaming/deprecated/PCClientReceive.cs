using SpatialGestureText.Input;
using SpatialGestureText.Interfaces;
using SpatialGestureText.LetterCube;
using SpatialGestureText.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace SpatialGestureText.Streaming
{
    /// <summary>
    /// Receives full 6DOF control data including cursor position, hand orientation, and activation gestures.
    /// (as opposed to PCClientReceiveTextOnly, which receives packets containing only keyboard commands)
    /// </summary>
	public class PCClientReceive : MonoBehaviour
	{
        public static void Welcome(Packet _packet)
        {
            string _msg = _packet.ReadString();
            int _myId = _packet.ReadInt();

            Debug.Log($"Message from server: {_msg}");
            Lexting.streamReceiver.myId = _myId;
            ClientSend.WelcomeReceived();

            Lexting.streamReceiver.udp.Connect(((IPEndPoint)Lexting.streamReceiver.tcp.socket.Client.LocalEndPoint).Port);
        }

        public static void UDPTest(Packet _packet)
        {
            string _msg = _packet.ReadString();
            Debug.Log($"Received packet via UDP. Contains msg: {_msg}");
            ClientSend.UDPTestReceived();
        }

        public static void IndexTipPosUpdate(Packet _packet)
        {
            bool right = _packet.ReadBool();
            Vector3 currentIndexPos = new Vector3(_packet.ReadFloat(), _packet.ReadFloat(), _packet.ReadFloat());

            if (currentIndexPos.Equals(Vector3.zero))
            {
                Debug.Log($"Wireless index finger position lost: hand {right} x {currentIndexPos.x} y {currentIndexPos.y} z {currentIndexPos.z}");
                Lexting.rHandMarker.UpdateSmoothDestination(LextingController.Instance.centerPosition - new Vector3(0,0.15f,0));
                Lexting.lHandMarker.UpdateSmoothDestination(LextingController.Instance.centerPosition - new Vector3(0, 0.15f, 0));
            }
            else {
                Debug.Log($"Wireless index finger position update: hand {right} x {currentIndexPos.x} y {currentIndexPos.y} z {currentIndexPos.z}");
                if (right)
                    Lexting.rHandMarker.UpdateSmoothDestination(currentIndexPos);
                else Lexting.lHandMarker.UpdateSmoothDestination(currentIndexPos);
            }
        }

        public static void IndexTipPosUpdateReceived(Packet _packet)
        {

        }

        public static void GestureInput(Packet _packet)
        {
            bool right = _packet.ReadBool();
            
            HandGestureState newState = (HandGestureState)_packet.ReadInt();
            HandGestureState previousState = (HandGestureState)_packet.ReadInt();
            //OculusHandInput.Instance.NetworkGestureUpdateTrigger(right, newState, previousState);
            Debug.Log($"PCClient received a gesture input update from server via UDP: right {right} newState {newState} previousState {previousState}");
        }

        public static void RotationStateInput(Packet _packet)
        {
            bool right = _packet.ReadBool();
            CubeRotationState newState = (CubeRotationState)_packet.ReadInt();
            CubeRotationState previousState = (CubeRotationState)_packet.ReadInt();
            //OculusHandInput.Instance.NetworkRotationUpdateTrigger(right, newState, previousState);
            Debug.Log($"PCClient received a rotation input update from server via UDP: right {right} newState {newState} previousState {previousState}");
        }
    }
}
