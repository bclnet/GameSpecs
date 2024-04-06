#if WINDOWS
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using StereoKit.UIX.Controls;

namespace StereoKit.Maui.Handlers
{
    public partial class SKApplicationHandler : SKElementHandler<IApplication, Microsoft.UI.Xaml.Application>
    {
        public static void MapTerminate(SKApplicationHandler handler, IApplication application, object? args)
        {
            handler.PlatformView.Exit();
        }

        public static void MapOpenWindow(SKApplicationHandler handler, IApplication application, object? args)
        {
            handler.PlatformView?.CreatePlatformWindow(application, args as OpenWindowRequest);
        }

        public static void MapCloseWindow(SKApplicationHandler handler, IApplication application, object? args)
        {
            if (args is IWindow window)
            {
                (window.Handler?.PlatformView as Window)?.Close();
            }
        }
    }
}
#endif