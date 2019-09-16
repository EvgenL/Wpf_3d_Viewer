using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf3d.Engine.Data;

namespace Wpf3d.Engine
{
    public class Matrix4x4
    {
        private float[,] m = new float[4,4];

        public float this[int i, int j]
        {
            get { return m[i, j]; }
            set { m[i, j] = value; }
        }

        public static Vector3d MultiplyVector(Matrix4x4 m, Vector3d vec)
        {
            Vector3d result = new Vector3d();
            result.x = vec.x * m[0, 0] + vec.y * m[1, 0] + vec.z * m[2, 0] + vec.w * m[3, 0];
            result.y = vec.x * m[0, 1] + vec.y * m[1, 1] + vec.z * m[2, 1] + vec.w * m[3, 1];
            result.z = vec.x * m[0, 2] + vec.y * m[1, 2] + vec.z * m[2, 2] + vec.w * m[3, 2];
            result.w = vec.x * m[0, 3] + vec.y * m[1, 3] + vec.z * m[2, 3] + vec.w * m[3, 3];
            return result;
        }

        public static Matrix4x4 CreateIdentity()
        {
            Matrix4x4 mat = new Matrix4x4();
            mat[0, 0] = 1;
            mat[1, 1] = 1;
            mat[2, 2] = 1;
            mat[3, 3] = 1;
            return mat;
        }

        public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b)
        {
            Matrix4x4 result = new Matrix4x4();

            float[,] A = a.m;
            float[,] B = b.m;

            for (int c = 0; c < 4; c++)
            {
                for (int r = 0; r < 4; r++)
                {
                    result[r, c] = A[r, 0] * B[0, c] 
                                   + A[r, 1] * B[1, c] 
                                   + A[r, 2] * B[2, c] 
                                   + A[r, 3] * B[3, c];
                }
            }

            return result;
        }

        public static Matrix4x4 RotationMatrixX(float angleRad)
        {
            Matrix4x4 rotX = new Matrix4x4();
            rotX[0, 0] = 1;
            rotX[1, 1] = (float)Math.Cos(angleRad * 0.5f);
            rotX[1, 2] = (float)Math.Sin(angleRad * 0.5f);
            rotX[2, 1] = -(float)Math.Sin(angleRad * 0.5f);
            rotX[2, 2] = (float)Math.Cos(angleRad * 0.5f);
            rotX[3, 3] = 1;
            return rotX;
        }

        public static Matrix4x4 RotationMatrixY(float angleRad)
        {
            Matrix4x4 rotY = new Matrix4x4();
            rotY[0, 0] = (float)Math.Cos(angleRad);
            rotY[0, 2] = (float)Math.Sin(angleRad);
            rotY[2, 0] = -(float)Math.Sin(angleRad);
            rotY[1, 1] = 1;
            rotY[2, 2] = (float)Math.Cos(angleRad);
            rotY[3, 3] = 1;
            return rotY;
        }

        public static Matrix4x4 RotationMatrixZ(float angleRad)
        {
            Matrix4x4 rotZ = new Matrix4x4();
            rotZ[0, 0] = (float)Math.Cos(angleRad);
            rotZ[0, 1] = (float)Math.Sin(angleRad);
            rotZ[1, 0] = -(float)Math.Sin(angleRad);
            rotZ[1, 1] = (float)Math.Cos(angleRad);
            rotZ[2, 2] = 1;
            rotZ[3, 3] = 1;
            return rotZ;
        }

        public static Matrix4x4 CreateTranslation(float x, float y, float z)
        {  
            var result = new Matrix4x4();

            result[0, 0] = 1.0f;
            result[1, 1] = 1.0f;
            result[2, 2] = 1.0f;
            result[3, 3] = 1.0f;

            result[3, 0] = x;
            result[3, 1] = y;
            result[3, 2] = z;

            return result;
        }

        public static Matrix4x4 CreateProjection(float aspectRatio, float fFovScaleFactorRad, float zFar, float zNear)
        {
            return new Matrix4x4
            {
                m =
                {
                    [0, 0] = aspectRatio * fFovScaleFactorRad,
                    [1, 1] = fFovScaleFactorRad,
                    [2, 2] = zFar / (zFar - zNear),
                    [3, 2] = (zFar * zNear) / (zFar - zNear),
                    [2, 3] = 1f,
                    [3, 3] = 0f
                }
            };
        }

        public static Matrix4x4 CreatePointAt(Vector3d pos, Vector3d target, Vector3d up)
        {
            // create forward vector - direction from 'pos' to 'target'
            Vector3d newForward = target - pos;
            newForward = newForward.Normalized();

            // create new up vector - perpendicular to 'new forward'
            Vector3d A = newForward * (Vector3d.DotProduct(up, newForward));
            Vector3d newUp = up - A;
            newUp = newUp.Normalized();

            // create right vector - perpendicular to both up and forward
            Vector3d newRight = Vector3d.CrossProduct(newUp, newForward);

            // create PointAt matrix that contains calculated directions and position 
            Matrix4x4 mat = new Matrix4x4();
            mat[0, 0] = newRight.x;     mat[0, 1] = newRight.y;    mat[0, 2] = newRight.z;    mat[0, 3] = 0;
            mat[1, 0] = newUp.x;        mat[1, 1] = newUp.y;       mat[1, 2] = newUp.z;       mat[1, 3] = 0;
            mat[2, 0] = newForward.x;   mat[2, 1] = newForward.y;  mat[2, 2] = newForward.z;  mat[2, 3] = 0;
            mat[3, 0] = pos.x;          mat[3, 1] = pos.y;         mat[3, 2] = pos.z;         mat[3, 3] = 1;

            return mat;
        }

        // Quick inverse that can be applied only to pointAt matrix
        public static Matrix4x4 TransformPointAtToLookAt(Matrix4x4 pointAt)
        {
            Matrix4x4 mat = new Matrix4x4();
            mat[0, 0] = pointAt[0, 0]; mat[0, 1] = pointAt[1, 0]; mat[0, 2] = pointAt[2, 0]; mat[0, 3] = pointAt[3, 0];
            mat[1, 0] = pointAt[0, 1]; mat[1, 1] = pointAt[1, 1]; mat[1, 2] = pointAt[2, 1]; mat[1, 3] = pointAt[3, 1];
            mat[2, 0] = pointAt[0, 2]; mat[2, 1] = pointAt[1, 2]; mat[2, 2] = pointAt[2, 2]; mat[2, 3] = pointAt[3, 2];
            mat[3, 0] = -(pointAt[3, 0] * mat[0, 0] + pointAt[3, 1] * mat[1, 0] + pointAt[3, 2] * mat[2, 0]);
            mat[3, 1] = -(pointAt[3, 0] * mat[0, 1] + pointAt[3, 1] * mat[1, 1] + pointAt[3, 2] * mat[2, 1]);
            mat[3, 2] = -(pointAt[3, 0] * mat[0, 2] + pointAt[3, 1] * mat[1, 2] + pointAt[3, 2] * mat[2, 2]);
            mat[3, 3] = 1;
            return mat;
        }


    }
}
