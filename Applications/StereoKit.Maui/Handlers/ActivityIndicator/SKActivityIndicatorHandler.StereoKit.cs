using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using System;
using PlatformView = StereoKit.Maui.Controls.ActivityIndicator;

namespace StereoKit.Maui.Handlers
{
    public partial class SKActivityIndicatorHandler : ViewHandler<IActivityIndicator, PlatformView>
    {
        protected override PlatformView CreatePlatformView() => new();

        public static void MapIsRunning(ISKActivityIndicatorHandler handler, IActivityIndicator activityIndicator) { }
        public static void MapColor(ISKActivityIndicatorHandler handler, IActivityIndicator activityIndicator) { }
    }
}