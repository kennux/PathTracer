using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace PathTracer
{
    public struct BoundingBoxSoA : ISoAContainer<BoundingBoxSoA>
    {
        public Vector3[] min;
        public Vector3[] max;
        public Vector3[] center;
        public int objectCount;

        public BoundingBox this[int i]
        {
            get
            {
                return new BoundingBox(min[i], max[i]);
            }
            set
            {
                min[i] = value.min;
                max[i] = value.max;
                center[i] = value.center;
            }
        }

        public BoundingBoxSoA(int count)
        {
            this.objectCount = count;
            this.min = new Vector3[this.objectCount];
            this.max = new Vector3[this.objectCount];
            this.center = new Vector3[this.objectCount];
        }

        public BoundingBoxSoA(BoundingBox[] boundingBoxes)
        {
            this.objectCount = boundingBoxes.Length;
            this.min = new Vector3[this.objectCount];
            this.max = new Vector3[this.objectCount];
            this.center = new Vector3[this.objectCount];

            for (int i = 0; i < this.objectCount; i++)
            {
                this.min[i] = boundingBoxes[i].min;
                this.max[i] = boundingBoxes[i].max;
                this.center[i] = boundingBoxes[i].center;
            }
        }

        public void CreateFromSubsetOfOther(BoundingBoxSoA other, int[] indices)
        {
            this.objectCount = indices.Length;
            this.min = new Vector3[this.objectCount];
            this.max = new Vector3[this.objectCount];
            this.center = new Vector3[this.objectCount];

            for (int i = 0; i < this.objectCount; i++)
            {
                this.min[i] = other.min[indices[i]];
                this.max[i] = other.max[indices[i]];
                this.center[i] = other.center[indices[i]];
            }
        }
    }

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

        public static BoundingBox FromSphere(Vector3 center, float radius)
        {
            Vector3 min = center - (Vector3.One * (radius / 2f));
            Vector3 max = center + (Vector3.One * (radius / 2f));

            return new BoundingBox(min, max);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overlaps(ref Vector3 min, ref Vector3 max, ref Vector3 otherMin, ref Vector3 otherMax)
        {
            if (min.X > otherMax.X || otherMin.X > max.X)
                return false;

            if (min.Y > otherMax.Y || otherMin.Y > max.Y)
                return false;

            if (min.Z > otherMax.Z || otherMin.Z > max.Z)
                return false;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap(ref float a, ref float b)
        {
            float c = a;
            a = b;
            b = c;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RayIntersection(ref Vector3 min, ref Vector3 max, ref Vector3 center, ref Ray r)
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
            return Overlaps(ref this.min, ref this.max, ref box.min, ref box.max);
        }

        public bool RayIntersection(ref Ray r)
        {
            return RayIntersection(ref min, ref max, ref center, ref r);
        }
    }
}
