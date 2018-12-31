using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace PathTracer
{
    /// <summary>
    /// Dielectric glass material.
    /// </summary>
    public sealed class DielectricMaterial : Material
    {
        private float ri;
        private float _invRi;

        public DielectricMaterial(float ri)
        {
            this.ri = ri;
            this._invRi = 1.0f / ri;
        }

        public override bool Scatter(ref HitInfo hitInfo, out Vector3 attenuation, ref Ray ray, Scene scene, ref uint rndState)
        {
            // Prepare stack variables
            Vector3 outwardNormal, rDir = ray.direction, refracted;
            float nint, reflectionProbe = 1, cosine;
            attenuation = new Vector3(1, 1, 1);
            float rDN = Vector3.Dot(rDir, hitInfo.normal);

            // Determine whether we are looking from the inside or outside at the sphere
            if (rDN > 0)
            {
                // We are looking from the inside, invert the normal
                outwardNormal = hitInfo.normal * -1f;
                nint = this.ri;
                cosine = this.ri * rDN;
            }
            else
            {
                // We are looking from outside
                outwardNormal = hitInfo.normal;
                nint = this._invRi;
                cosine = -rDN;
            }

            // Check if refracting the ray is possible
            if (MathUtil.Refract(rDir, outwardNormal, nint, out refracted))
                reflectionProbe = MathUtil.Schlick(cosine, this.ri);

            // Reflection or refraction ray trace randomization
            if (FastRandom.RandomFloat01(ref rndState) < reflectionProbe)
                ray.direction = Vector3.Reflect(rDir, hitInfo.normal);
            else
                ray.direction = refracted;
            ray.origin = hitInfo.point;

            return true;
        }
    }
}
