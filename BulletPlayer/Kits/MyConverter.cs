using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulletPlayer.Kits
{
    public static class MyConverter
    {
        public static byte[] IntToBytes(int input) 
        {
            return BitConverter.GetBytes(input).Reverse().ToArray();
        }

        public static byte[] UshortToBytes(ushort input)
        {
            return BitConverter.GetBytes(input).Reverse().ToArray();
        }
    }
}
