#if __ANDROID__
using System;
using Android.App;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace StereoKit.Maui.Handlers
{
    public partial class SKApplicationHandler : SKElementHandler<IApplication, Application>
    {
        public static void MapTerminate(SKApplicationHandler handler, IApplication application, object? args)
        {
            var currentActivity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;

            if (currentActivity != null)
            {
                currentActivity.FinishAndRemoveTask();

                Environment.Exit(0);
            }
        }

        public static void MapOpenWindow(SKApplicationHandler handler, IApplication application, object? args)
        {
            handler.PlatformView?.RequestNewWindow(application, args as OpenWindowRequest);
        }

        public static void MapCloseWindow(SKApplicationHandler handler, IApplication application, object? args)
        {
            if (args is IWindow window)
            {
                if (window.Handler?.PlatformView is Activity activity)
                    activity.Finish();
            }
        }
    }
}
#endif