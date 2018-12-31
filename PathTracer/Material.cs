using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace PathTracer
{
    /// <summary>
    /// Interface for implementing materials.
    /// </summary>
    public abstract class Material
    {
        /// <summary>
        /// Scatters the specified ray.
        /// </summary>
        /// <param name="hitInfo">The hit information.</param>
        /// <param name="attenuation">The attenuation for hitInfo evaluated from this material.</param>
        /// <param name="ray">The ray. If it gets scattered, write the scattered ray into this reference.</param>
        /// <returns>Whether or not the ray was scattered. If false is returned, path tracing ends at this hit for the current ray.</returns>
        public abstract bool Scatter(ref HitInfo hitInfo, out Vector3 attenuation, ref Ray ray, Scene scene, ref uint rndState);
    }
}
