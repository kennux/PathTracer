using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace PathTracer
{
    /// <summary>
    /// Lambertian diffuse material implementation.
    /// </summary>
    public sealed class LambertianMaterial : Material
    {
        public Vector3 albedo;

        public LambertianMaterial(Vector3 albedo)
        {
            this.albedo = albedo;
        }

        public override bool Scatter(ref HitInfo hitInfo, out Vector3 attenuation, ref Ray ray, Scene scene, ref uint rndState)
        {
            // Scatter ray in random direction
            Vector3 target = hitInfo.point + hitInfo.normal + FastRandom.RandomUnitVector(ref rndState);
            ray.origin = hitInfo.point;
            ray.direction = Vector3.Normalize(target - hitInfo.point);

            // Attenuation is albedo
            attenuation = albedo;
            return true;
        }
    }
}
