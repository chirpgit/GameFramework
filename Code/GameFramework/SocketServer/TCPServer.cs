using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Game.Utility;

namespace SocketServer
{
    public class TCPServer
    {
        private int _port;
        private Socket _serverSocket;
        private Thread _clientConnectThread;
        private bool _running;

        public TCPServer(int port)
        {
            _port = port;
        }

        public void Start()
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(new IPEndPoint(ip, _port));  //绑定IP地址：端口  
            _serverSocket.Listen(10);    //设定最多10个排队连接请求
            _clientConnectThread = new Thread(ListenClientConnect);
            _clientConnectThread.Start();
        }

        /// <summary>  
        /// 监听客户端连接  
        /// </summary>  
        private void ListenClientConnect()
        {
            while (true)
            {
                Socket clientSocket = _serverSocket.Accept();
                TCPSocketReceiver clientReceiver = new TCPSocketReceiver(clientSocket);
                clientReceiver.onError += OnClientError;
                clientReceiver.Start();
                LogHelper.LogInfo("已连接客户端："+ clientSocket.ToString());
            }
        }

        private void OnClientError(Exception ex)
        {
            LogHelper.LogInfo("客户端网络错误！");
        }
    }

    public class TCPSocketReceiver
    {
        private Socket _socket;
        private Thread _receiveThread;
        private readonly Queue<byte[]> _receiveQueue = new Queue<byte[]>();
        public Action<Exception> onError;

        private bool _receiving = false;

        public TCPSocketReceiver(Socket socket)
        {
            _receiveThread = new Thread(ReceiveProcess);
            _socket = socket;
        }

        public void Start()
        {
            _receiving = true;
            _receiveThread.Start();
        }

        private void ReceiveProcess()
        {
            LogHelper.LogInfo("Start Receive");
            while (_receiving)
            {
                lock (this)
                {
                    if (_socket == null) return;
                    try
                    {
                        if (_socket.Poll(5, SelectMode.SelectRead))
                        {
                            if (_socket.Available == 0)
                            {
                                LogHelper.LogErr("Available = 0");
                                break;
                            }

                            //读取前4个字节（包长度）
                            byte[] prefix = new byte[4];
                            int preLength = 0;
                            do
                            {
                                int rev = _socket.Receive(prefix, preLength, 4 - preLength, SocketFlags.None);
                                if (rev <= 0)
                                {
                                    break;
                                }
                                preLength += rev;
                            } while (preLength != 4);

                            //int packetLength = BitConverter.ToInt32(prefix, 0);
                            //获取包长度(big endian)
                            int packetLength = StructHelper.GetBigEndianHead(prefix);
                            LogHelper.LogInfo("socket:{0} receive packet，长度：{1}", _socket.LocalEndPoint, packetLength);

                            byte[] packet = new byte[packetLength];
                            int receiveLength = 0;

                            //接受完整包
                            do
                            {
                                int rev = _socket.Receive(packet, receiveLength, packetLength - receiveLength, SocketFlags.None);
                                if (rev <= 0)
                                {
                                    break;
                                }
                                receiveLength += rev;
                            } while (receiveLength != packetLength);

                            ////判断流是否有Gzip压缩
                            //if (data[0] == 0x1f && data[1] == 0x8b && data[2] == 0x08 && data[3] == 0x00)
                            //{
                            //    data = NetReader.Decompression(data);
                            //}

                            EnqueuePacket(packet);
                        }
                        else if (_socket.Poll(5, SelectMode.SelectError))
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke(ex);
                    }
                }
                Thread.Sleep(5);
            }
            _socket.Close();
        }

        /// <summary>
        /// 处理接收到的包
        /// </summary>
        private void EnqueuePacket(byte[] packet)
        {
            //加入接收队列
            lock (_receiveQueue)
            {
                _receiveQueue.Enqueue(packet);
            }
        }
    }
}
