using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Globalization;

namespace PathTracer.Test
{
    public static class WavefrontLoader
    {
        public struct Face
        {
            public int p1;
            public int p2;
            public int p3;

            public int t1;
            public int t2;
            public int t3;

            public int n1;
            public int n2;
            public int n3;
        }

        public static void Load(string content, ref List<Triangle> triangles)
        {
            List<Vector3> positions, normals;
            List<Face> faces;
            Load(content, out positions, out normals, out faces);

            if (triangles == null)
                triangles = new List<Triangle>();

            foreach (var face in faces)
            {
                Vector3 p1 = positions[face.p1];
                Vector3 p2 = positions[face.p2];
                Vector3 p3 = positions[face.p3];

                triangles.Add(new Triangle(p1, p2, p3, null));
            }
        }

        public static void Load(string content, out List<Vector3> positions, out List<Vector3> normals, out List<Face> faces)
        {
            positions = new List<Vector3>();
            normals = new List<Vector3>();
            faces = new List<Face>();

            string[] lines = content.Split('\n');
            foreach (var line in lines)
            {
                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                    continue;

                switch (parts[0])
                {
                    case "v":
                        {
                            positions.Add(new Vector3(float.Parse(parts[1], CultureInfo.InvariantCulture), float.Parse(parts[2], CultureInfo.InvariantCulture), float.Parse(parts[3], CultureInfo.InvariantCulture)));
                        }
                        break;
                    case "vn":
                        {
                            normals.Add(new Vector3(float.Parse(parts[1], CultureInfo.InvariantCulture), float.Parse(parts[2], CultureInfo.InvariantCulture), float.Parse(parts[3], CultureInfo.InvariantCulture)));
                        }
                        break;
                    case "f":
                        {
                            string[] faceParts1 = parts[1].Split('/');
                            string[] faceParts2 = parts[2].Split('/');
                            string[] faceParts3 = parts[3].Split('/');

                            if (faceParts1.Length != faceParts2.Length || faceParts2.Length != faceParts3.Length)
                                throw new FormatException("Face has different channel counts");

                            Face face = new Face();
                            face.t1 = face.t2 = face.t3 = face.n1 = face.n2 = face.n3 = -1;

                            face.p1 = int.Parse(faceParts1[0]) - 1;
                            face.p2 = int.Parse(faceParts2[0]) - 1;
                            face.p3 = int.Parse(faceParts3[0]) - 1;

                            if (faceParts1.Length > 1)
                            {
                                face.t1 = int.Parse(faceParts1[1]) - 1;
                                face.t2 = int.Parse(faceParts2[1]) - 1;
                                face.t3 = int.Parse(faceParts3[1]) - 1;
                            }

                            if (faceParts1.Length > 2)
                            {
                                face.n1 = int.Parse(faceParts1[2]) - 1;
                                face.n2 = int.Parse(faceParts2[2]) - 1;
                                face.n3 = int.Parse(faceParts3[2]) - 1;
                            }

                            faces.Add(face);
                        }
                        break;
                    case "vt":
                        {

                        }
                        break;
                    default:
                        {
                            // Console.WriteLine("Unknown " + line);
                        }
                        break;
                }
            }
        }
    }
}