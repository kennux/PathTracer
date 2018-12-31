using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace PathTracer
{
    /// <summary>
    /// Structure that can be used to transfer information about a ray hit.
    /// </summary>
    public struct HitInfo
    {
        public Vector3 point;
        public Vector3 normal;

        public Material material;
    }
}
