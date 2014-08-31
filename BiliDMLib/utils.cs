using System;
using System.Linq;

namespace BiliDMLib
{
    public static class utils
    {
        public static byte[] ToBE(this byte[] b)
        {
            if (BitConverter.IsLittleEndian)
            {
               return b.Reverse().ToArray();
            }
            else{return b;}
        }

    }
}