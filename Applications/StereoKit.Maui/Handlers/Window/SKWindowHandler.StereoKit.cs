using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using StereoKit.Maui.Platform;
using System;
using PlatformView = StereoKit.UIX.Controls.Window;

namespace StereoKit.Maui.Handlers
{
    public partial class SKWindowHandler : ElementHandler<IWindow, PlatformView>
    {
        protected override PlatformView CreatePlatformElement()
        {
            var r = new PlatformView();
            r.Initialize();
            return r;
        }

        public static void MapTitle(ISKWindowHandler handler, IWindow window) { }
        public static void MapContent(ISKWindowHandler handler, IWindow window)
        {
            _ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

            var platformContent = window.Content.ToContainerView(handler.MauiContext);

            //handler.MauiContext.GetPlatformWindow().SetContent(platformContent);
            handler.PlatformView.SetContent(platformContent);

            //if (window.VisualDiagnosticsOverlay != null)
            //    window.VisualDiagnosticsOverlay.Initialize();
        }
        public static void MapX(ISKWindowHandler handler, IWindow view) => handler.PlatformView?.UpdateX(view);
        public static void MapY(ISKWindowHandler handler, IWindow view) => handler.PlatformView?.UpdateY(view);
        public static void MapWidth(ISKWindowHandler handler, IWindow view) => handler.PlatformView?.UpdateWidth(view);
        public static void MapHeight(ISKWindowHandler handler, IWindow view) => handler.PlatformView?.UpdateHeight(view);

        public static void MapRequestDisplayDensity(ISKWindowHandler handler, IWindow window, object? args) { }
    }
}