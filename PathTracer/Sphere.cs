using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace PathTracer
{
    /// <summary>
    /// Sphere object implementation.
    /// </summary>
    public struct Sphere
    {
        public Vector3 center
        {
            get { return this._center; }
            set { this._center = value; Precompute(); }
        }

        public float radius
        {
            get { return this._radius; }
            set { this._radius = value; Precompute(); }
        }

        private Vector3 _center;
        private float _radius;
        private float _radiusSq;
        private float _invRadius;

        public Material material;

        /// <summary>
        /// Called when <see cref="_center"/> or <see cref="_radius"/> is changed to precompute collision data.
        /// </summary>
        private void Precompute()
        {
            this._radiusSq = this._radius * this._radius;
            this._invRadius = 1f / this._radius;
        }

        public Sphere(Vector3 center, float radius, Material material)
        {
            this._center = center;
            this._radius = radius;

            // Precompute() inlined
            this._radiusSq = this._radius * this._radius;
            this._invRadius = 1f / this._radius;
            
            this.material = material;
        }
        
        public bool Raycast(ref Ray ray, float minDist, float maxDist, ref HitInfo hitInfo)
        {
            // Inlined and optimized math
            Vector3 oc = ray.origin - _center;
            float b = Vector3.Dot(oc, ray.direction);
            if (b > 0f) // Behind?
                return false;

            float c = oc.LengthSquared();
            float discriminantSqr = b * b - (c - _radiusSq);
            if (discriminantSqr > 0)
            {
                float discriminant = Mathf.Sqrt(discriminantSqr);
                float temp = (-b - discriminant);
                if (temp < maxDist && temp > minDist)
                {
                    ray.GetPointOptimized(temp, ref hitInfo.point);
                    hitInfo.normal = (hitInfo.point - this._center) * this._invRadius;
                    hitInfo.material = this.material;
                    return true;
                }

                temp = (-b + discriminant);
                if (temp < maxDist && temp > minDist)
                {
                    ray.GetPointOptimized(temp, ref hitInfo.point);
                    hitInfo.normal = (hitInfo.point - this._center) * this._invRadius;
                    hitInfo.material = this.material;
                    return true;
                }
            }
            return false;
        }
    }
}
