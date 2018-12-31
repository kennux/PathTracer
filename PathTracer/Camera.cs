using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace PathTracer
{
    /// <summary>
    /// A camera implementation taken from Aras-P (https://github.com/aras-p/ToyPathTracer)
    /// </summary>
    public class Camera
    {
        public Vector3 origin;
        public Vector3 lowerLeftCorner;
        public Vector3 horizontal;
        public Vector3 vertical;
        public Vector3 u, v, w;
        float lensRadius;

        // vfov is top to bottom in degrees
        public Camera(Vector3 lookFrom, Vector3 lookAt, Vector3 vup, float vfov, float aspect, float aperture, float focusDist)
        {
            lensRadius = aperture / 2;
            float theta = vfov * MathUtil.PI / 180;
            float halfHeight = Mathf.Tan(theta / 2f);
            float halfWidth = aspect * halfHeight;
            origin = lookFrom;
            w = Vector3.Normalize(lookFrom - lookAt);
            u = Vector3.Normalize(Vector3.Cross(vup, w));
            v = Vector3.Cross(w, u);
            lowerLeftCorner = origin - halfWidth * focusDist * u - halfHeight * focusDist * v - focusDist * w;
            horizontal = 2 * halfWidth * focusDist * u;
            vertical = 2 * halfHeight * focusDist * v;
        }

        public void GetRays(float u, float v, Ray[] rays, int count, ref uint rndState)
        {
            // parameters.camera.GetRay(u + (invWidth * FastRandom.RandomFloat01(ref rndState)), v + (invHeight * FastRandom.RandomFloat01(ref rndState)), ref rays[i], ref rndState);
            Ray ray = new Ray();
            Vector3 uv = (u * horizontal) + (v * vertical);

            for (int i = 0; i < count; i++)
            {
                Vector3 vec = FastRandom.RandomInUnitDisk(ref rndState);
                Vector3 offset = this.u * (vec.X * lensRadius) + this.v * (vec.Y * lensRadius);
                ray.origin = origin + offset;
                ray.direction = Vector3.Normalize(lowerLeftCorner + uv - origin - offset); 
                rays[i] = ray;
            }
        }

        /// <summary>
        /// Returns a ray for the specified coordinates.
        /// </summary>
        /// <param name="u">Horizontal (U-) coordinate on the screen. 0-1</param>
        /// <param name="v">Vertical (V-) coordinate on the screen. 0-1</param>
        /// <param name="ray">Resulting ray with a random offset.</param>
        public void GetRay(float u, float v, ref Ray ray, ref uint rndState)
        {
            Vector3 vec = FastRandom.RandomInUnitDisk(ref rndState);
            Vector3 offset = this.u * (vec.X * lensRadius) + this.v * (vec.Y * lensRadius);
            ray.origin = origin + offset;
            // ray.direction = lowerLeftCorner + (MathUtil.Vec3Zero * u) * horizontal + (MathUtil.Vec3Zero * v) * vertical - origin - offset; 

            
            ray.direction.X = (lowerLeftCorner.X + u * horizontal.X + v * vertical.X - origin.X - offset.X);
            ray.direction.Y = (lowerLeftCorner.Y + u * horizontal.Y + v * vertical.Y - origin.Y - offset.Y);
            ray.direction.Z = (lowerLeftCorner.Z + u * horizontal.Z + v * vertical.Z - origin.Z - offset.Z);
            ray.direction = Vector3.Normalize(ray.direction);
        }
    }
}
