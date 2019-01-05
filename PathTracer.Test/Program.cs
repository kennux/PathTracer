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
        private static Scene SimpleSphereScene(int width, int height, out Stopwatch stopwatch, out Camera camera)
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
            camera = new Camera(new Vector3(0, 2, 3), new Vector3(0, 0, 0), new Vector3(0, 1, 0), 70, (float)width / (float)height, 0.025f, 3f);

            return scene;
        }

        private static Scene TeapotScene(int width, int height, out Stopwatch stopwatch, out Camera camera)
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
                //new Sphere(new Vector3(-1.5f,1.5f,0f), 0.3f, new LambertianMaterial(new Vector3(.8f, .6f, .2f)))
            };

            Scene scene = new Scene();
            TriangleHitSystem triangleHitSystem = scene.GetOrCreate<TriangleHitSystem>();
            SceneSphereHitSystem sphereHitSystem = scene.GetOrCreate<SceneSphereHitSystem>();
            for (int i = 0; i < spheres.Length; i++)
                sphereHitSystem.Add(spheres[i]);

            List<Triangle> triangles = new List<Triangle>();
            WavefrontLoader.Load(File.ReadAllText(@"Resources/Teapot/teapot.obj"), ref triangles);

            var mat = new MetalMaterial(new Vector3(.25f, .25f, .25f), .4f);
            // var mat = new DielectricMaterial(1.5f);
            for (int i = 0; i < triangles.Count; i++)
            {
                var tri = triangles[i];
                tri.p1 *= .01f; // Scale down
                tri.p2 *= .01f; // Scale down
                tri.p3 *= .01f; // Scale down
                tri.p1 += new Vector3(-1.5f, 1.25f, 0f); // Translate
                tri.p2 += new Vector3(-1.5f, 1.25f, 0f); // Translate
                tri.p3 += new Vector3(-1.5f, 1.25f, 0f); // Translate

                tri.material = mat;

                triangleHitSystem.Add(tri);
            }

            // triangleHitSystem.Add(new Triangle(new Vector3(-2, 0, 1), new Vector3(0, 1, 1), new Vector3(2, 0, 1), mat));

            scene.PrepareForRendering();
            stopwatch.Stop();
            camera = new Camera(new Vector3(0, 2, 3), new Vector3(0, 0, 0), new Vector3(0, 1, 0), 70, (float)width / (float)height, 0.025f, 3f);

            return scene;
        }

        private static TraceResult TraceTest(int width, int height, Scene scene, Camera camera, out Stopwatch stopwatch)
        {
            stopwatch = Stopwatch.StartNew();
            TraceParams parameters = new TraceParams()
            {
                ambientLight = new Vector3(.75f, .75f, .75f),
                camera = camera,
                height = height,
                width = width,
                samplesPerPixel = 2048,
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
            return result;
        }

        static void Main(string[] args)
        {
            int width = 1920;
            int height = 1080;
            Stopwatch swInit, swRender;
            Camera camera;

            // var scene = SimpleSphereScene(width, height, out swInit, out camera);
            var scene = TeapotScene(width, height, out swInit, out camera);
            var result = TraceTest(width, height, scene, camera, out swRender);

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
