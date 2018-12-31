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
    /// Bounding box implementation.
    /// </summary>
    public struct BoundingBox
    {
        public Vector3 min;
        public Vector3 max;

        public BoundingBox(Vector3 center, Vector3 size)
        {
            this.min = center - (size / 2f);
            this.max = center + (size / 2f);
        }

        public bool RayIntersection(ref Ray r)
        {
            float tmin = (min.X - r.origin.X) / r.direction.X;
            float tmax = (max.X - r.origin.X) / r.direction.X;

            if (tmin > tmax) Swap(ref tmin, ref tmax);

            float tymin = (min.Y - r.origin.Y) / r.direction.Y;
            float tymax = (max.Y - r.origin.Y) / r.direction.Y;

            if (tymin > tymax) Swap(ref tymin, ref tymax);

            if ((tmin > tymax) || (tymin > tmax))
                return false;

            if (tymin > tmin)
                tmin = tymin;

            if (tymax < tmax)
                tmax = tymax;

            float tzmin = (min.Z - r.origin.Z) / r.direction.Z;
            float tzmax = (max.Z - r.origin.Z) / r.direction.Z;

            if (tzmin > tzmax) Swap(ref tzmin, ref tzmax);

            if ((tmin > tzmax) || (tzmin > tmax))
                return false;

            if (tzmin > tmin)
                tmin = tzmin;

            if (tzmax < tmax)
                tmax = tzmax;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap(ref float a, ref float b)
        {
            float c = a;
            a = b;
            b = c;
        }
    }
}
