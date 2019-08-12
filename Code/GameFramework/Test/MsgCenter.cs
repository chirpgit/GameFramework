using System;
using System.Collections.Generic;
using System.Linq;
using Game.Network;

namespace Test
{
    public class MsgCenter : NetMsgMgr
    {
        protected override MsgReceiver makeReceiver(int cmd)
        {
            switch (cmd)
            {
                case 1:
                    return new StringReceiver();
                default:
                    return null;
            }
        }

        protected override void pushReceiverHandler(int cmd ,MsgReceiver receiver)
        {
            Console.WriteLine("收到推送：{0}   {1}", cmd, receiver);
        }
    }
}
