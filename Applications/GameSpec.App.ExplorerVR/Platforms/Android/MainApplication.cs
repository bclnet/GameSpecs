using Android.App;
using Android.Runtime;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using StereoKit;
using System;
using System.Threading.Tasks;

namespace GameSpec.App.Explorer
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership) { }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        static bool running = false;
        internal static void Run(IntPtr activityHandle)
        {
            if (running) return;
            running = true;

            var app = App.Instance;
            app.Settings.androidActivity = activityHandle;
            app.PlatformStartup().Wait();
            // Now pass execution over to StereoKit
            //Task.Run(() =>
            //{
            SK.Run(app.Step, () => Log.Info("Done"));
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
            //});
        }
    }
}