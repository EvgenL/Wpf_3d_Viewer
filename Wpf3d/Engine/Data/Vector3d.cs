using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf3d.Engine.Data
{
    public class Vector3d : ICloneable
    {
        public float x, y, z;
        public float w = 1;

        public Vector3d()
        {
            x = y = z = w = 0;
        }

        public Vector3d(float x, float y, float z)
        {
            this.x = x;  this.y = y;  this.z = z;
        }

        public object Clone()
        {
            return new Vector3d(x, y, z);
        }

        // get length of vector
        public float Magnitude()
        {
            return (float) Math.Sqrt(x * x + y * y + z * z);
        }

        // normalize (привести к единичному вектору)
        public Vector3d Normalized()
        {
            // magnitude (length) of the vector
            float m = this.Magnitude();

            return new Vector3d(x / m, y / m, z / m);
        }


        public static Vector3d operator +(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3d operator +(Vector3d a, float n)
        {
            return new Vector3d(a.x + n, a.y + n, a.z + n);
        }

        public static Vector3d operator -(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
        }
        public static Vector3d operator -(Vector3d a)
        {
            return new Vector3d(-a.x, -a.y, -a.z);
        }

        public static Vector3d CrossProduct(Vector3d a, Vector3d b)
        {
            return new Vector3d(
                a.y * b.z - a.z * b.y,
                a.z * b.x - a.x * b.z,
                a.x * b.y - a.y * b.x);
        }

        public static float DotProduct(Vector3d a, Vector3d b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public static Vector3d operator *(Vector3d a, float mul)
        {
            return new Vector3d(a.x * mul, a.y * mul, a.z * mul);
        }

        public static Vector3d operator /(Vector3d a, float div)
        {
            return new Vector3d(a.x / div, a.y / div, a.z / div);
        }

        public static float Distance(Vector3d a, Vector3d b)
        {
            return (float)Math.Sqrt(
                Math.Pow((a.x - b.x), 2) + 
                Math.Pow((a.y - b.y), 2) + 
                Math.Pow((a.z - b.z), 2));
        }
    }
}
