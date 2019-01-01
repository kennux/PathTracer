using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace PathTracer
{
    public class TriangleHitSystem : SceneHitSystem<Triangle>
    {
        private Vector3[] p1;
        private Vector3[] p2;
        private Vector3[] p3;
        private Vector3[] faceNormal;
        private Material[] material;
        private int triangleCount;

        public override void PrepareForRendering()
        {
            this.triangleCount = this.objects.Count;
            this.p1 = new Vector3[this.triangleCount];
            this.p2 = new Vector3[this.triangleCount];
            this.p3 = new Vector3[this.triangleCount];
            this.faceNormal = new Vector3[this.triangleCount];
            this.material = new Material[this.triangleCount];

            for (int i = 0; i < this.triangleCount; i++)
            {
                this.p1[i] = this.objects[i].p1;
                this.p2[i] = this.objects[i].p2;
                this.p3[i] = this.objects[i].p3;
                this.faceNormal[i] = this.objects[i].faceNormal;
                this.material[i] = this.objects[i].material;
            }
        }

        public override void Raycast(Ray[] rays, HitInfo[] hits, float minDist, float maxDist, int count, uint rayMask, ref uint hitMask)
        {
            Vector3 e1, e2;
            Vector3 p, q, t;
            float det, invDet, u, v;
            HitInfo hitInfo = new HitInfo();
            Ray ray, nextRay = rays[0];

            for (int i = 0; i < count; i++)
            {
                ray = nextRay;

                int nIndex = count + 1;
                if (nIndex < count - 1)
                    nextRay = rays[i + 1];

                if (!BitHelper.GetBit(ref rayMask, i))
                    continue;

                for (int j = 0; j < this.triangleCount; j++)
                {
                    Vector3 p1 = this.p1[j];
                    Vector3 p2 = this.p2[j];
                    Vector3 p3 = this.p3[j];
                    
                    e1 = p2 - p1;
                    e2 = p3 - p1;
                    p = Vector3.Cross(ray.direction, e2);
                    det = Vector3.Dot(e1, p);
                    
                    if (det > -float.Epsilon && det < float.Epsilon) { continue; }

                    invDet = 1.0f / det;
                    t = ray.origin - p1;
                    u = Vector3.Dot(t, p) * invDet;

                    //Check for ray hit
                    if (u < 0 || u > 1) { continue; }
                    
                    q = Vector3.Cross(t, e1);
                    v = Vector3.Dot(ray.direction, q) * invDet;

                    //Check for ray hit
                    if (v < 0 || u + v > 1) { continue; }

                    float distance = (Vector3.Dot(e2, q) * invDet);
                    if (distance > float.Epsilon)
                    {
                        hitInfo.distance = distance;
                        hitInfo.material = this.material[j];
                        ray.GetPointOptimized(distance, ref hitInfo.point);
                        hitInfo.normal = this.faceNormal[j];

                        if (BitHelper.GetBit(ref hitMask, i))
                            HitInfo.ExchangeIfBetter(ref hits[i], hitInfo);
                        else
                        {
                            hits[i] = hitInfo;
                            BitHelper.SetBit(ref hitMask, i);
                        }
                    }
                }
            }
        }
    }

    public struct Triangle : ISceneObject
    {
        public Vector3 p1;
        public Vector3 p2;
        public Vector3 p3;

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
            this.faceNormal = Vector3.Normalize(Vector3.Cross(side1, side2));
        }
    }
}
