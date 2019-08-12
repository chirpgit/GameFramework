using System;
using System.Collections.Generic;

namespace Game.Network
{
    /// <summary>
    /// 套接字代理，解析发送包和接收包
    /// </summary>
    public class SocketProxy : IDisposable
    {
        private SocketWrapperBase _socketWrapper;
        
        private Action<NetMsg> _receive;
        public event Action<NetMsg> Receive
        {
            add { _receive += value; }
            remove { _receive -= value; }
        }

        public SocketProxy(SocketWrapperBase socketWrapper)
        {
            _socketWrapper = socketWrapper;
            _socketWrapper.ReceiveEvent += handleReceive;
        }

        public bool Send (NetMsg msg)
        {
            byte[] data = msg.Binary;
            return _socketWrapper.Send(data);
        }

        virtual protected void handleReceive(byte[] data)
        {
            NetMsg netMsg = new NetMsg();
            netMsg.Binary = data;
            if(_receive != null)
            {
                _receive(netMsg);
            }
        }

        public void Dispose()
        {
            _socketWrapper.ReceiveEvent -= handleReceive;
        }
    }    
}