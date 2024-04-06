//#if WINDOWS
//using Microsoft.Maui;
//using Microsoft.Maui.Handlers;
//using Microsoft.Maui.LifecycleEvents;
//using Microsoft.Maui.Platform;
//using StereoKit.Maui.LifecycleEvents;

//namespace StereoKit.Maui.Platform
//{
//    public static class SKApplicationExtensions
//    {
//        public static void CreateSKPlatformWindow(this Microsoft.UI.Xaml.Application platformApplication, IApplication application, Microsoft.UI.Xaml.LaunchActivatedEventArgs? args) =>
//            platformApplication.CreateSKPlatformWindow(application, new OpenWindowRequest(LaunchArgs: args));

//        public static void CreateSKPlatformWindow(this Microsoft.UI.Xaml.Application platformApplication, IApplication application, OpenWindowRequest? args)
//        {
//            if (application.Handler?.MauiContext is not IMauiContext applicationContext)
//                return;

//            var winuiWndow = new MauiWinUISKWindow();

//            var mauiContext = applicationContext!.MakeWindowScope(winuiWndow, out var windowScope);

//            //applicationContext.Services.InvokeLifecycleEvents<WindowsLifecycle.OnMauiContextCreated>(del => del(mauiContext));

//            var activationState = args?.State is not null
//                ? new ActivationState(mauiContext, args.State)
//                : new ActivationState(mauiContext, args?.LaunchArgs);

//            var window = application.CreateWindow(activationState);

//            winuiWndow.SetWindowHandler(window, mauiContext);

//            applicationContext.Services.InvokeLifecycleEvents<WindowsLifecycle.OnWindowCreated>(del => del(winuiWndow));

//            winuiWndow.Activate();
//        }
//    }
//}
//#endif