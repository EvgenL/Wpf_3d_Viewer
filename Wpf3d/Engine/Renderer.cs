using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Cache;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Wpf3d.Engine.Data;

namespace Wpf3d.Engine
{
    /// <summary>
    /// Draws things to canvas
    /// </summary>
    internal class Renderer
    {
        private float zNear = 0.1f;
        private float zFar = 1000f;
        private float fov = 90f;
        // to be initialized on creation
        private Matrix4x4 projectionMatrix;

        private List<Mesh> processedObjects = new List<Mesh>();

        private Color wireframeColor = Colors.GreenYellow;
        private Color normalColor = Colors.Aqua;
        private Color backgroundColor = Colors.DarkSlateGray;

        // temporary simplifications
        private Vector3d cameraPos = new Vector3d();
        private Vector3d cameraLookDir = new Vector3d(0, 0, 1);
        private float camYaw;
        private Vector3d directedLight = new Vector3d(0, 0, -1);

        private static bool drawNormals = false;
        private static bool drawFps = true;

        private Image imageViewport;
        private Label fpsCounter;
        private static ImageSource testTexture;
        private static WriteableBitmap wb;

        private static Stopwatch stopwatch = new Stopwatch();
        private static float targetFps = 60.0f;

        public Renderer(Image imageViewport, Label fpsCounter)
        {
            this.imageViewport = imageViewport;
            this.fpsCounter = fpsCounter;

            CalculateProjectionMatrix();

            DebugInit();

            InitRender();

            StartRenderTimer();
        }

        private void DebugInit()
        {
            var cube = new MeshCube();
            //cube.Scale(2f);
            //processedObjects.Add(cube);

            //var testMesh = FileLoader.ReadMeshFromObj("models/notebook/Lowpoly_Notebook_2.obj");
            var testMesh = FileLoader.ReadMeshFromObj("models/barrel.obj");
            processedObjects.Add(testMesh);
        }

        private void CalculateProjectionMatrix()
        {
            float aspectRatio = (float)(imageViewport.Height / imageViewport.Width);
            float fFovScaleFactorRad = (float)(1f / Math.Tan(fov * 0.5f / 180f * Math.PI));

            projectionMatrix = Matrix4x4.CreateProjection(aspectRatio, fFovScaleFactorRad, zFar, zNear);
        }

        private void StartRenderTimer()
        {
            // Start render timer at 60 fps
            DispatcherTimer renderTimer = new DispatcherTimer();
            renderTimer.Interval = TimeSpan.FromSeconds(1d / targetFps);
            renderTimer.Tick += (o, args) => Render();
            renderTimer.Start();
            stopwatch.Start();
        }

        private void InitRender()
        {
            wb = BitmapFactory.New((int)imageViewport.Width, (int)imageViewport.Height);
            imageViewport.Source = wb;
        }

        private void Render()
        {
            DebugUpdateInput();

            Random rand = new Random();

            // Clear figures from last frame
            wb.Clear(backgroundColor);
            
            // drawing
            foreach (var ob in processedObjects)
            {
                DrawMesh(ob, wb);
            }

            if (drawFps)
            {
                fpsCounter.Content = "fps:" + (1000f / (float) stopwatch.ElapsedMilliseconds).ToString("F1");
            }

            stopwatch.Restart();
        }

        private void DebugUpdateInput()
        {
            float cameraMoveSpeed = 0.1f;
            float cameraRotationSpeed = 0.04f;

            if (Keyboard.IsKeyDown(Key.E) || Keyboard.IsKeyDown(Key.Space))
            {
                cameraPos.y += -cameraMoveSpeed;
            }
            if (Keyboard.IsKeyDown(Key.Q) || Keyboard.IsKeyDown(Key.LeftShift))
            {
                cameraPos.y -= -cameraMoveSpeed;
            }

            // rotate camera look dir to 90 degree. todo vector rotation methods
            Vector3d rightVelocity 
                = Matrix4x4.MultiplyVector(Matrix4x4.RotationMatrixY(-90f / 180f * (float)Math.PI), cameraLookDir) * cameraMoveSpeed;

            if (Keyboard.IsKeyDown(Key.D))
            {
                cameraPos += rightVelocity;
            }
            if (Keyboard.IsKeyDown(Key.A))
            {
                cameraPos -= rightVelocity;
            }

            Vector3d forwardVelocity = cameraLookDir * cameraMoveSpeed;

            if (Keyboard.IsKeyDown(Key.W))
            {
                cameraPos += forwardVelocity;
            }
            if (Keyboard.IsKeyDown(Key.S))
            {
                cameraPos -= forwardVelocity;
            }

            if (Keyboard.IsKeyDown(Key.Left))
            {
                camYaw += cameraRotationSpeed;
            }
            if (Keyboard.IsKeyDown(Key.Right))
            {
                camYaw -= cameraRotationSpeed;
            }
        }

