using System;
using System.Net.Sockets;
using System.Threading;

namespace AssembledNet
{
    public class SocketThread
    {
        private readonly Socket _socket;
        private readonly SocketTransfer _receiver;
        public event Action<SocketTransfer.TransferResult> OnError;
        public event Action<SocketTransfer.TransferResult> OnClose;
        public event Action<SocketTransfer.TransferResult> OnEvent;

        public SocketThread(Socket socket)
        {
            _socket = socket;
            _receiver = new SocketTransfer(socket);
        }

        public void StartThread()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(SendThread));
            ThreadPool.QueueUserWorkItem(new WaitCallback(ReceiveThread));
        }

        private void SendThread(object obj)
        {
            while (true)
            {
                var result = _receiver.TrySend();
                switch (result)
                {
                    case SocketTransfer.TransferResult.SendNoSocket:
                        OnError?.Invoke(result);
                        return;
                    case SocketTransfer.TransferResult.SendException:
                        _socket.Close();
                        OnClose?.Invoke(result);
                        return;
                    case SocketTransfer.TransferResult.Idle:
                        Thread.Sleep(1);
                        break;
                    case SocketTransfer.TransferResult.Timeout:
                        OnEvent?.Invoke(result);
                        break;
                    case SocketTransfer.TransferResult.SendSucceed:
                    default:
                        break;
                }
            }
        }

        private void ReceiveThread(object obj)
        {
            while (true)
            {
                var result = _receiver.TryReceive();
                switch (result)
                {
                    case SocketTransfer.TransferResult.ReceiveNoSocket:
                        OnError?.Invoke(result);
                        return;
                    case SocketTransfer.TransferResult.SelectError:
                    case SocketTransfer.TransferResult.ReceiveException:
                        _socket.Close();
                        OnClose?.Invoke(result);
                        return;
                    case SocketTransfer.TransferResult.ZeroAvailable:
                        Thread.Sleep(1);
                        break;
                    case SocketTransfer.TransferResult.ReceiveSucceed:
                    default:
                        break;
                }
            }
        }
    }
}
