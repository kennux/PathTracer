using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Numerics;
using System.Drawing;
using System.Diagnostics;
using System.IO;

namespace PathTracer.Test
{
    class Program
    {
        private static Scene SimpleSphereScene(out Stopwatch stopwatch)
        {
            stopwatch = Stopwatch.StartNew();

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
            SceneSphereHitSystem sphereHitSystem = scene.GetOrCreate<SceneSphereHitSystem>();
            for (int i = 0; i < spheres.Length; i++)
                sphereHitSystem.Add(spheres[i]);

            scene.PrepareForRendering();
            stopwatch.Stop();

            return scene;
        }

        private static Scene TeapotScene(out Stopwatch stopwatch)
        {
            stopwatch = Stopwatch.StartNew();

            Scene scene = new Scene();
            TriangleHitSystem triangleHitSystem = scene.GetOrCreate<TriangleHitSystem>();
            SceneSphereHitSystem sphereHitSystem = scene.GetOrCreate<SceneSphereHitSystem>();

            sphereHitSystem.Add(new Sphere(new Vector3(0, -100.5f, -1), 100, new LambertianMaterial(new Vector3(.8f, .8f, .8f))));

            List<Triangle> triangles = new List<Triangle>();
            Matrix4x4 TRS = Matrix4x4.CreateScale(0.01f);
            TRS.Translation = new Vector3(0, 1f, 1f);
            WavefrontLoader.Load(File.ReadAllText(@"Resources/Teapot/teapot.obj"), TRS, ref triangles);

            var mat = new MetalMaterial(new Vector3(.1f, .1f, .1f), 1f);
            for (int i = 0; i < triangles.Count; i++)
            {
                var tri = triangles[i];
                tri.material = mat;

                triangleHitSystem.Add(tri);
            }

            scene.PrepareForRendering();
            stopwatch.Stop();
            Console.WriteLine("Teapot loaded!");

            return scene;
        }

        private static TraceResult TraceTest(int width, int height, Scene scene, out Stopwatch stopwatch)
        {
            stopwatch = Stopwatch.StartNew();
            Camera camera = new Camera(new Vector3(0, 2, 3), new Vector3(0, 0, 0), new Vector3(0, 1, 0), 70, (float)width / (float)height, 0.025f, 3f);
            TraceParams parameters = new TraceParams()
            {
                ambientLight = new Vector3(.75f, .75f, .75f),
                camera = camera,
                height = height,
                width = width,
                samplesPerPixel = 128,
                scene = scene,
                maxBounces = 8,
                maxDepth = float.PositiveInfinity,
                traceTileDimension = 32,
                multithreading = true
            };
            var result = Tracer.Render(parameters, (count, processed) =>
            {
                if (processed % 25 == 0)
                    Console.WriteLine(string.Format("Completed {0} tiles of {1} ({2} %)", processed.ToString(), count.ToString(), ((processed / (float)count) * 100f).ToString("0.00")));
            });

            stopwatch.Stop();
            return result;
        }

        static void Main(string[] args)
        {
            int width = 320;
            int height = 240;
            Stopwatch swInit, swRender;

            // var scene = SimpleSphereScene(out swInit);
            var scene = TeapotScene(out swInit);
            var result = TraceTest(width, height, scene, out swRender);

            double mRays = (result.rayCount / 1000000d);
            Console.WriteLine("RayCount: " + mRays.ToString("0.00") + " MRays | " + swRender.Elapsed.TotalMilliseconds.ToString("0.000") + " ms, Init: " + swInit.Elapsed.TotalMilliseconds.ToString("0.000") + " ms | " + (mRays / swRender.Elapsed.TotalSeconds).ToString("0.00") + " MRays/s");

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
