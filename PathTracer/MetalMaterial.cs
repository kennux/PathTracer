using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace PathTracer
{
    public sealed class MetalMaterial : Material
    {
        public Vector3 albedo;
        public float roughness;

        public MetalMaterial(Vector3 albedo, float roughness)
        {
            this.albedo = albedo;
            this.roughness = roughness;
        }

        public override bool Scatter(ref HitInfo hitInfo, out Vector3 attenuation, ref Ray ray, Scene scene, ref uint rndState)
        {
            // Reflect the ray
            Vector3 reflected = Vector3.Reflect(ray.direction, hitInfo.normal);
            ray.origin = hitInfo.point;
            ray.direction = Vector3.Normalize(reflected + this.roughness * FastRandom.RandomInUnitSphere(ref rndState));

            // Attenuation is albeddo
            attenuation = albedo;

            return Vector3.Dot(ray.direction, hitInfo.normal) > 0;
        }
    }
}