        float theta = 0;
        private void DrawMesh(Mesh mesh, WriteableBitmap wb)
        {
            // create matrix to transform a triangle
            theta += (float)stopwatch.ElapsedMilliseconds / 1000f;
            Matrix4x4 rotZ = Matrix4x4.RotationMatrixZ(theta/2);
            Matrix4x4 rotX = Matrix4x4.RotationMatrixX(theta);
            Matrix4x4 translationMat = Matrix4x4.CreateTranslation(0, 0, 3);

            Matrix4x4 worldMat = Matrix4x4.CreateIdentity();
            worldMat = rotZ * rotX;
            worldMat = worldMat * translationMat;

            // create camera matrix
            Vector3d up = new Vector3d(0, 1, 0);
            // rotate camera 
            Matrix4x4 camRotY = Matrix4x4.RotationMatrixY(camYaw);
            Vector3d target = new Vector3d(0, 0, 1);
            cameraLookDir = Matrix4x4.MultiplyVector(camRotY, target);
            target = cameraPos + cameraLookDir;

            Matrix4x4 cameraMat = Matrix4x4.CreatePointAt(cameraPos, target, up);
            //invert camera matrix to get View matrix
            Matrix4x4 matView = Matrix4x4.TransformPointAtToLookAt(cameraMat);

            List<Triangle> trianglesToRaster = new List<Triangle>();

            Vector3d normalizedLight = directedLight.Normalized();

            foreach (var triangle in mesh.Triangles)
            {
                Triangle triTransformed = Triangle.MultiplyPointsByMatrix(triangle, worldMat);

                // calculate normal (if wasn't present in loaded file)
                Vector3d normal = triTransformed.GetNormal();

                // dont draw if triangle is not seen
                Vector3d cameraRay = triTransformed.p[0] - cameraPos;
                // get dot product of direction from camera and normal (скалярное произведение нормали на вектор от камеры к точке)
                // dot product is < 0 if angle between vectors is more than 90"
                float camRayToNormal = Vector3d.DotProduct(cameraRay, normal);

                // skip if not seen
                if (camRayToNormal > 0) continue;
                //if (camRayToNormal < 0) todo see inside;

                // illumination
                float normalToLightDot = Vector3d.DotProduct(normalizedLight, normal);
                byte shadeOfGrey = (byte)(255 * normalToLightDot);
                var triangleColor = Color.FromRgb(shadeOfGrey, shadeOfGrey, shadeOfGrey);
                triTransformed.SetColor(triangleColor);

                // camera movement (word space to view space)
                Triangle triViewed = Triangle.MultiplyPointsByMatrix(triTransformed, matView);

                // clip triangles that are behind the camera (near plane)
                Triangle[] clipped = Triangle.ClipTriangleToPlane(
                    new Vector3d(0, 0, zNear), new Vector3d(0, 0, 1), triViewed);

                foreach (var triClip in clipped)
                {
                    // project on screen 3D -> 2D                              
                    Triangle triProjected = Triangle.MultiplyPointsByProjectionMatrix(triClip, projectionMatrix);

                    // map to screen coordinates
                    RenderMath.ScaleProjectedTriangle(ref triProjected, (int)imageViewport.Width, (int)imageViewport.Height);

                    trianglesToRaster.Add(triProjected);
                }
            }

            // sort triangles (painter alghoritm)
            trianglesToRaster.Sort((a, b) =>
            {
                float z1 = a.p[0].z + a.p[1].z + a.p[2].z / 3.0f;
                float z2 = b.p[0].z + b.p[1].z + b.p[2].z / 3.0f;
                return z1 > z2 ? 1 : z1 < z2 ? -1 : 0;
            });


            // draw 
            foreach (var triangle in trianglesToRaster)
            {

                // clip triangles
                // do after sorting because the number of triangles may change but their order wont

                Queue<Triangle> trianglesQ = new Queue<Triangle>();
                Queue<Triangle> trianglesResult = new Queue<Triangle>();
                trianglesQ.Enqueue(triangle);

                bool changed = false;
                while (changed)
                {
                    Triangle test = trianglesQ.Dequeue();

                    Triangle[] newTris = new Triangle[0];
                    for (int i = 0; i < 4; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                newTris = Triangle.ClipTriangleToPlane(new Vector3d(0, 0, 0), new Vector3d(0, 1, 0), test); // top of the screen
                                break;
                            case 1:
                                newTris = Triangle.ClipTriangleToPlane(new Vector3d(0, (float)imageViewport.Height, 0), new Vector3d(0, -1, 0), test); // bottom
                                break;
                            case 2:
                                newTris = Triangle.ClipTriangleToPlane(new Vector3d(0, 0, 0), new Vector3d(1, 0, 0), test); // left
                                break;
                            case 3:
                                newTris = Triangle.ClipTriangleToPlane(new Vector3d((float)imageViewport.Width, 0, 0), new Vector3d(-1, 0, 0), test); // right
                                break;
                        }
                    }

                    if (newTris.Length > 0)
                    {
                        foreach (var newTri in newTris)
                        {
                            trianglesQ.Enqueue(newTri);
                        }
                        changed = true;
                    }
                    else
                    {
                        trianglesResult.Enqueue(test);
                    }
                }


                var triDraw = triangle;
                //todo foreach (var triDraw in trianglesResult)
                {
                    // shade
                    FillTriangle(triDraw, wb, triangle.Color);

                    // texture
                    //TextureTriangle(triDraw, wb);
                    
                    // draw wireframe
                    DrawTriangleWireframe(triDraw, wb, wireframeColor);
                    
                    // draw normals
                    if (true||drawNormals)
                    {
                        var cp = triangle.GetCenterPoint();
                        DrawPoint(cp, wb, normalColor);
                        //DrawRay(triangle.GetCenterPoint(), triangle.GetNormal() * 50, drawingContext, normalsPen);
                    }
                }
                

            }
        }

