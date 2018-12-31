using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace PathTracer
{
    /// <summary>
    /// Fast random number generator implementation.
    /// Underlying implementation is XorShift (taken from https://github.com/aras-p/ToyPathTracer).
    /// </summary>
    public static class FastRandom
    {
        public static uint Seed()
        {
            uint state = (uint)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            uint ms = (uint)DateTime.Now.Millisecond;
            if (ms != 0)
                state /= ms;
            return state;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint XorShift32(ref uint state)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 15;
            return state;
        }

        /// <summary>
        /// Returns random float 0-1.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RandomFloat01(ref uint state)
        {
            return (XorShift32(ref state) & 0xFFFFFF) / 16777216.0f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 RandomInUnitDisk(ref uint state)
        {
            return Vector3.Normalize(new Vector3((RandomFloat01(ref state) * 2f) - 1f, (RandomFloat01(ref state) * 2f) - 1f, 0));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 RandomInUnitSphere(ref uint state)
        {
            Vector3 p;
            do
            {
                p = 2.0f * new Vector3(RandomFloat01(ref state), RandomFloat01(ref state), RandomFloat01(ref state)) - new Vector3(1, 1, 1);
            } while (p.LengthSquared() >= 1.0);
            return p;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 RandomUnitVector(ref uint state)
        {
            float z = RandomFloat01(ref state) * 2.0f - 1.0f;
            float a = RandomFloat01(ref state) * 2.0f * MathUtil.PI;
            float r = Mathf.Sqrt(1.0f - z * z);
            float x = Mathf.Sin(a);
            float y = Mathf.Cos(a);
            return new Vector3(r * x, r * y, z);
        }
    }
}
