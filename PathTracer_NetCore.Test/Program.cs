using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Numerics;
using System.Drawing;
using System.IO;

namespace PathTracer.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Sphere[] spheres = new Sphere[]
            {
                new Sphere(new Vector3(0,-100.5f,-1), 100, new LambertianMaterial(new Vector3(.8f, .8f, .8f))),
                new Sphere(new Vector3(2,0,-1), 0.5f, new LambertianMaterial(new Vector3(0.8f, 0.4f, 0.4f))),
                new Sphere(new Vector3(0,0,-1), 0.5f, new LambertianMaterial(new Vector3(0.4f, 0.8f, 0.4f))),
                new Sphere(new Vector3(-2,0,-1), 0.5f, new MetalMaterial(new Vector3(.4f, .4f, .8f), 0)),
                new Sphere(new Vector3(2,0,1), 0.5f, new MetalMaterial(new Vector3(.4f, .8f, .4f), 0)),
                new Sphere(new Vector3(0,0,1), 0.5f, new MetalMaterial(new Vector3(.4f, .8f, .4f), 0.2f)),
                new Sphere(new Vector3(-2,0,1), 0.5f, new MetalMaterial(new Vector3(.4f, .8f, .4f), 0.6f)),
                new Sphere(new Vector3(0.5f,1.25f,0.5f), 0.5f, new DielectricMaterial(1.5f)),
                new Sphere(new Vector3(-1.5f,1.5f,0f), 0.3f, new LambertianMaterial(new Vector3(.8f, .6f, .2f)))
            };

            Scene scene = new Scene();
            SceneSphereHitSystem sphereHitSystem = new SceneSphereHitSystem();
            for (int i = 0; i < spheres.Length; i++)
                sphereHitSystem.Add(spheres[i]);

            scene.Add(sphereHitSystem);
            scene.PrepareForRendering();
            int width = 640;
            int height = 480;

            Camera camera = new Camera(new Vector3(0, 2, 3), new Vector3(0, 0, 0), new Vector3(0, 1, 0), 70, (float)width / (float)height, 0.025f, 3f);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            TraceParams parameters = new TraceParams()
            {
                ambientLight = new Vector3(.75f, .75f, .75f),
                camera = camera,
                height = height,
                width = width,
                samplesPerPixel = 512,
                scene = scene,
                maxBounces = 12,
                maxDepth = float.PositiveInfinity,
                traceTileDimension = 16,
                multithreading = true
            };
            var result = Tracer.Render(parameters, (count, processed) =>
            {
                if (processed % 25 == 0)
                    Console.WriteLine(string.Format("Completed {0} tiles of {1} ({2} %)", processed.ToString(), count.ToString(), ((processed / (float)count) * 100f).ToString("0.00")));
            });
            stopwatch.Stop();

            double mRays = (result.rayCount / 1000000d);
            Console.WriteLine("RayCount: " + mRays.ToString("0.00") + " MRays | " + stopwatch.Elapsed.TotalMilliseconds.ToString("0.000") + " ms | " + (mRays / stopwatch.Elapsed.TotalSeconds).ToString("0.00") + " MRays/s");

            Bitmap bmp = new Bitmap(width, height);
            int idx = 0;
            for (int y = 0; y < result.parameters.height; y++)
                for (int x = 0; x < result.parameters.width; x++)
                {
                    // Sqrt as approximation to move linear -> gamma space
                    byte r = (byte)(255.99f * Math.Sqrt(result.backbuffer[idx]));
                    byte g = (byte)(255.99f * Math.Sqrt(result.backbuffer[idx + 1]));
                    byte b = (byte)(255.99f * Math.Sqrt(result.backbuffer[idx + 2]));

                    bmp.SetPixel(x, height - y - 1, Color.FromArgb(r, g, b));

                    idx += 3;
                }

            bmp.Save(@"D:\test.png");
            Console.ReadLine();
        }
    }
}
