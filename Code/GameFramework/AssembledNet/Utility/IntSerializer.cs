using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssembledNet
{
    internal static class Utility
    {

        public static void SetBigEndian(this byte[] data, Int32 lenght)
        {
            data[0] = (byte)((lenght >> 24));
            data[1] = (byte)((lenght >> 16));
            data[2] = (byte)((lenght >> 8));
            data[3] = (byte)lenght;
            //for (int i = 0; i < bit; i++)
            //{
            //    data[i] = (byte)((lenght >> (8 * (bit-1-i))));
            //}
        }

        public static Int32 GetBigEndian(this byte[] data)
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
    }
}
