using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Game.Network;
namespace Test
{
    class Program
    {
        static bool isAlive = true;
        static private Net _net;

        static void Main(string[] args)
        {
            _net = new Net("127.0.0.1", 8088, new MsgCenter());
           
            //while (!tcp.connect())
            //{
            //    Console.ReadKey();
            //}
            bool connecting = false;
            bool connected = false;
            new Thread(new ThreadStart(Runing)).Start();
            while (!connected)
            {
                if (!connecting)
                {
                    Console.WriteLine("按任意键连接...");
                    Console.ReadKey();
                    connecting = true;
                    _net.Connect((isConnected) =>
                    {
                        connected = isConnected;
                        connecting = false;
                        Console.WriteLine("连接{0}", isConnected?"成功":"失败");
                    });
                }
                Thread.Sleep(300);
            }


            StringSender sender = new StringSender("Send1");
            Action<MsgReceiver> resListener = (receiver) => {
                Console.WriteLine("收到反馈:{0}", receiver.GetObject<string>());
            };

            Console.ReadKey();
            _net.Request(1, sender, resListener);
            Console.ReadKey();
            _net.Request(1, sender, resListener);
            Console.ReadKey();
            _net.Request(1, sender, resListener);
            Console.ReadKey();
            _net.Close();
            isAlive = false;
            Console.ReadKey();
        }
        
        static private void Runing()
        {
            Console.WriteLine("开始轮询进程！");
            while (isAlive)
            {
                _net.Process();
            }
        }
    }
}
