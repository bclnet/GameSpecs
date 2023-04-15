#if __ANDROID__
using Android.OS;
using AndroidX.AppCompat.App;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Microsoft.Maui;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
using Java.Lang;
using StereoKit;
using System;

namespace StereoKit.Maui
{
	public partial class MauiSKAppCompatActivity : AppCompatActivity, ISurfaceHolderCallback2
	{
        Android.Views.View surface;

#pragma warning disable CA1416 // Validate platform compatibility
        //public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        //{
        //    Microsoft.Maui.ApplicationModel.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        //    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        //}
#pragma warning restore CA1416 // Validate platform compatibility

        // Events related to surface state changes
        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height) => SK.SetWindow(holder.Surface.Handle);
        public void SurfaceCreated(ISurfaceHolder holder) => SK.SetWindow(holder.Surface.Handle);
        public void SurfaceDestroyed(ISurfaceHolder holder) => SK.SetWindow(IntPtr.Zero);
        public void SurfaceRedrawNeeded(ISurfaceHolder holder) { }

		// Override this if you want to handle the default Android behavior of restoring fragments on an application restart
		protected virtual bool AllowFragmentRestore => false;

		protected override void OnCreate(Bundle? savedInstanceState)
		{
            JavaSystem.LoadLibrary("openxr_loader");
            JavaSystem.LoadLibrary("StereoKitC");

            // Set up a surface for StereoKit to draw on
            Window.TakeSurface(this);
            Window.SetFormat(Format.Unknown);
            surface = new View(this);
            SetContentView(surface);
            surface.RequestFocus();

			if (!AllowFragmentRestore)
			{
				// Remove the automatically persisted fragment structure; we don't need them
				// because we're rebuilding everything from scratch. This saves a bit of memory
				// and prevents loading errors from child fragment managers
				savedInstanceState?.Remove("android:support:fragments");
				savedInstanceState?.Remove("androidx.lifecycle.BundlableSavedStateRegistry.key");
			}

			// If the theme has the maui_splash attribute, change the theme
			if (Theme.TryResolveAttribute(Resource.Attribute.maui_splash))
			{
				SetTheme(Resource.Style.Maui_MainTheme_NoActionBar);
			}

			base.OnCreate(savedInstanceState);

			this.CreatePlatformWindow(MauiApplication.Current.Application, savedInstanceState);

            Microsoft.Maui.ApplicationModel.Platform.Init(this, savedInstanceState);

			Console.WriteLine($"1: {Handle}");
		}
	}
}
#endif