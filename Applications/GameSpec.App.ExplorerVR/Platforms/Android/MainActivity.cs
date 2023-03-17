using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using Java.Lang;
using Microsoft.Maui;
using StereoKit;
using System;
using System.Threading.Tasks;

namespace GameSpec.App.Explorer
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, Exported = true)]
    [IntentFilter(new[] { Intent.ActionMain }, Categories = new[] { "org.khronos.openxr.intent.category.IMMERSIVE_HMD", "com.oculus.intent.category.VR", Intent.CategoryLauncher })]
    public class MainActivity : AppCompatActivity, ISurfaceHolderCallback2 //: MauiAppCompatActivity
    {
        App app;
        Android.Views.View surface;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            JavaSystem.LoadLibrary("openxr_loader");
            JavaSystem.LoadLibrary("StereoKitC");

            // Set up a surface for StereoKit to draw on
            Window.TakeSurface(this);
            Window.SetFormat(Format.Unknown);
            surface = new View(this);
            SetContentView(surface);
            surface.RequestFocus();

            base.OnCreate(savedInstanceState);
            Microsoft.Maui.ApplicationModel.Platform.Init(this, savedInstanceState);

            Run(Handle);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Microsoft.Maui.ApplicationModel.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnDestroy()
        {
            SK.Quit();
            base.OnDestroy();
        }

        static bool running = false;
        void Run(IntPtr activityHandle)
        {
            if (running) return;
            running = true;

            Task.Run(() =>
            {
                var app = App.Instance;
                app.Settings.androidActivity = activityHandle;
                app.PlatformStartup();
                // Now pass execution over to StereoKit
                SK.Run(app.Step, () => Log.Info("Done"));
                Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
            });
        }

        // Events related to surface state changes
        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height) => SK.SetWindow(holder.Surface.Handle);
        public void SurfaceCreated(ISurfaceHolder holder) => SK.SetWindow(holder.Surface.Handle);
        public void SurfaceDestroyed(ISurfaceHolder holder) => SK.SetWindow(IntPtr.Zero);
        public void SurfaceRedrawNeeded(ISurfaceHolder holder) { }
    }
}