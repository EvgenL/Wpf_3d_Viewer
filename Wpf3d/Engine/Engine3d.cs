using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wpf3d.Engine
{
    internal class Engine3d
    {

        private static Engine3d _instance;

        private Renderer renderer;

        public static void Start(Image imageViewport, Label fpsCounter)
        {
            _instance = new Engine3d();

            _instance.RendererInit(imageViewport, fpsCounter);

            Thread mainThread = new Thread(new ThreadStart(_instance.MainThread));
            mainThread.Start();
        }

        private void MainThread()
        {
            Initialize();

            while (true)
            {
                GetInput();
                Update();
                Render();
            }
        }

        private void RendererInit(Image imageViewport, Label fpsCounter)
        {
            renderer = new Renderer(imageViewport, fpsCounter);
        }

        protected virtual void Initialize()
        {
        }

        protected virtual void GetInput()
        {
        }

        protected virtual void Update()
        {
        }

        private void Render()
        {
        }

    }
}
