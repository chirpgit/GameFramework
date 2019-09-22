using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace AssembledNet
{
    public interface ITransfer
    {
        Exception ReceiveThread(Socket socket, Queue<byte[]> receiveQueue);
        void Send(Socket socket, byte[] data);
    }
}