#if __ANDROID__
using System;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using StereoKit.Maui.LifecycleEvents;
using System.Threading.Tasks;
using Microsoft.Maui.Platform;

namespace StereoKit.Maui
{
	public abstract class MauiSKApplication : MauiApplication
	{
        delegate uint XR_xrConvertTimeToWin32PerformanceCounterKHR(ulong instance, long time, out long performanceCounter);
        static XR_xrConvertTimeToWin32PerformanceCounterKHR xrConvertTimeToWin32PerformanceCounterKHR;

        protected virtual void SKInitialize(ISKApplication app)
        {
            // Preload the StereoKit library for access to Time.Scale before initialization occurs.
            SK.PreLoadLibrary();
            //Time.Scale = 1;
            Log.Subscribe(app.OnLog);

            // Initialize StereoKit, and the app
            Backend.OpenXR.RequestExt("XR_KHR_win32_convert_performance_counter_time");
            if (!SK.Initialize(app.Settings)) System.Environment.Exit(1);
            if (Backend.XRType == BackendXRType.OpenXR && Backend.OpenXR.ExtEnabled("XR_KHR_win32_convert_performance_counter_time"))
            {
                xrConvertTimeToWin32PerformanceCounterKHR = Backend.OpenXR.GetFunction<XR_xrConvertTimeToWin32PerformanceCounterKHR>("xrConvertTimeToWin32PerformanceCounterKHR");
                if (xrConvertTimeToWin32PerformanceCounterKHR != null)
                {
                    xrConvertTimeToWin32PerformanceCounterKHR(Backend.OpenXR.Instance, Backend.OpenXR.Time, out long counter);
                    Log.Info($"XrTime: {counter}");
                }
            }
        }

        protected virtual void SKThread()
        {
            var app = (ISKApplication)Application;
            SKInitialize(app);
            app.Initialize();
            SK.Run(app.OnStep, () => Log.Info("Done"));
			Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
        }

        public void StartSKThread(IntPtr activityHandle)
        {
            var app = (ISKApplication)Application;
			var settings = app.Settings;
			settings.androidActivity = activityHandle;
			app.Settings = settings;
            Task.Run(SKThread);
        }

		protected MauiSKApplication(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership) { }
	}
}
#endif