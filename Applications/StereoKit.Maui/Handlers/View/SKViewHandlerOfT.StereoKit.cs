using Microsoft.Maui.Graphics;

namespace StereoKit.Maui.Handlers
{
    public abstract partial class SKViewHandler<TVirtualView, TPlatformView>
    {
        public override void PlatformArrange(Rect rect) { }

        public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
            => Size.Zero;

        protected override void SetupContainer() { }

        protected override void RemoveContainer() { }
    }
}