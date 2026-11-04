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
	public class ClientReceive : MonoBehaviour
	{
        public static void Welcome(Packet _packet)
        {
            string _msg = _packet.ReadString();
            int _myId = _packet.ReadInt();

            Debug.Log($"Message from server: {_msg}");
            Lexting.streamReceiver.myId = _myId;
            ClientSend.WelcomeReceived();

            Lexting.streamReceiver.udp.Connect(((IPEndPoint) Lexting.streamReceiver.tcp.socket.Client.LocalEndPoint).Port);
        }

        public static void UDPTest(Packet _packet)
        {
            string _msg = _packet.ReadString();
            Debug.Log($"Received packet via UDP. Contains msg: {_msg}");
            ClientSend.UDPTestReceived();
        }

        public static void IndexTipPosUpdateReceived(Packet _packet)
        {}
    }
}
