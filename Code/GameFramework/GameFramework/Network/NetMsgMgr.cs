using System;
using System.Collections.Generic;
using Game.Utility;
using System.Text;

namespace Game.Network
{
    public abstract class NetMsgMgr
    {
        private Dictionary<int, RequestResponse> _reqResDict = new Dictionary<int, RequestResponse>();

        public NetMsg TakeMsg(int cmd, MsgSender sender)
        {
            NetMsg msg = new NetMsg();
            msg.Head = cmd;
            msg.Body = sender.GetBinary();
            return msg;
        }

        public NetMsg RequestMsg(int cmd, MsgSender sender, Action<MsgReceiver> responseListener)
        {
            if (_reqResDict.ContainsKey(cmd))
            {
                LogHelper.LogInfo("不要重复发送命令：{0}", cmd);
                return null;
            }
            else
            {
                RequestResponse reqRes = new RequestResponse(cmd, sender, makeReceiver(cmd), responseListener);
                _reqResDict.Add(cmd, reqRes);
                return TakeMsg(cmd, sender);
            }
        }

        public void HandleMsg(NetMsg msg)
        {
            int cmd = msg.Head;
            byte[] body = msg.Body;
            if (_reqResDict.ContainsKey(cmd))
            {
                RequestResponse reqRes = _reqResDict[cmd];
                _reqResDict.Remove(cmd);
                reqRes.SetResponse(body).Response();
            }
            else
            {
                MsgReceiver receiver = makeReceiver(cmd);
                receiver.Binary = body;
                pushReceiverHandler(cmd, receiver);
            }
        }
        /// <summary>
        /// 为Cmd创建对应的MsgHandler，子类实现
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        abstract protected MsgReceiver makeReceiver(int cmd);

        abstract protected void pushReceiverHandler(int cmd, MsgReceiver receiver);
    }
}
