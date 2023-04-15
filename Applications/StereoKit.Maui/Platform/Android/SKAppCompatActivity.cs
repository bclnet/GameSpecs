#if __ANDROID__
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using Java.Lang;
using StereoKit;
using System;

namespace StereoKit.Maui
{
	public partial class SKAppCompatActivity : AppCompatActivity, ISurfaceHolderCallback2
	{
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

            // start sk-thread
            var app = (MauiSKApplication)Application;
            app.StartSKThread(Handle);
        }

#pragma warning disable CA1416 // Validate platform compatibility
        //public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        //{
        //    Microsoft.Maui.ApplicationModel.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        //    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        //}
#pragma warning restore CA1416 // Validate platform compatibility

        protected override void OnDestroy()
        {
            SK.Quit();
            base.OnDestroy();
        }

        // Events related to surface state changes
        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height) => SK.SetWindow(holder.Surface.Handle);
        public void SurfaceCreated(ISurfaceHolder holder) => SK.SetWindow(holder.Surface.Handle);
        public void SurfaceDestroyed(ISurfaceHolder holder) => SK.SetWindow(IntPtr.Zero);
        public void SurfaceRedrawNeeded(ISurfaceHolder holder) { }
	}
}
#endif