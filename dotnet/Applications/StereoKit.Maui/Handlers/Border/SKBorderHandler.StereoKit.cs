using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Views.Border;

namespace StereoKit.Maui.Handlers
{
    public partial class SKBorderHandler : SKViewHandler<IBorderView, PlatformView>
    {
        protected override PlatformView CreatePlatformView() => new();

        public static void MapContent(ISKBorderHandler handler, IBorderView border) { }
    }
}
