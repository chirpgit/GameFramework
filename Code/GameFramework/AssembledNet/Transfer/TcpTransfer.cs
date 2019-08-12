using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace AssembledNet
{
    [StructLayout(LayoutKind.Explicit)]
    public struct DataPackage
    {
        [FieldOffset(0)]
        public Int32 length;
        [FieldOffset(0)][MarshalAs(UnmanagedType.ByValArray,SizeConst = 4)]
        public byte[] package;
        [FieldOffset(4)][MarshalAs(UnmanagedType.ByValArray)]
        public byte[] data;
        public DataPackage(int len)
        {
            data = new byte[4 + len];
            package = new byte[4];
            length = len;
        }
    }

    public class TcpTransfer
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

        private void Send(Socket socket, byte[] data)
        {
            Int32 length = (Int32)data.Length;
            //消息，前4位为表示消息长度的消息头
            byte[] buffer = new byte[length + HEAD_LEN];
            //big endian 消息头
            buffer.SetBigEndian(length);
            Buffer.BlockCopy(data, 0, buffer, HEAD_LEN, length);
            int bufferLength = buffer.Length;
            int sendedLength = 0;
            do
            {
                sendedLength += socket.Send(buffer, sendedLength, bufferLength - sendedLength, SocketFlags.None);
            } while (bufferLength - sendedLength > 0);
        }
    }
}
