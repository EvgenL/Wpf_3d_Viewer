using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Wpf3d.Engine.Data
{
    // using class and not struct to constrain points array size to [3]
    public class Triangle : ICloneable
    {
        public Vector3d Normal = null;

        public Vector3d[] p = new Vector3d[3]; // vertex coordinates
        public Vector2d[] uv = new Vector2d[3]; // texture coordinates

        public Color Color;
        public bool Colored = false;
        public static bool visualiseClipping = false;

        public Triangle()
        {
            for (int i = 0; i < p.Length; i++)
            {
                p[i] = new Vector3d();
            }
        }

        public Triangle(Vector3d a, Vector3d b, Vector3d c)
        {
            p[0] = a;  p[1] = b;  p[2] = c;
        }
        public Triangle(Vector3d[] vectors)
        {
            p[0] = vectors[0];  p[1] = vectors[1];  p[2] = vectors[2];
        }

        public Vector3d GetCenterPoint()
        {
            Vector3d sum = p[0] + p[1] + p[2];
            return sum / 3 ;
        }

        public Vector3d GetNormal()
        {
            if (Normal == null)
            {
                return CalculateNormal();
            }
            return Normal;
        }

        public Vector3d CalculateNormal()
        {
            // two vectors for cross product
            Vector3d a = p[1] - p[0];
            Vector3d b = p[2] - p[0];

            Normal = Vector3d.CrossProduct(a, b);
            Normal = Normal.Normalized();
            return Normal;
        }

        public object Clone()
        {
            Triangle tri = new Triangle((Vector3d)p[0].Clone(), (Vector3d)p[1].Clone(), (Vector3d)p[2].Clone());
            tri.Colored = Colored;
            tri.Color = Color;
            tri.uv = uv;
            tri.Normal = Normal;
            return tri;
        }

        public static Triangle MultiplyPointsByMatrix(Triangle tri, Matrix4x4 mat)
        {
            Triangle result = (Triangle)tri.Clone();

            result.p[0] = Matrix4x4.MultiplyVector(mat, result.p[0]);
            result.p[1] = Matrix4x4.MultiplyVector(mat, result.p[1]);
            result.p[2] = Matrix4x4.MultiplyVector(mat, result.p[2]);

            return result;
        }

        public static Triangle MultiplyPointsByProjectionMatrix(Triangle tri, Matrix4x4 mat)
        {
            Triangle result = (Triangle)tri.Clone();

            result.p[0] = Matrix4x4.MultiplyVector(mat, result.p[0]);
            result.p[1] = Matrix4x4.MultiplyVector(mat, result.p[1]);
            result.p[2] = Matrix4x4.MultiplyVector(mat, result.p[2]);

            for (int i = 0; i < result.p.Length; i++)
            {
                if (result.p[i].w != 0.0f)
                {
                    result.p[i] /= result.p[i].w;
                }
            }

            return result;
        }

        public static Triangle[] ClipTriangleToPlane(Vector3d planeP, Vector3d planeN, Triangle tri)
        {
            planeN = planeN.Normalized();

            List<Vector3d> outsidePoints = new List<Vector3d>(3);
            List<Vector3d> insidePoints = new List<Vector3d>(3);
            List<Vector2d> outsideTex = new List<Vector2d>(3);
            List<Vector2d> insideTex = new List<Vector2d>(3);


            // find if points are inside or outside of the plane
            float planePN = Vector3d.DotProduct(planeN, planeP);
            for (int i = 0; i < tri.p.Length; i++)
            {
                // signed min distance from point to plane
                float d = Vector3d.DotProduct(tri.p[i], planeN) - planePN;

                if (d >= 0)
                {
                    insidePoints.Add(tri.p[i]);
                    insideTex.Add(tri.uv[i]);
                }
                else
                {
                    outsidePoints.Add(tri.p[i]);
                    outsideTex.Add(tri.uv[i]);
                }
            }

            // break input triangle if needed

            if (insidePoints.Count == 0)
            {
                // all points lie outside the plane
                // no new triangles are generated. retun empty array
                return new Triangle[0];
            }
            if (insidePoints.Count == 3)
            {
                // all points are in plane
                // original triangle would not be changed
                return new[] {tri};
            }

            Triangle newTri1 = new Triangle();
            newTri1.SetColor(tri.Color);
            newTri1.Normal = tri.Normal;

            float t; // how much of a edge was cut in range of [0; 1]

            if (insidePoints.Count == 1 && outsidePoints.Count == 2)
            {
                // other two points are outside of the plane so set them to be at intersect position
                // the one inside point is valid
                newTri1.p[0] = insidePoints[0];
                newTri1.uv[0] = insideTex[0];

                newTri1.p[1] = RenderMath.LineIntersectPlane(planeP, planeN, insidePoints[0], outsidePoints[0], out t);
                newTri1.uv[1] = t * (outsideTex[0] - insideTex[0]);

                newTri1.p[2] = RenderMath.LineIntersectPlane(planeP, planeN, insidePoints[0], outsidePoints[1], out t);
                newTri1.uv[2] = t * (outsideTex[1] - insideTex[0]);

                if (visualiseClipping)
                {
                    newTri1.SetColor(Colors.Green);
                }

                // ret new triangle
                return new[] { newTri1 };
            }

            if (insidePoints.Count == 2 && outsidePoints.Count == 1)
            {
                // if two points are inside and one outside
                // quad should be formed as two new triangles

                Triangle newTri2 = new Triangle();

                // the two inside points are valid
                newTri1.p[0] = insidePoints[0];
                newTri1.uv[0] = insideTex[0];

                newTri1.p[1] = insidePoints[1];
                newTri1.uv[1] = insideTex[1];

                newTri1.p[2] = RenderMath.LineIntersectPlane(planeP, planeN, insidePoints[0], outsidePoints[0], out t);
                newTri1.uv[1] = t * (outsideTex[0] - insideTex[0]);

                if (visualiseClipping)
                {
                    newTri1.SetColor(Colors.GreenYellow);
                }

                // other two points are outside of the plane so set them to be at intersect point
                newTri2.p[0] = insidePoints[1];
                newTri1.uv[0] = insideTex[1];

                newTri2.p[1] = newTri1.p[2];
                newTri1.uv[1] = newTri1.uv[1];

                newTri2.p[2] = RenderMath.LineIntersectPlane(planeP, planeN, insidePoints[1], outsidePoints[0], out t);
                newTri1.uv[2] = t * (outsideTex[0] - insideTex[1]);


                newTri2.SetColor(tri.Color);
                newTri2.Normal = tri.Normal;

                if (visualiseClipping)
                {
                    newTri2.SetColor(Colors.LightGreen);
                }

                // ret new triangleS
                return new[] { newTri1, newTri2 };
            }

            throw new Exception("Wrong triangle clipping"); // we'll never get there
        }

        public void SetColor(Color color)
        {
            Color = color;
            Colored = true;
        }
        
    }

}
