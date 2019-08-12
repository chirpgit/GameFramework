using System;
using System.Collections.Generic;
using Game.Utility;

namespace Game.Network
{
    public abstract class SocketWrapperBase
    {
        protected readonly string _host;
        protected readonly int _port;
        private readonly List<byte[]> _sendList;
        private readonly Queue<byte[]> _receiveQueue;

        //连接关闭事件
        private Action _closedListener;
        public event Action ClosedListener
        {
            add { _closedListener += value; }
            remove { _closedListener -= value; }
        }
        //错误事件
        private Action<Exception> _errorListener;
        public event Action<Exception> ErrorListener
        {
            add { _errorListener += value; }
            remove { _errorListener -= value; }
        }
        //接受完整包事件
        private Action<byte[]> _receiveListener;
        public event Action<byte[]> ReceiveEvent
        {
            add { _receiveListener += value; }
            remove { _receiveListener -= value; }
        }

        public SocketWrapperBase(string host, int port)
        {
            _host = host;
            _port = port;
            _sendList = new List<byte[]>();
            _receiveQueue = new Queue<byte[]>();
        }


        /// <summary>
        /// 消息轮询机制调用
        /// </summary>
        public void Process()
        {
            if (IsConnect)
            {
                byte[] data = Dequeue();
                if (data != null)
                {
                    if(_receiveListener != null)
                    {
                        _receiveListener.Invoke(data);
                    }
                }
            }
        }

        private byte[] Dequeue()
        {
            lock (_receiveQueue)
            {
                if (_receiveQueue.Count == 0)
                {
                    return null;
                }
                else
                {
                    return _receiveQueue.Dequeue();
                }
            }
        }

        /// <summary>
        /// 连接
        /// </summary>
        abstract public void Connect(Action<bool> completeListener);

        /// <summary>
        /// 是否已连接
        /// </summary>
        abstract public bool IsConnect { get; }

        /// <summary>
        /// 断开连接
        /// </summary>
        abstract public void Close();

        abstract public bool Send(byte[] data);

        /// <summary>
        /// 处理接收到的包
        /// </summary>
        virtual protected void handleReceiveData(byte[] packet)
        {
            //加入接收队列
            lock (_receiveQueue)
            {
                _receiveQueue.Enqueue(packet);
            }
        }

        protected void onClosing()
        {
            LogHelper.LogInfo("网络正在断开");
        }

        protected void onClosed()
        {
            LogHelper.LogInfo("网络断开了");

            if (_closedListener != null)
            {
                _closedListener.Invoke();
            }
        }

        protected void onError(Exception ex)
        {
            LogHelper.LogErr("网络错误:{0}",ex.ToString());

            if (_errorListener != null)
            {
                _errorListener.Invoke(ex);
            }
        }
    }
}
