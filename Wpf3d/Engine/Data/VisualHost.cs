using System.Windows;
using System.Windows.Media;

namespace Wpf3d.Engine.Data
{
    public class VisualHost : UIElement
    {
        public VisualHost(Visual visual)
        {
            this.Visual = visual;
        }

        public Visual Visual { get; set; }

        protected override int VisualChildrenCount
        {
            get { return Visual != null ? 1 : 0; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return Visual;
        }
    }
}