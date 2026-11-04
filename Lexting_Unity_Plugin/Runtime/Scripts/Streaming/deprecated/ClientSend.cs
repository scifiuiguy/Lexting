using SpatialGestureText.Input;
using SpatialGestureText.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SpatialGestureText.Streaming
{
    /// <summary>
    /// Currently this is the Quest client input sender - will need to make an abstract class from this for other sender platforms e.g. iPad in the future
    /// </summary>
	public class ClientSend : MonoBehaviour
	{
        private static void SendUDPData(Packet _packet)
        {
            _packet.WriteLength();
            Lexting.streamReceiver.udp.SendData(_packet);
        }

        private static void SendTCPData(Packet _packet)
        {
            _packet.WriteLength();
            Lexting.streamReceiver.tcp.SendData(_packet);
        }

        public static void WelcomeReceived()
        {
            using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
            {
                _packet.Write(Lexting.streamReceiver.myId);
                SendTCPData(_packet);
            }
        }

        public static async void UDPTestReceived()
        {
            using (Packet _packet = new Packet((int)ClientPackets.udpTestReceived))
            {
                //await Task.Delay(10000);
                //_packet.Write(StreamReceiverClient._.Instance.myId);
                //_packet.WriteLength();
                _packet.Write("Client successfully received a UDP packet.");
                //_packet.WriteLength();
                Debug.Log("Sending UDP data with test confirmation back to server...");
                SendUDPData(_packet);
            }
        }

        public static void SendIndexTipPosition(bool right, Vector3 position)
        {
            if (!Lexting.input.inputActive)
            {
                using (Packet _packet = new Packet((int)ClientPackets.indexTip))
                {
                    _packet.Write(right);
                    _packet.Write(0);
                    _packet.Write(0);
                    _packet.Write(0);
                    Debug.Log("Sending neutral index finger tip pos to server via UDP...");
                    SendUDPData(_packet);
                }
            }
            else {
                using (Packet _packet = new Packet((int)ClientPackets.indexTip))
                {
                    _packet.Write(right);
                    _packet.Write(position.x);
                    _packet.Write(position.y);
                    _packet.Write(position.z);
                    Debug.Log("Sending current index finger tip pos to server via UDP...");
                    SendUDPData(_packet);
                }
            }
        }

        public static void SendGestureInput(bool right, int currentState, int previousState)
        {
            if (!Lexting.input.inputActive) return;
            using (Packet _packet = new Packet((int)ClientPackets.gestureState))
            {
                _packet.Write(right);
                _packet.Write(currentState);
                _packet.Write(previousState);
                Debug.Log("Sending hand, current gesture state, and previous gesture state to server via UDP...");
                SendUDPData(_packet);
            }
        }

        public static void SendRotationStateInput(bool right, int currentRotationState, int previousRotationState)
        {
            if (!Lexting.input.inputActive) return;
            using (Packet _packet = new Packet((int)ClientPackets.rotationState))
            {
                _packet.Write(right);
                _packet.Write(currentRotationState);
                _packet.Write(previousRotationState);
                Debug.Log("Sending hand, current gesture state, and previous gesture state to server via UDP...");
                SendUDPData(_packet);
            }
        }
	}
}
