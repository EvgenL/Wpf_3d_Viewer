using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf3d.Engine.Data;

namespace Wpf3d.Engine
{
    public static class RenderMath
    {
        public static void MulVetorMatr(Vector3d v, ref Vector3d result, Matrix4x4 m)
        {
            result.x = v.x * m[0, 0] + v.y * m[1, 0] + v.z * m[2, 0] + m[3, 0];
            result.y = v.x * m[0, 1] + v.y * m[1, 1] + v.z * m[2, 1] + m[3, 1];
            result.z = v.x * m[0, 2] + v.y * m[1, 2] + v.z * m[2, 2] + m[3, 2];
            float w = v.x * m[0, 3] + v.y * m[1, 3] + v.z * m[2, 3] + m[3, 3];

            if (w != 0)
            {
                result.x /= w;
                result.y /= w;
                result.z /= w;
            }
        }
        
        // todo to camera?
        public static void ScaleProjectedTriangle(ref Triangle tri, int screenW, int screenH)
        {
            for (int i = 0; i < tri.p.Length; i++)
            {
                ScaleProjectedPoint(ref tri.p[i], screenW, screenH);
            }
        }

        public static void ScaleProjectedPoint(ref Vector3d point, int screenW, int screenH)
        {
            // from [-1; 1] to [0; 2]
            point += 1.0f;
            // from [0; 2] to [0; screen_size]
            point.x *= 0.5f * (float) screenW;
            point.y *= 0.5f * (float) screenH;
        }

        public static Vector3d LineIntersectPlane(Vector3d planeP, Vector3d planeN, Vector3d lineStart,
            Vector3d lineEnd, out float t)
        {
            planeN = planeN.Normalized();
            float planeD = -(Vector3d.DotProduct(planeN, planeP));
            float ad = Vector3d.DotProduct(lineStart, planeN);
            float bd = Vector3d.DotProduct(lineEnd, planeN);
            /*float*/ t = (-planeD - ad) / (bd - ad);
            Vector3d lineStartToEnd = lineEnd - lineStart;
            Vector3d lineToIntersect = lineStartToEnd * t;
            return lineStart + lineToIntersect;
        }


    }
}
