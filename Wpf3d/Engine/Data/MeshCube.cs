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

            Vector2d[] uv1 = new Vector2d[]{ new Vector2d(0, 1), new Vector2d(0, 0), new Vector2d(1 , 0) };
            Vector2d[] uv2 = new Vector2d[]{ new Vector2d(0, 1), new Vector2d(1, 0), new Vector2d(1 , 1) };

            Triangle bad = new Triangle(b,a,d);
            Triangle cbd = new Triangle(c,b,d);
            bad.uv = uv1;
            cbd.uv = uv2;

            Triangle lcd = new Triangle(l,c,d);
            Triangle kld = new Triangle(k,l,d);
            lcd.uv = uv1;
            kld.uv = uv2;

            Triangle and = new Triangle(a,n,d);
            Triangle nkd = new Triangle(n,k,d);
            and.uv = uv1;
            nkd.uv = uv2;

            Triangle abm = new Triangle(a,b,m);
            Triangle nam = new Triangle(n,a,m);
            abm.uv = uv1;
            nam.uv = uv2;

            Triangle bcm = new Triangle(b,c,m);
            Triangle clm = new Triangle(c,l,m);
            bcm.uv = uv1;
            clm.uv = uv2;

            Triangle knm = new Triangle(k,n,m);
            Triangle lkm = new Triangle(l,k,m);
            knm.uv = uv1;
            lkm.uv = uv2;

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

            foreach (var triangle in Triangles)
            {

                for (int i = 0; i < triangle.p.Length; i++)
                {
                    triangle.uv[i].u = triangle.p[i].x;
                    triangle.uv[i].v = triangle.p[i].y;
                }
            }
        }
    }
}
