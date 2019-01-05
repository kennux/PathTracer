using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PathTracer
{
    public struct TraceParams
    {
        public int width;
        public int height;

        public Scene scene;
        public Camera camera;

        public int samplesPerPixel;
        public Vector3 ambientLight;
        public int traceTileDimension;
        public float maxDepth;
        public int maxBounces;

        public ITracingProcessor tracingProcessor;
    }

    public struct TraceResult
    {
        public TraceParams parameters;

        public float[] backbuffer;
        public long rayCount;
    }
}
