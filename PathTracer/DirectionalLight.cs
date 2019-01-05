using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace PathTracer
{
    public class DirectionalLightSystem : SceneLightSystem<DirectionalLight>
    {
        private Vector3[] direction;

        private int lightCount;

        public override void PrepareForRendering(Scene scene)
        {
            this.lightCount = this.lights.Count;
            direction = new Vector3[this.lightCount];

            for (int i = 0; i < lightCount; i++)
            {
                direction[i] = this.lights[i].direction;
            }
        }
    }

    public class DirectionalLight : ISceneLight
    {
        public Vector3 direction;
    }
}
