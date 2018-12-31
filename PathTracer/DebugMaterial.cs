using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace PathTracer
{
    public class DebugMaterial : Material
    {
        public override bool Scatter(ref HitInfo hitInfo, out Vector3 attenuation, ref Ray scatteredRay, Scene scene, ref uint rndState)
        {
            attenuation = 0.5f * (hitInfo.normal + new Vector3(1, 1, 1));
            return false;
        }
    }
}
