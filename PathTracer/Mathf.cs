using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PathTracer
{
    public static class Mathf
    {
        public static float Pow(float x, float y)
        {
#if NET_CORE
            return MathF.Pow(x, y);
#else
            return (float)Math.Pow(x, y);
#endif
        }

        public static float Ceiling(float value)
        {
#if NET_CORE
            return MathF.Ceiling(value);
#else
            return (float)Math.Ceiling(value);
#endif
        }

        public static float Tan(float value)
        {
#if NET_CORE
            return MathF.Tan(value);
#else
            return (float)Math.Tan(value);
#endif
        }

        public static float Sqrt(float value)
        {
#if NET_CORE
            return MathF.Sqrt(value);
#else
            return (float)Math.Sqrt(value);
#endif
        }

        public static float Sin(float value)
        {
#if NET_CORE
            return MathF.Sin(value);
#else
            return (float)Math.Sin(value);
#endif
        }

        public static float Cos(float value)
        {
#if NET_CORE
            return MathF.Cos(value);
#else
            return (float)Math.Cos(value);
#endif
        }

    }
}
