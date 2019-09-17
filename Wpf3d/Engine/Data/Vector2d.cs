using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf3d.Engine.Data
{
    // two dimesional vetor used for texture coordinates
    public class Vector2d
    {
        public Vector2d()
        {
        }

        public Vector2d(float u, float v)
        {
            this.u = u;
            this.v = v;
        }

        public static Vector2d operator *(Vector2d a, Vector2d b)
        {
            return new Vector2d(a.u * b.u, a.v * b.v);
        }

        public static Vector2d operator +(Vector2d a, Vector2d b)
        {
            return new Vector2d(a.u + b.u, a.v + b.v);
        }

        public static Vector2d operator -(Vector2d a, Vector2d b)
        {
            return new Vector2d(a.u - b.u, a.v - b.v);
        }

        public static Vector2d operator +(float value, Vector2d a)
        {
            return new Vector2d(a.u + value, a.v + value);
        }

        public static Vector2d operator -(float value, Vector2d a)
        {
            return new Vector2d(a.u - value, a.v - value);
        }
        public static Vector2d operator *(float value, Vector2d a)
        {
            return new Vector2d(a.u * value, a.v * value);
        }

        public float u, v;
    }
}
