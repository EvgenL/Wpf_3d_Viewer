using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf3d.Engine.Data
{
    class MeshCube : Mesh
    {
        public MeshCube()
        {
            Vector3d a = new Vector3d(0, 0, 0);
            Vector3d b = new Vector3d(1, 0, 0);
            Vector3d c = new Vector3d(1, 1, 0);
            Vector3d d = new Vector3d(0, 1, 0);
            Vector3d n = new Vector3d(0, 0, 1);
            Vector3d m = new Vector3d(1, 0, 1);
            Vector3d k = new Vector3d(0, 1, 1);
            Vector3d l = new Vector3d(1, 1, 1);

            Triangle bad = new Triangle(b,a,d);
            Triangle cbd = new Triangle(c,b,d);
            Triangle lcd = new Triangle(l,c,d);
            Triangle kld = new Triangle(k,l,d);
            Triangle and = new Triangle(a,n,d);
            Triangle nkd = new Triangle(n,k,d);
            Triangle abm = new Triangle(a,b,m);
            Triangle nam = new Triangle(n,a,m);
            Triangle bcm = new Triangle(b,c,m);
            Triangle clm = new Triangle(c,l,m);
            Triangle knm = new Triangle(k,n,m);
            Triangle lkm = new Triangle(l,k,m);

            Triangles.Add(bad);
            Triangles.Add(cbd);
            Triangles.Add(lcd);
            Triangles.Add(kld);
            Triangles.Add(and);
            Triangles.Add(nkd);
            Triangles.Add(abm);
            Triangles.Add(nam);
            Triangles.Add(bcm);
            Triangles.Add(clm);
            Triangles.Add(knm);
            Triangles.Add(lkm);
        }
    }
}
