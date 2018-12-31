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
    }
}
