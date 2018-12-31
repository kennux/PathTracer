using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracer
{
    public class Scene
    {
        private List<Sphere> spheres = new List<Sphere>();
        private Sphere[] _spheres;

        public int Add(Sphere sphere)
        {
            this.spheres.Add(sphere);
            return spheres.Count - 1;
        }

        public void PrepareForRendering()
        {
            this._spheres = spheres.ToArray();
        }

        public void Raycast(Ray[] rays, HitInfo[] hits, float minDist, float maxDist, int count, uint rayMask, ref uint hitMask, ref long rayCounter)
        {
            if (count > 32)
                throw new InvalidOperationException("Can only operate on batches of <= 32 rays!");

            int l = _spheres.Length;
            for (int i = 0; i < count; i++)
            {
                BitHelper.UnsetBit(ref hitMask, i);
                if (!BitHelper.GetBit(ref rayMask, i))
                    continue;

                rayCounter++;
                for (int j = 0; j < l; j++)
                {
                    if (_spheres[j].Raycast(ref rays[i], minDist, maxDist, ref hits[i]))
                        BitHelper.SetBit(ref hitMask, i);
                }
            }
        }
    }
}
