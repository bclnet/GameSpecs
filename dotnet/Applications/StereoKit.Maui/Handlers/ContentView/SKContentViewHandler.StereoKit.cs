using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using StereoKit.Maui.Platform;
using System;
using PlatformView = StereoKit.UIX.Views.ContentView;

namespace StereoKit.Maui.Handlers
{
    public partial class SKContentViewHandler : SKViewHandler<IContentView, PlatformView>
    {
        protected override PlatformView CreatePlatformView() => new();

        public static void MapContent(ISKContentViewHandler handler, IContentView page) => UpdateContent(handler);

        static void UpdateContent(ISKContentViewHandler handler)
        {
            _ = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
            _ = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
            _ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

            handler.PlatformView.Children.Clear();
            if (handler.VirtualView.PresentedContent is IView view)
                handler.PlatformView.Children.Add(view.ToSKPlatform(handler.MauiContext));
        }
    }
}
