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
        public static void Subdivide(BoundingBox boundingBox, out BoundingBox frontBottomLeft, out BoundingBox frontBottomRight, out BoundingBox frontTopLeft,
            out BoundingBox frontTopRight, out BoundingBox backBottomLeft, out BoundingBox backBottomRight, out BoundingBox backTopLeft, out BoundingBox backTopRight)
        {
            Vector3 size = boundingBox.max - boundingBox.min;
            Vector3 sizeHalf = size * .5f;

            Vector3 _frontBottomLeft = boundingBox.min;
            Vector3 _frontBottomRight = boundingBox.min + new Vector3(sizeHalf.X, 0, 0);
            Vector3 _frontTopLeft = boundingBox.min + new Vector3(0, sizeHalf.Y, 0);
            Vector3 _frontTopRight = boundingBox.min + new Vector3(sizeHalf.X, sizeHalf.Y, 0);
            Vector3 _backBottomLeft = boundingBox.min + new Vector3(0, 0, sizeHalf.Z);
            Vector3 _backBottomRight = boundingBox.min + new Vector3(sizeHalf.X, 0, sizeHalf.Z);
            Vector3 _backTopLeft = boundingBox.min + new Vector3(0, sizeHalf.Y, sizeHalf.Z);
            Vector3 _backTopRight = boundingBox.min + sizeHalf;

            frontBottomLeft = new BoundingBox(_frontBottomLeft, _frontBottomLeft + sizeHalf);
            frontBottomRight = new BoundingBox(_frontBottomRight, _frontBottomRight + sizeHalf);
            frontTopLeft = new BoundingBox(_frontTopLeft, _frontTopLeft + sizeHalf);
            frontTopRight = new BoundingBox(_frontTopRight, _frontTopRight + sizeHalf);
            backBottomLeft = new BoundingBox(_backBottomLeft, _backBottomLeft + sizeHalf);
            backBottomRight = new BoundingBox(_backBottomRight, _backBottomRight + sizeHalf);
            backTopLeft = new BoundingBox(_backTopLeft, _backTopLeft + sizeHalf);
            backTopRight = new BoundingBox(_backTopRight, _backTopRight + sizeHalf);

        }
        public Vector3 min;
        public Vector3 max;
        public Vector3 center;

        public BoundingBox(Vector3 min, Vector3 max)
        {
            this.min = min;
            this.max = max;
            this.center = (min + max) / 2f;
        }

        public bool Overlaps(BoundingBox box)
        {
            if (this.min.X > box.max.X || box.min.X > this.max.X)
                return false;

            if (this.min.Y > box.max.Y || box.min.Y > this.max.Y)
                return false;

            if (this.min.Z > box.max.Z || box.min.Z > this.max.Z)
                return false;

            return true;
        }

        public bool RayIntersection(ref Ray r)
        {
            if (Vector3.Dot(r.direction, (center - r.origin)) < 0)
            {
                // Ray pointing away from the box, only hit if inside the box
                if ((r.origin.X > min.X && r.origin.Y > min.Y && r.origin.Z > min.Z)
                    && (r.origin.X < max.X && r.origin.Y < max.Y && r.origin.Z < max.Z))
                    return true;
                return false;
            }

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
