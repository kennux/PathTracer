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

    public abstract class OCTreeSceneHitSystem<TObject, TContainer> : SceneHitSystem<TObject> where TObject : ISceneObject where TContainer : ISoAContainer<TContainer>, new()
    {
        protected TContainer container;
        protected OCTree<TContainer> ocTree = new OCTree<TContainer>();
        protected OCTree<TContainer>.RaycastCallback<State> raycastDelegate;
        protected int objectCount;

        protected BoundingBox[] boundingBoxes;

        public struct State
        {
            public Ray ray;
            public uint hitMask;
            public HitInfo[] hits;
            public float minDist;
            public float maxDist;
            public int rayIndex;
        }

        public override void PrepareForRendering()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            this.objectCount = this.objects.Count;

            this.container = PrepareData(out this.boundingBoxes);
            this.raycastDelegate = Raycast;
            this.ocTree.Bake(this.boundingBoxes, this.container);
            sw.Stop();
            Console.WriteLine(this.GetType().FullName + " prepared in " + sw.Elapsed.TotalMilliseconds + " ms");
        }

        public override void Raycast(Ray[] rays, HitInfo[] hits, float minDist, float maxDist, int count, uint rayMask, ref uint hitMask)
        {
            Ray nextRay = rays[0];
            State state = new State();
            state.hitMask = hitMask;
            state.hits = hits;
            state.minDist = minDist;
            state.maxDist = maxDist;
            
            for (int i = 0; i < count; i++)
            {
                state.ray = nextRay;

                int nIndex = count + 1;
                if (nIndex < count - 1)
                    nextRay = rays[i + 1];

                if (!BitHelper.GetBit(ref rayMask, i))
                    continue;

                state.rayIndex = i;
                this.ocTree.Raycast(state.ray, this.raycastDelegate, ref state);
            }

            hitMask = state.hitMask;
        }
        
        protected abstract TContainer PrepareData(out BoundingBox[] boundingBoxes);
        protected abstract void Raycast(ref State state, ref TContainer container);
    }

    public interface ISceneLight
    {

    }

    public interface ISceneLightSystem
    {
        void PrepareForRendering(Scene scene);
    }

    public abstract class SceneLightSystem<TLight> : ISceneLightSystem where TLight : ISceneLight
    {
        protected List<TLight> lights = new List<TLight>();
        protected Scene scene;

        public int Add(TLight obj)
        {
            this.lights.Add(obj);
            return lights.Count - 1;
        }

        public virtual void PrepareForRendering(Scene scene) { this.scene = scene; }
    }

    public class Scene
    {
        private List<ISceneHitSystem> hitSystems = new List<ISceneHitSystem>();
        private ISceneHitSystem[] _hitSystems;
        private List<ISceneLightSystem> lightSystems = new List<ISceneLightSystem>();
        private ISceneLightSystem[] _lightSystems;

        public TSystem GetOrCreateHitSystem<TSystem>() where TSystem : class, ISceneHitSystem, new()
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

        public TSystem GetOrCreateLightSystem<TSystem>() where TSystem : class, ISceneLightSystem, new()
        {
            TSystem sys = null;
            foreach (var lightSystem in lightSystems)
            {
                sys = lightSystem as TSystem;
                if (sys != null)
                    break;
            }

            if (sys == null)
            {
                sys = new TSystem();
                this.lightSystems.Add(sys);
            }

            return sys;
        }

        public void PrepareForRendering()
        {
            _hitSystems = hitSystems.ToArray();
            _lightSystems = lightSystems.ToArray();

            for (int i = 0; i < _hitSystems.Length; i++)
                _hitSystems[i].PrepareForRendering();
            for (int i = 0; i < _lightSystems.Length; i++)
                _lightSystems[i].PrepareForRendering(this);
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
