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
    /// Contains math utility and constants.
    /// </summary>
    public class MathUtil
    {
        public const float PI = (float)Math.PI;
        public static readonly Vector3 Vec3Zero = new Vector3(0, 0, 0);

        /// <summary>
        /// Schlick's approximation for refractions.
        /// https://en.wikipedia.org/wiki/Schlick%27s_approximation
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Schlick(float cosine, float ri)
        {
            float r0 = (1f - ri) / (1f + ri);
            r0 *= r0;
            return r0 + (1f - r0) * Mathf.Pow(1f - cosine, 5);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Reflect(Vector3 dir, Vector3 normal)
        {
            return Vector3.Normalize(dir - 2 * Vector3.Dot(dir, normal) * normal);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Refract(Vector3 v, Vector3 n, float nint, out Vector3 outRefracted)
        {
            float dt = Vector3.Dot(v, n);
            float discr = 1.0f - nint * nint * (1 - dt * dt);
            if (discr > 0)
            {
                outRefracted = nint * (v - n * dt) - n * Mathf.Sqrt(discr);
                return true;
            }
            outRefracted = new Vector3(0, 0, 0);
            return false;
        }
    }
}
