using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wpf3d.Engine.Data;

namespace Wpf3d.Engine
{
    static class FileLoader
    {
        public static Mesh ReadMeshFromObj(string path)
        {
            FileStream stream = File.OpenRead(path);
            StreamReader reader = new StreamReader(stream);

            Mesh mesh = new Mesh();
            List<Vector3d> vertices = new List<Vector3d>();

            // parse with culture that takes dot (not comma) as decimal serparator
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] split = line.Split(' ');

                if (line.StartsWith("v "))
                {

                    vertices.Add(new Vector3d(
                        float.Parse(split[1], NumberStyles.Any, ci),
                        float.Parse(split[2], NumberStyles.Any, ci),
                        float.Parse(split[3], NumberStyles.Any, ci)
                    ));

                }

                if (line.StartsWith("f "))
                {
                    int[] indexes = new int[3];
                    indexes[0] = int.Parse(split[1].Split('/')[0]) - 1;
                    indexes[1] = int.Parse(split[2].Split('/')[0]) - 1;
                    indexes[2] = int.Parse(split[3].Split('/')[0]) - 1;

                    mesh.Triangles.Add(new Triangle(
                        vertices[indexes[0]],
                        vertices[indexes[1]],
                        vertices[indexes[2]]
                        ));
                }
            }


            return mesh;
        }
    }
}
