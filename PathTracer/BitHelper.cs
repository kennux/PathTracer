using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PathTracer
{
    public static class BitHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBit(ref uint value, int bit)
        {
            value = value | (uint)(1 << bit);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnsetBit(ref uint value, int bit)
        {
            value &= (uint)~(1 << bit);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetBit(ref uint value, int bit)
        {
            return (value & (uint)(1 << bit)) != 0;
        }

        public static uint NumberOfSetBits(uint i)
        {
            i = i - ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }
    }
}
