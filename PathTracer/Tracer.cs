using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Threading;

namespace PathTracer
{
    public interface ITracingProcessor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns>Amount of rays cast while tracing</returns>
        void Trace(List<TileTraceParameters> traceTiles, ref TraceResult result, ProgressReportDelegate progressReport = null);
    }

    public static class TracingProcessors<T> where T : class, ITracingProcessor, new()
    {
        public static T instance
        {
            get
            {
                if (ReferenceEquals(_instance, null))
                    _instance = new T();
                return _instance;
            }
        }
        private static T _instance;

    }

    public class SinglethreadedCPUProcessor : ITracingProcessor
    {
        public void Trace(List<TileTraceParameters> traceTiles, ref TraceResult result, ProgressReportDelegate progressReport = null)
        {
            long rc = 0;
            for (int i = 0; i < traceTiles.Count; i++)
            {
                Tracer.TraceTile(traceTiles[i], result.backbuffer, out rc);
                progressReport?.Invoke(traceTiles.Count, i);
                result.rayCount += rc;
            }
        }
    }

    public class MultithreadedCPUProcessor : ITracingProcessor
    {
        public void Trace(List<TileTraceParameters> traceTiles, ref TraceResult result, ProgressReportDelegate progressReport = null)
        {
            AutoResetEvent evt = new AutoResetEvent(false);

            float[] backbuffer = result.backbuffer;
            long[] rayCounts = new long[traceTiles.Count];
            int tilesProcessed = 0;
            int c = traceTiles.Count;
            for (int i = 0; i < c; i++)
            {
                int j = i; // Move to local scope for lambda
                var tt = traceTiles[i];
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    Tracer.TraceTile(tt, backbuffer, out rayCounts[j]);
                    int processed = Interlocked.Increment(ref tilesProcessed);

                    progressReport?.Invoke(traceTiles.Count, processed);
                    if (processed == traceTiles.Count)
                        evt.Set();
                });
            }

            evt.WaitOne();
            result.rayCount = 0;
            for (int i = 0; i < rayCounts.Length; i++)
                result.rayCount += rayCounts[i];
        }
    }

    public struct TileTraceParameters
    {
        public TraceParams parameters;
        public TraceRuntimeParams runtimeParams;

        public int xStart;
        public int yStart;
        public int xEnd;
        public int yEnd;
    }

    public class TraceRuntimeParams
    {
        /// <summary>
        /// Standard and max.
        /// </summary>
        public const int StandardBatchSize = 32;

        public float invWidth;
        public float invHeight;
        public float invSamplesPerPixel;

        public int sampleBatchCount;
        public int batchSize = StandardBatchSize;
    }

    public delegate void ProgressReportDelegate(int tileCount, int tilesProcessed);

    public static class Tracer
    {
        public static TraceResult Render(TraceParams parameters, ProgressReportDelegate progressReport = null, float[] backbuffer = null)
        {
            // Allocate backbuffer
            if (ReferenceEquals(backbuffer, null) || backbuffer.Length != (parameters.width * parameters.height * 3))
                backbuffer = new float[parameters.width * parameters.height * 3];

            TraceResult result = new TraceResult()
            {
                parameters = parameters,
                backbuffer = backbuffer
            };

            TraceRuntimeParams runtimeParams = new TraceRuntimeParams();
            runtimeParams.invWidth = 1.0f / parameters.width;
            runtimeParams.invHeight = 1.0f / parameters.height;
            runtimeParams.invSamplesPerPixel = 1.0f / parameters.samplesPerPixel;
            runtimeParams.sampleBatchCount = (int)Mathf.Ceiling(parameters.samplesPerPixel / (float)runtimeParams.batchSize);

            List<TileTraceParameters> traceTiles = new List<TileTraceParameters>();
            for (int y = 0; y < parameters.height; y+= parameters.traceTileDimension)
                for (int x = 0; x < parameters.width; x+= parameters.traceTileDimension)
                {
                    var tt = new TileTraceParameters()
                    {
                        xStart = x,
                        yStart = y,
                        parameters = parameters,
                        runtimeParams = runtimeParams
                    };

                    tt.xEnd = Math.Min(tt.xStart + parameters.traceTileDimension, parameters.width);
                    tt.yEnd = Math.Min(tt.yStart + parameters.traceTileDimension, parameters.height);
                    traceTiles.Add(tt);
                }

            parameters.tracingProcessor.Trace(traceTiles, ref result, progressReport);
            return result;
        }
        
        [ThreadStatic]
        private static Ray[] _rays = null;
        [ThreadStatic]
        private static HitInfo[] _hits = null;
        [ThreadStatic]
        private static Vector3[] _localColor = null;

        private static void InitBatchArray<T>(ref T[] array)
        {
            if (ReferenceEquals(array, null))
                array = new T[TraceRuntimeParams.StandardBatchSize];
        }

        public static void TraceTile(TileTraceParameters tile, float[] backbuffer, out long rayCount)
        {
            InitBatchArray(ref _rays);
            InitBatchArray(ref _hits);
            InitBatchArray(ref _localColor);

            Ray[] rays = _rays;
            HitInfo[] hits = _hits;
            Vector3[] localColor = _localColor;
            uint rndState = FastRandom.Seed();
            rayCount = 0;

            for (int x = tile.xStart; x < tile.xEnd; x++)
                for (int y = tile.yStart; y < tile.yEnd; y++)
                {
                    Vector3 color = new Vector3(0, 0, 0);
                    // Sample bouncing batches
                    for (int b = 0; b < tile.runtimeParams.sampleBatchCount; b++)
                    {
                        // Calculate batch info
                        int batchStart = b * tile.runtimeParams.batchSize, batchEnd = batchStart + tile.runtimeParams.batchSize;
                        int batchRayCount = Math.Min(batchEnd, tile.parameters.samplesPerPixel);
                        batchRayCount -= batchStart;

                        uint hitMask = 0;
                        uint bounces = uint.MaxValue;
                        float u = x * tile.runtimeParams.invWidth, v = y * tile.runtimeParams.invHeight;

                        // Get initial rays
                        tile.parameters.camera.GetRays(u, v, rays, batchRayCount, ref rndState);

                        // Reset local colors
                        for (int i = 0; i < batchRayCount; i++)
                            localColor[i] = tile.parameters.ambientLight;

                        Vector3 atten;
                        int bounceCount = 0;
                        while (bounceCount < tile.parameters.maxBounces && bounces != 0)
                        {
                            tile.parameters.scene.Raycast(rays, hits, 0.01f, tile.parameters.maxDepth, batchRayCount, bounces, ref hitMask, ref rayCount);
                            if (hitMask == 0)
                                break;

                            bounces = 0;
                            for (int i = 0; i < batchRayCount; i++)
                            {
                                if (BitHelper.GetBit(ref hitMask, i)) // Did ray hit?
                                {
                                    if (hits[i].material.Scatter(ref hits[i], out atten, ref rays[i], tile.parameters.scene, ref rndState))
                                        BitHelper.SetBit(ref bounces, i);

                                    localColor[i] *= atten;
                                }
                            }
                            bounceCount++;
                        }

                        for (int i = 0; i < batchRayCount; i++)
                            color += localColor[i];
                    }

                    color *= tile.runtimeParams.invSamplesPerPixel;

                    // Write backbuffer
                    int idx = (y * (tile.parameters.width*3)) + (x * 3);
                    backbuffer[idx] = color.X;
                    backbuffer[idx+1] = color.Y;
                    backbuffer[idx+2] = color.Z;
                }
        }
    }
}
