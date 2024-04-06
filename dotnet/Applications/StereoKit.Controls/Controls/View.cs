using System;
using System.Drawing;

namespace StereoKit.UIX.Controls
{
    public class View : IDisposable
    {
        public bool IsEnabled { get; set; }
        public SizeF Measured { get; set; }
        public SizeF MinimumSize { get; set; }
        public SizeF NaturalSize { get; set; }
        public View Parent { get; set; }
        public Color BackgroundColor { get; set; }
        public View Layout { get; set; }

        public object Border { get; set; }
        public float Opacity { get; set; }
        public object Clip { get; set; }
        public SizeF Size { get; set; }
        public Rect Bounds { get; set; }
        public bool Sensitive { get; set; }
        public LayoutParamPolicies WidthSpecification { get; set; }
        public LayoutParamPolicies HeightSpecification { get; set; }
        public ResizePolicyType HeightResizePolicy { get; set; }
        public ResizePolicyType WidthResizePolicy { get; set; }

        public event Action<object?, EventArgs> LayoutUpdated;

        public event Action<object, EventArgs> FocusGained;
        public event Action<object, EventArgs> FocusLost;

        public void MarkChanged() { }

        public void Dispose() { }

        public Rect GetBounds() => Bounds;
        protected virtual bool HitTest(object touch) => false;

        public void Hide() { }

        public void Show() { }

        public void UpdateBackgroundColor(Color brush) { }

        public void UpdateBounds(Rect bounds) => Bounds = bounds;
        public void UpdateSize(SizeF size) => Size = size;

        public void RequestLayout() { }

        public View GetParent() => Parent;

        public virtual void OnStep(object? arg) { }
    }
}