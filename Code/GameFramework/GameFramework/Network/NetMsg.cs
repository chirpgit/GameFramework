using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Game.Utility;

namespace Game.Network
{

    /// <summary>
    /// 网络消息,4字节消息头（消息类型或命令id）+数据流
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NetMsg
    {
        protected byte[] _head = new byte[4];
        protected byte[] _body;
        public byte[] Body { get { return _body; } set { _body = value; } }
        public byte[] Binary {
            get {
                int bodyLen = (_body == null) ? 0 : _body.Length;
                byte[] buffer = new byte[4 + bodyLen];
                Buffer.BlockCopy(_head, 0, buffer, 0, 4);
                Buffer.BlockCopy(_body, 0, buffer, 4, bodyLen);
                return buffer;
            }
            set
            {
                int len = value.Length;
                int bodyLen = len - 4;
                Buffer.BlockCopy(value, 0, _head, 0, 4);
                _body = new byte[bodyLen];
                Buffer.BlockCopy(value, 4, _body, 0, bodyLen);
            }
        }
        public Int32 Head
        {
            get
            {
                return StructHelper.GetBigEndianHead(_head);
            }
            set
            {
                StructHelper.SetBigEndianHead(ref _head, value);
            }
        }
    }

    /// <summary>
    /// 协议消息处理器
    /// </summary>
    public abstract class MsgReceiver
    {
        protected byte[] _bin;
        public byte[] Binary { set { _bin = value; } }
        //反序列化
        abstract protected object Deserialize(byte[] bin);
        public T GetObject<T>() {
            if (_bin == null) return default(T);
            return (T)Deserialize(_bin);
        }
        public override string ToString()
        {
            string typeName;
            try
            {
                typeName = (_bin == null) ? "Null" : Deserialize(_bin).GetType().Name;
            }
            catch (Exception)
            {
                typeName = "[ Deserialize Error ]";
            }
            return "[MsgReceiver] Type:" + typeName;
        }
    }

    /// <summary>
    /// 协议消息发送器
    /// </summary>
    public abstract class MsgSender
    {
        protected object _obj;
        //序列化
        abstract protected byte[] Serialize(object obj);
        public byte[] GetBinary()
        {
            if (_obj == null) return null;
            return Serialize(_obj);
        }
        public override string ToString()
        {
            string typeName = (_obj == null)?"Null":_obj.GetType().Name;
            return "[MsgSender] Type:" + typeName;
        }
    }

    /// <summary>
    /// 请求响应组合类
    /// </summary>
    public class RequestResponse
    {
        protected int _cmd;
        protected MsgSender _request;
        protected MsgReceiver _response;
        public MsgReceiver Receiver { get { return _response; } }
        protected Action<MsgReceiver> _responseListener;
        public RequestResponse(int cmd, MsgSender req, MsgReceiver res, Action<MsgReceiver> responseListener)
        {
            _cmd = cmd;
            _request = req;
            _response = res;
            _responseListener = responseListener;
        }

        public RequestResponse SetResponse(byte[] bin)
        {
            _response.Binary = bin;
            return this;
        }

        public void Response()
        {
            if (_responseListener != null)
            {
                _responseListener(_response);
            }
        }
        public override string ToString()
        {
            return "[RequestResponse] cmd:"+_cmd;
        }
    }
}
