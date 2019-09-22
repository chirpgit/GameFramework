using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace AssembledNet
{
    /// <summary>
    /// 异步接收
    /// </summary>
    public class SocketTransfer : ISocketTransfer
    {
        private readonly Queue<byte[]> _sendQueue = new Queue<byte[]>();
        private readonly Queue<byte[]> _receiveQueue = new Queue<byte[]>();
        private readonly Socket _socket;

        public SocketTransfer(Socket socket)
        {
            _socket = socket;
        }

        /// <summary>
        /// 加入数据到发送队列
        /// </summary>
        /// <param name="data"></param>
        public void AddToSend(byte[] data)
        {
            lock (_sendQueue)
            {
                _sendQueue.Enqueue(data);
            }
        }

        /// <summary>
        /// 尝试取得接收到的数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>是否有数据</returns>
        public bool TryGetReceivedData(out byte[] data)
        {
            lock (_receiveQueue)
            {
                if (_receiveQueue.Count <= 0)
                {
                    data = null;
                    return false;
                }
                data = _receiveQueue.Dequeue();
            }
            return true;
        }

        /// <summary>
        /// 轮询结果
        /// </summary>
        public enum TransferResult
        {
            SendSucceed = 0,
            Idle,
            SendNoSocket,
            Timeout,
            SendException,
            ReceiveSucceed,
            ReceiveNoSocket,
            ZeroAvailable,
            SelectError,
            ReceiveException,
        }

        #region 发送
        /// <summary>
        /// 发送数据，在发送线程中循环调用
        /// </summary>
        /// <param name="data"></param>
        private void Send(Socket socket, byte[] data)
        {
            Int32 length = (Int32)data.Length;
            //消息，前4位为表示消息长度的消息头
            byte[] buffer = new byte[length + 4];
            //big endian 消息头
            buffer.SetBigEndian(length);
            //StructHelper.SetBigEndianHead(ref result, length);
            Buffer.BlockCopy(data, 0, buffer, 4, length);

            //IAsyncResult asyncSend = socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(sendCallback), socket);
            //bool success = asyncSend.AsyncWaitHandle.WaitOne(5000, true);
            //if (!success)
            //{
            //    return SendResult.Timeout;
            //}
            int bufferLength = buffer.Length;
            int sendedLength = 0;
            do
            {
                sendedLength += socket.Send(buffer, sendedLength, bufferLength - sendedLength, SocketFlags.None);
            } while (bufferLength - sendedLength > 0);
        }

        /// <summary>
        /// 尝试发送，
        /// </summary>
        /// <returns></returns>
        public TransferResult TrySend()
        {
            if (_socket == null)
            {
                return TransferResult.SendNoSocket;
            }
            //if (_sendQueue == null)
            //{
            //    return SendResult.NoSendQueue;
            //}
            byte[] data;
            lock (_sendQueue)
            {
                if (_sendQueue.Count <= 0)
                {
                    return TransferResult.Idle;
                }
                data = _sendQueue.Dequeue();
            }
            try
            {
                Send(_socket, data);
            }
            catch (Exception)
            {
                return TransferResult.SendException;
            }
            return TransferResult.SendSucceed;
        }
        #endregion


        #region 接收
        /// <summary>
        /// 接受给定长度的数据包
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="packetLength"></param>
        /// <returns></returns>
        private byte[] ReceiveByLength(Socket socket, int packetLength)
        {
            byte[] packet = new byte[packetLength];
            int receivedLength = 0;

            do
            {
                int rev = socket.Receive(packet, receivedLength, packetLength - receivedLength, SocketFlags.None);
                if (rev <= 0)
                {
                    break;
                }
                receivedLength += rev;
            } while (receivedLength != packetLength);
            return packet;
        }

        /// <summary>
        /// 尝试接收包,在接收线程中循环调用
        /// </summary>
        /// <returns></returns>
        public TransferResult TryReceive()
        {
            if (_socket == null)
            {
                return TransferResult.ReceiveNoSocket;
            }
            //if (_receiveQueue == null)
            //{
            //    return ReceiveResult.NoReceiveQueue;
            //}
            try
            {
                if (_socket.Poll(5, SelectMode.SelectRead))
                {
                    if (_socket.Available == 0)
                    {
                        return TransferResult.ZeroAvailable;
                    }
                    //读取前4个字节（包长度）                            
                    byte[] prefix = ReceiveByLength(_socket, 4);
                    //获取包长度(big endian)
                    int packetLength = prefix.GetBigEndian();
                    //if (logger != null) logger.LogInfo("socket:{0} receive packet，长度：{1}", socket.LocalEndPoint, packetLength);
                    byte[] packet = ReceiveByLength(_socket, packetLength);
                    lock (_receiveQueue)
                    {
                        _receiveQueue.Enqueue(packet);
                    }
                }
                else if (_socket.Poll(5, SelectMode.SelectError))
                {
                    return TransferResult.SelectError;
                }
            }
            catch (Exception)
            {
                return TransferResult.ReceiveException;
            }
            return TransferResult.ReceiveSucceed;
        }
        #endregion
    }
}
