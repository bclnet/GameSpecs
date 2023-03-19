using Microsoft.Maui;
using PlatformView = StereoKit.Maui.Controls.Toolbar;

namespace StereoKit.Maui.Handlers
{
    public partial class SKToolbarHandler : SKElementHandler<IToolbar, PlatformView>
    {
        protected override PlatformView CreatePlatformElement() => new();

        public static void MapTitle(ISKToolbarHandler arg1, IToolbar arg2) { }
    }
}