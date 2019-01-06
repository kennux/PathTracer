using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace PathTracer
{
    public struct TriangleSoA : ISoAContainer<TriangleSoA>
    {
        public Vector3[] p1;
        public Vector3[] p2;
        public Vector3[] p3;

        public Vector3[] n1;
        public Vector3[] n2;
        public Vector3[] n3;

        public Vector3[] faceNormal;
        public Material[] material;
        public BoundingBox[] boundingBoxes;
        public int objectCount;

        public void CreateFromSubsetOfOther(TriangleSoA other, int[] indices)
        {
            this.p1 = new Vector3[indices.Length];
            this.p2 = new Vector3[indices.Length];
            this.p3 = new Vector3[indices.Length];
            this.n1 = new Vector3[indices.Length];
            this.n2 = new Vector3[indices.Length];
            this.n3 = new Vector3[indices.Length];
            this.faceNormal = new Vector3[indices.Length];
            this.material = new Material[indices.Length];
            this.boundingBoxes = new BoundingBox[indices.Length];
            this.objectCount = indices.Length;

            for (int i = 0; i < this.objectCount; i++)
            {
                this.p1[i] = other.p1[indices[i]];
                this.p2[i] = other.p2[indices[i]];
                this.p3[i] = other.p3[indices[i]];

                this.n1[i] = other.n1[indices[i]];
                this.n2[i] = other.n2[indices[i]];
                this.n3[i] = other.n3[indices[i]];

                this.faceNormal[i] = other.faceNormal[indices[i]];
                this.material[i] = other.material[indices[i]];
                this.boundingBoxes[i] = other.boundingBoxes[indices[i]];
            }
        }
    }

    public class TriangleHitSystem : OCTreeSceneHitSystem<Triangle, TriangleSoA>
    {
        private const float Epsilon = float.Epsilon;

        protected override void Raycast(ref State state, ref TriangleSoA container)
        {
            Ray ray = state.ray;
            float minDist = state.minDist, maxDist = state.maxDist;
            int rayIndex = state.rayIndex;
            HitInfo[] hits = state.hits;
            HitInfo hitInfo = new HitInfo();

            for (int i = 0; i < container.objectCount; i++)
            {
                // https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm
                Vector3 e1, e2;
                Vector3 p1 = container.p1[i];
                Vector3 p2 = container.p2[i];
                Vector3 p3 = container.p3[i];

                e1 = p2 - p1;
                e2 = p3 - p1;
                Vector3 h = Vector3.Cross(ray.direction, e2);
                float a = Vector3.Dot(e1, h);
                if (a > -Epsilon && a < Epsilon)
                    continue;

                float f = 1.0f / a;
                Vector3 s = ray.origin - p1;
                float u = f * Vector3.Dot(s, h);
                if (u < 0 || u > 1)
                    continue;

                Vector3 q = Vector3.Cross(s, e1);
                float v = f * Vector3.Dot(ray.direction, q);
                if (v < 0 || (u + v) > 1)
                    continue;

                float t = f * Vector3.Dot(e2, q);
                if (t > minDist && t <= maxDist)
                {
                    hitInfo.distance = t;
                    hitInfo.material = container.material[i];
                    ray.GetPointOptimized(t, ref hitInfo.point);

                    // Barycentric normal
                    Vector3 v0 = p2 - p1, v1 = p3 - p1, v2 = hitInfo.point - p1;
                    float d00 = Vector3.Dot(v0, v0);
                    float d01 = Vector3.Dot(v0, v1);
                    float d11 = Vector3.Dot(v1, v1);
                    float d20 = Vector3.Dot(v2, v0);
                    float d21 = Vector3.Dot(v2, v1);
                    float denom = d00 * d11 - d01 * d01;
                    float baryV = (d11 * d20 - d01 * d21) / denom;
                    float baryW = (d00 * d21 - d01 * d20) / denom;
                    float baryU = 1.0f - baryV - baryW;

                    hitInfo.normal = container.n1[i] * baryU + container.n2[i] * baryV + container.n3[i] * baryW;
                    if (BitHelper.GetBit(ref state.hitMask, rayIndex))
                        HitInfo.ExchangeIfBetter(ref hits[rayIndex], hitInfo);
                    else
                    {
                        hits[rayIndex] = hitInfo;
                        BitHelper.SetBit(ref state.hitMask, rayIndex);
                    }
                }
            }
        }

        protected override TriangleSoA PrepareData(out BoundingBox[] boundingBoxes)
        {
            TriangleSoA container = new TriangleSoA();

            container.p1 = new Vector3[this.objectCount];
            container.p2 = new Vector3[this.objectCount];
            container.p3 = new Vector3[this.objectCount];
            container.n1 = new Vector3[this.objectCount];
            container.n2 = new Vector3[this.objectCount];
            container.n3 = new Vector3[this.objectCount];
            container.faceNormal = new Vector3[this.objectCount];
            container.material = new Material[this.objectCount];
            container.boundingBoxes = new BoundingBox[this.objectCount];

            for (int i = 0; i < this.objectCount; i++)
            {
                container.p1[i] = this.objects[i].p1;
                container.p2[i] = this.objects[i].p2;
                container.p3[i] = this.objects[i].p3;

                container.n1[i] = this.objects[i].n1;
                container.n2[i] = this.objects[i].n2;
                container.n3[i] = this.objects[i].n3;

                container.faceNormal[i] = this.objects[i].faceNormal;
                container.material[i] = this.objects[i].material;
                container.boundingBoxes[i] = this.objects[i].CalcBoundingBox();
            }

            boundingBoxes = container.boundingBoxes;
            return container;
        }
    }

    public struct Triangle : ISceneObject
    {
        public Vector3 p1;
        public Vector3 p2;
        public Vector3 p3;

        public Vector3 n1;
        public Vector3 n2;
        public Vector3 n3;

        public Vector3 faceNormal;

        public Material material;

        public Triangle(Vector3 p1, Vector3 p2, Vector3 p3, Material material)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;

            this.material = material;

            Vector3 side1 = p2 - p1;
            Vector3 side2 = p3 - p1;
            this.faceNormal = this.n1 = this.n2 = this.n3 = Vector3.Normalize(Vector3.Cross(side1, side2));
        }

        public Triangle(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 n1, Vector3 n2, Vector3 n3, Material material)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;

            this.n1 = n1;
            this.n2 = n2;
            this.n3 = n3;

            this.material = material;

            this.faceNormal = Vector3.Lerp(Vector3.Lerp(n1, n2, .5f), n3, .5f);
        }

        public BoundingBox CalcBoundingBox()
        {
            BoundingBox boundingBox = new BoundingBox();
            boundingBox.min = Vector3.Min(Vector3.Min(p1, p2), p3);
            boundingBox.max = Vector3.Max(Vector3.Max(p1, p2), p3);
            return boundingBox;
        }
    }
}
