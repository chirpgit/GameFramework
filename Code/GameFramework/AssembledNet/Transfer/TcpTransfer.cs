using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace AssembledNet
{
    public class TcpTransfer : ITransfer
    {
        const int HEAD_LEN = 4;

        private void SetBigEndian(byte[] data, Int32 lenght)
        {
            data[0] = (byte)((lenght >> 24));
            data[1] = (byte)((lenght >> 16));
            data[2] = (byte)((lenght >> 8));
            data[3] = (byte)lenght;
        }

        private Int32 GetBigEndian(byte[] data)
        {
            if (data != null && data.Length >= 4)
            {
                return (Int32)data[0] << 24 | (Int32)data[1] << 16 | (Int32)data[2] << 8 | (Int32)data[3];
            }
            else
            {
                return 0;
            }
        }

        public void Send(Socket socket, byte[] data)
        {
            Int32 length = (Int32)data.Length;
            //消息，前4位为表示消息长度的消息头
            byte[] buffer = new byte[length + HEAD_LEN];
            //big endian 消息头
            SetBigEndian(buffer, length);
            Buffer.BlockCopy(data, 0, buffer, HEAD_LEN, length);
            int bufferLength = buffer.Length;
            int sendedLength = 0;
            do
            {
                sendedLength += socket.Send(buffer, sendedLength, bufferLength - sendedLength, SocketFlags.None);
            } while (bufferLength - sendedLength > 0);
        }

        public Exception ReceiveThread(Socket socket, Queue<byte[]> receiveQueue)
        {
            if (socket==null)
            {
                return new ArgumentNullException("Socket is Null.");
            }
            while (true)
            {
                try
                {
                    if (socket.Poll(5, SelectMode.SelectRead) && socket.Available > 4)
                    {
                        int packetLength;
                        byte[] head = new byte[4];
                        socket.Receive(head, 4, SocketFlags.None);
                        packetLength = GetBigEndian(head);
                        byte[] packet = new byte[packetLength];
                        int receivedLength = 0;
                        do
                        {
                            if (socket.Poll(5, SelectMode.SelectRead) && socket.Available > 0)
                            {
                                int rev = socket.Receive(packet, receivedLength, packetLength - receivedLength, SocketFlags.None);
                                if (rev <= 0)
                                {
                                    break;
                                }
                                receivedLength += rev;
                            }
                        } while (receivedLength != packetLength);
                        lock (receiveQueue)
                        {
                            receiveQueue.Enqueue(packet);
                        }
                    }
                }
                catch (Exception e)
                {
                    return e;
                }
            }
        }
    }
}
