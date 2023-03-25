using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.ActivityIndicator;

namespace StereoKit.Maui.Handlers
{
    public partial class SKActivityIndicatorHandler : SKViewHandler<IActivityIndicator, PlatformView>
    {
        protected override PlatformView CreatePlatformView() => new();

        public static void MapIsRunning(ISKActivityIndicatorHandler handler, IActivityIndicator activityIndicator) { }
        public static void MapColor(ISKActivityIndicatorHandler handler, IActivityIndicator activityIndicator) { }
    }
}