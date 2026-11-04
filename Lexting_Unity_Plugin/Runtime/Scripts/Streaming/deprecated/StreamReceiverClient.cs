using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using UnityEngine.Networking;
using SpatialGestureText.Input;

namespace SpatialGestureText.Streaming
{
    public class StreamReceiverClient : MonoBehaviour
    {
        public static Pingleton<StreamReceiverClient> _ = new Pingleton<StreamReceiverClient>();
        public static int dataBufferSize = 4096;
        public string serverIP = "127.0.0.1";
        public int port = 12525;
        public int myId = 0;
        public TCP tcp;
        public UDP udp;

        private delegate void PacketHandler(Packet _packet);
        private static Dictionary<int, PacketHandler> packetHandlers;
        private SpatialGestureInput _input;

        private void Awake()
        {
            _input = SpatialGestureInput.Instance;
            RegisterUserInputEvents();
        }

        private void Start()
        {
            tcp = new TCP();
            udp = new UDP();
           
            _.Instance.ConnectToServer();
        }

        public void ConnectToServer()
        {

            
            InitializeClientData();
            tcp.Connect();
        }

        #region event handlers
        private void RegisterUserInputEvents()
        {
            if (myId == 0) // if Quest
            {
                //Debug.Log("Stream client about to set up sender gestures");
                this.GetGestures( // forward hand input update to server relay
                    (right, currentG, previousG) => {
                        //Debug.Log("Sending gesture update to server via UDP");
                        ClientSend.SendGestureInput(right, (int)currentG, (int)previousG);
                    },//OnGestureStateChanged
                    (right, currentR, previousR) => ClientSend.SendRotationStateInput(right, (int)currentR, (int)previousR)//OnRotationStateChanged
                );
            }
        }
        #endregion event handlers

        public class UDP
        {
            public UdpClient socket;
            public IPEndPoint endPoint;

            public UDP()
            {
                endPoint = new IPEndPoint(IPAddress.Parse(_.Instance.serverIP), _.Instance.port);
            }

            public void Connect(int _localPort)
            {
                socket = new UdpClient(_localPort);
                socket.Connect(endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                using (Packet _packet = new Packet())
                {
                    SendData(_packet);
                }
            }

            public void SendData(Packet _packet)
            {
                try {
                    int id = _.Instance.myId;
                    _packet.InsertInt(id);
                    if (socket != null)
                    {
                        socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                    }
                }
                catch (Exception _ex)
                {
                    Debug.Log($"Error sending data to server via UDP: {_ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult _result)
            {
                try {
                    byte[] _data = socket.EndReceive(_result, ref endPoint);
                    socket.BeginReceive(ReceiveCallback, null);

                    if (_data.Length < 4)
                    {
                        return;
                    }

                    HandleData(_data);
                }
                catch (Exception _ex) {

                }
            }

            private void HandleData(byte[] _data)
            {
                using (Packet _packet = new Packet(_data))
                {
                    int _packetLength = _packet.ReadInt();
                    _data = _packet.ReadBytes(_packetLength);
                }

                ThreadManager.ExecuteOnMainThread(() => {
                    using (Packet _packet = new Packet(_data))
                    {
                        int _packetId = _packet.ReadInt();
                        packetHandlers[_packetId](_packet);
                    }
                });
            }
        }


        public class TCP
        {
            public TcpClient socket;
            private Packet _receivedData;
            private NetworkStream stream;
            private byte[] receiveBuffer;

            public void Connect()
            {
                socket = new TcpClient
                {
                    ReceiveBufferSize = dataBufferSize,
                    SendBufferSize = dataBufferSize
                };

                receiveBuffer = new byte[dataBufferSize];
                socket.BeginConnect(_.Instance.serverIP, _.Instance.port, ConnectCallback, socket); 
            }

            private void ConnectCallback(IAsyncResult _result)
            {
                socket.EndConnect(_result);

                if (!socket.Connected)
                {
                    return;
                }

                stream = socket.GetStream();

                _receivedData = new Packet();


                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }

            public void SendData(Packet _packet)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                }
                catch (Exception _ex) {
                    Debug.Log($"Error sending data to server via TCP: {_ex}");
                }
            }


            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    int _byteLength = stream.EndRead(_result);
                    if (_byteLength <= 0) return;
                    byte[] _data = new byte[_byteLength];
                    Array.Copy(receiveBuffer, _data, _byteLength);

                    _receivedData.Reset(HandleData(_data));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, (_newResult) => { }, null);
                }
                catch (Exception _ex)
                {
                    Console.WriteLine($"Error receiving TCP data: {_ex}");
                }
            }

            private bool HandleData(byte[] _data)
            {
                int _packetLength = 0;

                _receivedData.SetBytes(_data);

                if (_receivedData.UnreadLength() >= 4)
                {
                    _packetLength = _receivedData.ReadInt();
                    if (_packetLength <= 0)
                        return true;
                }

                while (_packetLength > 0 && _packetLength <= _receivedData.UnreadLength())
                {
                    byte[] _packetBytes = _receivedData.ReadBytes(_packetLength);
                    ThreadManager.ExecuteOnMainThread(()=> {
                        using (Packet _packet = new Packet(_packetBytes))
                        {
                            int _packetId = _packet.ReadInt();
                            packetHandlers[_packetId](_packet);
                        }
                    });

                    _packetLength = 0;
                    if (_receivedData.UnreadLength() >= 4)
                    {
                        _packetLength = _receivedData.ReadInt();
                        if (_packetLength <= 0)
                            return true;
                    }
                }

                if (_packetLength <= 1)
                    return true;
                return false;
                 
            }
        }

        private void InitializeClientData()
        {
            packetHandlers = new Dictionary<int, PacketHandler>()
                {
                    { (int)ServerPackets.welcome, ClientReceive.Welcome },
                    { (int)ServerPackets.udpTest, ClientReceive.UDPTest },
                    { (int)ServerPackets.indexTip, PCClientReceive.IndexTipPosUpdate },
                    { (int)ServerPackets.indexTipReceiptConf, PCClientReceive.IndexTipPosUpdateReceived },
                    { (int)ServerPackets.gestureState, PCClientReceive.GestureInput },
                    { (int)ServerPackets.rotationState, PCClientReceive.RotationStateInput },
                };
            Debug.Log("StreamReceiverClient - Initialized packets");
        }

        
    }

}