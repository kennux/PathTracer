using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracer
{
    public interface ISceneObject
    {

    }

    public interface ISceneHitSystem
    {
        void PrepareForRendering();
        void Raycast(Ray[] rays, HitInfo[] hits, float minDist, float maxDist, int count, uint rayMask, ref uint hitMask);
    }

    public abstract class SceneHitSystem<TObject> : ISceneHitSystem where TObject : ISceneObject
    {
        protected List<TObject> objects = new List<TObject>();

        public int Add(TObject obj)
        {
            this.objects.Add(obj);
            return objects.Count - 1;
        }

        public virtual void PrepareForRendering() { }
        public abstract void Raycast(Ray[] rays, HitInfo[] hits, float minDist, float maxDist, int count, uint rayMask, ref uint hitMask);
    }

    public class Scene
    {
        private List<ISceneHitSystem> hitSystems = new List<ISceneHitSystem>();
        private ISceneHitSystem[] _hitSystems;

        public TSystem GetOrCreate<TSystem>() where TSystem : class, ISceneHitSystem, new()
        {
            TSystem sys = null;
            foreach (var hitSystem in hitSystems)
            {
                sys = hitSystem as TSystem;
                if (sys != null)
                    break;
            }

            if (sys == null)
            {
                sys = new TSystem();
                this.hitSystems.Add(sys);
            }

            return sys;
        }

        public void PrepareForRendering()
        {
            _hitSystems = hitSystems.ToArray();
            for (int i = 0; i < _hitSystems.Length; i++)
                _hitSystems[i].PrepareForRendering();
        }

        public void Raycast(Ray[] rays, HitInfo[] hits, float minDist, float maxDist, int count, uint rayMask, ref uint hitMask, ref long rayCounter)
        {
            if (count == 0)
                return;

            if (count > 32)
                throw new InvalidOperationException("Can only operate on batches of <= 32 rays!");

            hitMask = 0;
            rayCounter += BitHelper.NumberOfSetBits(rayMask);
            int sc = _hitSystems.Length;
            for (int i = 0; i < sc; i++)
                _hitSystems[i].Raycast(rays, hits, minDist, maxDist, count, rayMask, ref hitMask);
        }
    }
}