        private static void TextureTriangle(Triangle tri, WriteableBitmap bitmap/*, Texture texture*/)
        {
        }

        private static void FillTriangle(Triangle tri, WriteableBitmap bitmap, Color color)
        {
            bitmap.FillTriangle((int)tri.p[0].x, (int)tri.p[0].y, (int)tri.p[1].x, (int)tri.p[1].y,
                (int)tri.p[2].x, (int)tri.p[2].y, color);
        }

        private static void DrawTriangleWireframe(Triangle tri, WriteableBitmap bitmap, Color color)
        {
            bitmap.DrawLine((int)tri.p[0].x, (int)tri.p[0].y, (int)tri.p[1].x, (int)tri.p[1].y, color);
            bitmap.DrawLine((int)tri.p[1].x, (int)tri.p[1].y, (int)tri.p[2].x, (int)tri.p[2].y, color);
            bitmap.DrawLine((int)tri.p[2].x, (int)tri.p[2].y, (int)tri.p[0].x, (int)tri.p[0].y, color);
        }

        private static void DrawLine(Vector3d position, Vector3d target, DrawingContext drawingContext,
            Pen pen){
            drawingContext.DrawLine(pen, new Point(position.x, position.y),
                new Point(target
.x, target.y));
        }

        private static void DrawRay(Vector3d position, Vector3d direction, DrawingContext drawingContext,
            Pen pen)
        {
            Vector3d posTarget = position + direction;

            DrawLine(position, posTarget, drawingContext, pen);
        }

        private static void DrawPoint(Vector3d position, WriteableBitmap bitmap, Color color)
        {
            bitmap.FillEllipse(
                (int)(position.x) - 1, (int)(position.y) - 1,
                (int)(position.x) + 1, (int)(position.y) + 1,
                color);
        }

        private static void DrawText(string text, int size, Point pos, DrawingContext drawingContext, Pen pen)
        {
            drawingContext.DrawText(
                new FormattedText(text,
                    CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight,
                    new Typeface("Verdana"),
                    size, pen.Brush),
                pos);
        }
    }

}
