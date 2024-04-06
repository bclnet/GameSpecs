using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.ScrollViewer;

namespace StereoKit.Maui.Handlers
{
    public partial class SKScrollViewHandler : SKViewHandler<IScrollView, PlatformView>
    {
        protected override PlatformView CreatePlatformView() => new();

        public static void MapContent(IViewHandler handler, IScrollView scrollView) { }
        public static void MapHorizontalScrollBarVisibility(IViewHandler handler, IScrollView scrollView) { }
        public static void MapVerticalScrollBarVisibility(IViewHandler handler, IScrollView scrollView) { }
        public static void MapOrientation(IViewHandler handler, IScrollView scrollView) { }
        public static void MapContentSize(IViewHandler handler, IScrollView scrollView) { }
        public static void MapRequestScrollTo(ISKScrollViewHandler handler, IScrollView scrollView, object? args) { }
    }
}
