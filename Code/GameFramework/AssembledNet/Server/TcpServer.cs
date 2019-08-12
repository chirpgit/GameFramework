using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AssembledNet.Server
{
    public class TcpServer
    {
        private Socket _serverSocket;
        private Dictionary<string, SocketThread> _clientDict = new Dictionary<string, SocketThread>();

        public TcpServer()
        {
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start(int port)
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            _serverSocket.Bind(new IPEndPoint(ip, port));  //绑定IP地址：端口  
            _serverSocket.Listen(10);    //设定最多10个排队连接请求
            new Thread(ListenClientConnectThread).Start();
        }

        /// <summary>  
        /// 监听客户端连接  
        /// </summary>  
        private void ListenClientConnectThread()
        {
            while (true)
            {
                Socket clientSocket = _serverSocket.Accept();
                var client = new SocketThread(clientSocket);
                _clientDict.Add(clientSocket.RemoteEndPoint.ToString(), client);
                client.OnClose += r => {
                    Console.WriteLine(clientSocket.ToString() + "已断开！");
                };
                client.StartThread();
                Console.WriteLine("已连接客户端：" + clientSocket.ToString());
            }
        }
    }
}
