using AssembledNet.Server;
using System;

namespace SocketServer
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpServer server = new TcpServer();
            server.Start(8088);
        }
    }
}
