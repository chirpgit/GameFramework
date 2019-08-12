using System;
using System.Collections.Generic;
using Game.Utility;

namespace Game.Network
{
    public class Net : IDisposable
    {
        private SocketWrapper _socketWrapper;
        SocketProxy _socketProxy;
        NetMsgMgr _netMsgMgr;
        
        //连接关闭事件
        private Action _closedListener;
        public event Action CloseEvent
        {
            add { _closedListener += value; }
            remove { _closedListener -= value; }
        }
        //错误事件
        private Action _errorListener;
        public event Action ErrorEvent
        {
            add { _errorListener += value; }
            remove { _errorListener -= value; }
        }

        private int _numOfCloseEvent = 0;
        private int _numOfErrorEvent = 0;
        private Queue<KeyValuePair<bool,Action<bool>>> _connectedHandles = new Queue<KeyValuePair<bool, Action<bool>>>();

        public Net(string host, int post, NetMsgMgr msgHandlerMgr)
        {
            _socketWrapper = new SocketWrapper(host, post);
            _socketWrapper.ClosedListener += _socketWrapper_ClosedListener;
            _socketWrapper.ErrorListener += _socketWrapper_ErrorListener;
            _socketProxy = new SocketProxy(_socketWrapper);
            _socketProxy.Receive += _socketProxy_Receive;
            _netMsgMgr = msgHandlerMgr;
        }
        
        private void _socketWrapper_ClosedListener()
        {
            LogHelper.LogInfo("网络关闭了");
            if (_closedListener != null)
            {
                //_closedListener();
                _numOfCloseEvent++;
            }
        }

        private void _socketWrapper_ErrorListener(Exception ex)
        {
            LogHelper.LogErr(ex.ToString());
            if (_errorListener != null)
            {
                //_errorListener();
                _numOfErrorEvent++;
            }
        }

        private void _socketProxy_Receive(NetMsg msg)
        {
            _netMsgMgr.HandleMsg(msg);
        }

        public bool IsConnected { get { return _socketWrapper.IsConnect; } }

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="callback">连接完成回调</param>
        public void Connect(Action<bool> callback)
        {
            _socketWrapper.Connect((connected)=>{
                lock (_connectedHandles)
                {
                    _connectedHandles.Enqueue(new KeyValuePair<bool, Action<bool>>(connected, callback));
                }
            });
        }

        /// <summary>
        /// 事件轮询，由主线程调用
        /// </summary>
        public void Process()
        {
            _socketWrapper.Process();
            //网络连接完成事件
            lock (_connectedHandles)
            {
                if (_connectedHandles.Count > 0)
                {
                    KeyValuePair<bool, Action<bool>> handler = _connectedHandles.Dequeue();
                    if (handler.Value != null)
                    {
                        handler.Value(handler.Key);
                    }
                }
            }
            //网络关闭事件
            if (_numOfCloseEvent > 0 && _closedListener != null)
            {
                _numOfCloseEvent--;
                _closedListener();
            }
            //网络错误事件
            if (_numOfErrorEvent > 0 && _errorListener != null)
            {
                _numOfErrorEvent--;
                _errorListener();
            }
        }

        public bool Request(int cmd, MsgSender sender, Action<MsgReceiver> resListener)
        {
            if (_socketWrapper.IsConnect)
            {
                NetMsg msg = _netMsgMgr.RequestMsg(cmd, sender, resListener);
                if (msg != null)
                {
                    _socketProxy.Send(msg);
                    return true;
                }
            }
            else
            {
                LogHelper.LogErr("网络未连接，无法发送！");
            }
            return false;
        }

        public bool Push(int cmd, MsgSender sender)
        {
            if (_socketWrapper.IsConnect)
            {
                NetMsg msg = _netMsgMgr.TakeMsg(cmd, sender);
                if (msg != null)
                {
                    _socketProxy.Send(msg);
                    return true;
                }
            }
            else
            {
                LogHelper.LogErr("网络未连接，无法发送！");
            }
            return false;
        }

        public void Close()
        {
            _socketWrapper.Close();
        }

        public void Dispose()
        {
            //销毁时移除监听
            _socketWrapper.ClosedListener -= _socketWrapper_ClosedListener;
            _socketWrapper.ErrorListener -= _socketWrapper_ErrorListener;
            _socketProxy.Receive -= _socketProxy_Receive;
        }
    }
}
