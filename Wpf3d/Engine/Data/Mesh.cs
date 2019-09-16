using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Wpf3d.Engine.Data
{
    class Mesh
    {
        public List<Triangle> Triangles = new List<Triangle>();

        public Mesh()
        {
        }

        public Mesh(List<Triangle> triangles)
        {
            this.Triangles = triangles;
        }

        public void Scale(float multiplier)
        {
            HashSet<Vector3d> ps = new HashSet<Vector3d>();
            foreach (var tri in Triangles)
            {
                foreach (var p in tri.p)
                {
                    ps.Add(p);
                }
            }

            foreach (var p in ps)
            {
                p.x *= multiplier;
                p.y *= multiplier;
                p.z *= multiplier;
            }
        }

        /*public Mesh(float[][] points)
        {
            foreach (var p in points)
            {
                Vector3d point = new Vector3d(p[0], p[1], p[2]);
                Triangles.Add(point);
            }
        }*/
    }
}
