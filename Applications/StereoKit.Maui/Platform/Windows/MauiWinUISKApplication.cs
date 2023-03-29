using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using StereoKit.Maui;
using System;

namespace StereoKit.Maui
{
    public abstract class MauiWinUISKApplication : WinUIApplication, IPlatformApplication
    {
        protected abstract MauiApp CreateMauiApp();

        //protected override void OnLaunched(UI.Xaml.LaunchActivatedEventArgs args)
        //{
        //    // Windows running on a different thread will "launch" the app again
        //    if (Application != null)
        //    {
        //        Services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunching>(del => del(this, args));
        //        Services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunched>(del => del(this, args));
        //        return;
        //    }

        //    IPlatformApplication.Current = this;
        //    var mauiApp = CreateMauiApp();

        //    var rootContext = new MauiContext(mauiApp.Services);

        //    var applicationContext = rootContext.MakeApplicationScope(this);

        //    Services = applicationContext.Services;

        //    DeploymentManagerAutoInitializer.LogIfFailed(Services);

        //    Services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunching>(del => del(this, args));

        //    Application = Services.GetRequiredService<IApplication>();

        //    this.SetApplicationHandler(Application, applicationContext);

        //    this.CreatePlatformWindow(Application, args);

        //    Services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunched>(del => del(this, args));
        //}

        //public static new MauiSKApplication Current => (MauiSKApplication)Application.Current;

        public object LaunchActivatedEventArgs { get; protected set; } = null!;

        public IServiceProvider Services { get; protected set; } = null!;

        public IApplication Application { get; protected set; } = null!;
    }
}