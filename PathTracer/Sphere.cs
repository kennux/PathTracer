using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace PathTracer
{
    public struct SphereSoA : ISoAContainer<SphereSoA>
    {
        public Material[] material;
        public Vector3[] center;
        public float[] radius;
        public float[] radiusSq;
        public float[] invRadius;
        public BoundingBox[] boundingBoxes;
        public int objectCount;

        public void CreateFromSubsetOfOther(SphereSoA other, int[] indices)
        {
            this.center = new Vector3[indices.Length];
            this.material = new Material[indices.Length];
            this.radius = new float[indices.Length];
            this.radiusSq = new float[indices.Length];
            this.invRadius = new float[indices.Length];
            this.boundingBoxes = new BoundingBox[indices.Length];
            this.objectCount = indices.Length;

            for (int i = 0; i < this.objectCount; i++)
            {
                this.center[i] = other.center[indices[i]];
                this.material[i] = other.material[indices[i]];
                this.radius[i] = other.radius[indices[i]];

                this.radiusSq[i] = other.radiusSq[indices[i]];
                this.invRadius[i] = other.invRadius[indices[i]];
                this.boundingBoxes[i] = other.boundingBoxes[indices[i]];
            }
        }
    }

    public class SceneSphereHitSystem : OCTreeSceneHitSystem<Sphere, SphereSoA>
    {
        protected override SphereSoA PrepareData(out BoundingBox[] boundingBoxes)
        {
            SphereSoA container = new SphereSoA();
            container.boundingBoxes = new BoundingBox[this.objectCount];
            container.center = new Vector3[this.objectCount];
            container.invRadius = new float[this.objectCount];
            container.radius = new float[this.objectCount];
            container.radiusSq = new float[this.objectCount];
            container.material = new Material[this.objectCount];

            for (int i = 0; i < this.objects.Count; i++)
            {
                container.center[i] = this.objects[i].center;
                container.invRadius[i] = 1f / this.objects[i].radius;
                container.radius[i] = this.objects[i].radius;
                container.radiusSq[i] = this.objects[i].radius*this.objects[i].radius;
                container.material[i] = this.objects[i].material;
                container.center[i] = this.objects[i].center;
                container.boundingBoxes[i] = BoundingBox.FromSphere(container.center[i], container.radius[i]);
            }

            container.objectCount = this.objectCount;
            boundingBoxes = container.boundingBoxes;
            return container;
        }

        protected override void Raycast(ref State state, ref SphereSoA container)
        {
            Ray ray = state.ray;
            float minDist = state.minDist, maxDist = state.maxDist;
            int rayIndex = state.rayIndex;
            HitInfo[] hits = state.hits;
            HitInfo hitInfo = new HitInfo();

            for (int j = 0; j < container.objectCount; j++)
            {
                // Inlined and optimized math
                Vector3 center = container.center[j];
                Vector3 oc = ray.origin - center;
                float b = Vector3.Dot(oc, ray.direction);
                if (b > 0f) // Behind?
                    continue;

                bool hasHit = false;
                float c = oc.LengthSquared();
                float discriminantSqr = b * b - (c - container.radiusSq[j]);
                if (discriminantSqr > 0)
                {
                    float discriminant = Mathf.Sqrt(discriminantSqr);
                    hitInfo.distance = (-b - discriminant);
                    if (hitInfo.distance < maxDist && hitInfo.distance > minDist)
                    {
                        ray.GetPointOptimized(hitInfo.distance, ref hitInfo.point);
                        hitInfo.normal = (hitInfo.point - center) * container.invRadius[j];
                        hitInfo.material = container.material[j];
                        hasHit = true;
                    }
                    else
                    {
                        hitInfo.distance = (-b + discriminant);
                        if (hitInfo.distance < maxDist && hitInfo.distance > minDist)
                        {
                            ray.GetPointOptimized(hitInfo.distance, ref hitInfo.point);
                            hitInfo.normal = (hitInfo.point - center) * container.invRadius[j];
                            hitInfo.material = container.material[j];
                            hasHit = true;
                        }
                    }
                }

                if (hasHit)
                {
                    if (BitHelper.GetBit(ref state.hitMask, rayIndex))
                        HitInfo.ExchangeIfBetter(ref state.hits[rayIndex], hitInfo);
                    else
                    {
                        hits[rayIndex] = hitInfo;
                        BitHelper.SetBit(ref state.hitMask, rayIndex);
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
