using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace PathTracer
{
    public class SceneSphereHitSystem : SceneHitSystem<Sphere>
    {
        private Material[] material;
        private Vector3[] center;
        private float[] radius;
        private float[] radiusSq;
        private float[] invRadius;
        private int sphereCount;

        public override void PrepareForRendering()
        {
            this.sphereCount = this.objects.Count;
            this.material = new Material[this.sphereCount];
            this.center = new Vector3[this.sphereCount];
            this.radius = new float[this.sphereCount];
            this.radiusSq = new float[this.sphereCount];
            this.invRadius = new float[this.sphereCount];

            for (int i = 0; i < this.sphereCount; i++)
            {
                this.material[i] = this.objects[i].material;
                this.center[i] = this.objects[i].center;
                this.radius[i] = this.objects[i].radius;
                this.radiusSq[i] = this.radius[i] * this.radius[i];
                this.invRadius[i] = 1f / this.radius[i];
            }
        }

        public override void Raycast(Ray[] rays, HitInfo[] hits, float minDist, float maxDist, int count, uint rayMask, ref uint hitMask, ref long rayCounter)
        {
            hitMask = 0;
            for (int i = 0; i < count; i++)
            {
                if (!BitHelper.GetBit(ref rayMask, i))
                    continue;

                rayCounter++;
                Ray ray = rays[i];
                HitInfo hitInfo = new HitInfo();
                for (int j = 0; j < this.sphereCount; j++)
                {
                    // Inlined and optimized math
                    Vector3 center = this.center[j];
                    Vector3 oc = ray.origin - center;
                    float b = Vector3.Dot(oc, ray.direction);
                    if (b > 0f) // Behind?
                        continue;

                    bool hasHit = false;
                    float c = oc.LengthSquared();
                    float discriminantSqr = b * b - (c - radiusSq[j]);
                    if (discriminantSqr > 0)
                    {
                        float discriminant = Mathf.Sqrt(discriminantSqr);
                        hitInfo.distance = (-b - discriminant);
                        if (hitInfo.distance < maxDist && hitInfo.distance > minDist)
                        {
                            ray.GetPointOptimized(hitInfo.distance, ref hitInfo.point);
                            hitInfo.normal = (hitInfo.point - center) * this.invRadius[j];
                            hitInfo.material = this.material[j];
                            hasHit = true;
                        }
                        else
                        {
                            hitInfo.distance = (-b + discriminant);
                            if (hitInfo.distance < maxDist && hitInfo.distance > minDist)
                            {
                                ray.GetPointOptimized(hitInfo.distance, ref hitInfo.point);
                                hitInfo.normal = (hitInfo.point - center) * this.invRadius[j];
                                hitInfo.material = this.material[j];
                                hasHit = true;
                            }
                        }
                    }

                    if (hasHit)
                    {
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
    
    public class Sphere : ISceneObject
    {
        public Vector3 center;
        public float radius;

        public Material material;

        public Sphere(Vector3 center, float radius, Material material)
        {
            this.center = center;
            this.radius = radius;
            
            this.material = material;
        }
    }
}
