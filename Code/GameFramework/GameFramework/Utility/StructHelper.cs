using System;
using System.Runtime.InteropServices;

namespace Game.Utility
{
    public static class StructHelper
    {
        public static byte[] StructToBytes(object structObj)
        {
            int num = Marshal.SizeOf(structObj);
            IntPtr intPtr = Marshal.AllocHGlobal(num);
            byte[] result;
            try
            {
                Marshal.StructureToPtr(structObj, intPtr, false);
                byte[] array = new byte[num];
                Marshal.Copy(intPtr, array, 0, num);
                result = array;
            }
            finally
            {
                Marshal.FreeHGlobal(intPtr);
            }
            return result;
        }

        public static object BytesToStruct(byte[] bytes, Type strcutType)
        {
            int num = Marshal.SizeOf(strcutType);
            IntPtr intPtr = Marshal.AllocHGlobal(num);
            object result;
            try
            {
                Marshal.Copy(bytes, 0, intPtr, num);
                result = Marshal.PtrToStructure(intPtr, strcutType);
            }
            finally
            {
                Marshal.FreeHGlobal(intPtr);
            }
            return result;
        }

        public static void SetBigEndianHead(ref byte[] data, Int32 num)
        {
            data[0] = (byte)((num >> 24));
            data[1] = (byte)((num >> 16));
            data[2] = (byte)((num >> 8));
            data[3] = (byte)num;
        }

        public static Int32 GetBigEndianHead(byte[] data)
        {
            if (data!=null && data.Length >= 4)
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
