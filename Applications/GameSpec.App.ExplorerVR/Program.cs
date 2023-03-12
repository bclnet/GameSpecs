using System;
using StereoKit;
namespace GameSpec.App.Explorer
{
    class Program
    {
        delegate uint XR_xrConvertTimeToWin32PerformanceCounterKHR(ulong instance, long time, out long performanceCounter);
        static XR_xrConvertTimeToWin32PerformanceCounterKHR xrConvertTimeToWin32PerformanceCounterKHR;

        static void Main(string[] args)
        {
            // If the app has a constructor that takes a string array, then we'll use that, and pass the command line arguments into it on creation
            var appType = typeof(App);
            var app = appType.GetConstructor(new[] { typeof(string[]) }) != null
                ? (App)Activator.CreateInstance(appType, new[] { args })
                : (App)Activator.CreateInstance(appType);
            if (app == null) throw new Exception("StereoKit loader couldn't construct an instance of the App!");

            // Initialize StereoKit, and the app
            Backend.OpenXR.RequestExt("XR_KHR_win32_convert_performance_counter_time");
            if (!SK.Initialize(app.Settings)) Environment.Exit(1);
            if (Backend.XRType == BackendXRType.OpenXR && Backend.OpenXR.ExtEnabled("XR_KHR_win32_convert_performance_counter_time"))
            {
                xrConvertTimeToWin32PerformanceCounterKHR = Backend.OpenXR.GetFunction<XR_xrConvertTimeToWin32PerformanceCounterKHR>("xrConvertTimeToWin32PerformanceCounterKHR");
                if (xrConvertTimeToWin32PerformanceCounterKHR != null)
                {
                    xrConvertTimeToWin32PerformanceCounterKHR(Backend.OpenXR.Instance, Backend.OpenXR.Time, out long counter);
                    Log.Info($"XrTime: {counter}");
                }
            }

            app.Initialize(args);

            // Now pass execution over to StereoKit
            SK.Run(app.Step, () => Log.Info("Done"));
        }
    }
}