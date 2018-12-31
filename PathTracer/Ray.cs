using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace PathTracer
{
    /// <summary>
    /// Ray structure that can be used to describe a ray in the world.
    /// </summary>
    public struct Ray
    {
        public Vector3 origin;
        public Vector3 direction;

        /// <summary>
        /// Optimized version of <see cref="GetPoint(float)"/>
        /// </summary>
        public void GetPointOptimized(float t, ref Vector3 point)
        {
            point.X = this.origin.X + (t * this.direction.X);
            point.Y = this.origin.Y + (t * this.direction.Y);
            point.Z = this.origin.Z + (t * this.direction.Z);
        }

        /// <summary>
        /// Returns the point on this ray with distance t to its origin.
        /// </summary>
        public Vector3 GetPoint(float t)
        {
            return this.origin + (t * this.direction);
        }

        public Ray(Vector3 origin, Vector3 direction)
        {
            this.origin = origin;
            this.direction = Vector3.Normalize(direction);
        }
    }
}
