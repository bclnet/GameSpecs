using Microsoft.Maui.Graphics;
using MRect = Microsoft.Maui.Graphics.Rect;

namespace StereoKit.Maui.Handlers
{
    public abstract partial class SKViewHandler<TVirtualView, TPlatformView>
    {
        public override void PlatformArrange(MRect rect) { }

        public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
            => Size.Zero;

        protected override void SetupContainer() { }

        protected override void RemoveContainer() { }
    }
}