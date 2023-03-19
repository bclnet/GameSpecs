using Microsoft.Maui;
using System;
using PlatformView = StereoKit.UIX.Views.ContentView;

namespace StereoKit.Maui.Handlers
{
    public partial class SKContentViewHandler : SKViewHandler<IContentView, PlatformView>
    {
        protected override PlatformView CreatePlatformView() => new();

        public static void MapContent(ISKContentViewHandler handler, IContentView page) { }
    }
}
