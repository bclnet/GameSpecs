#if WINDOWS
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
using StereoKit.Maui.LifecycleEvents;
using System;
using System.Threading.Tasks;

namespace StereoKit.Maui
{
    public abstract class MauiWinUISKApplication : MauiWinUIApplication
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
        }

        public virtual void SKThread()
        {
            var app = (ISKApplication)Application;
            SKInitialize(app);
            app.Initialize();
            SK.Run(OnStep, () => Log.Info("Done"));
        }

        static Pose demoSelectPose = new(new Vec3(0, 0, -0.6f), Quat.LookDir(-Vec3.Forward));

        public virtual void OnStep()
        {
            // Make a window for demo selection
            UI.WindowBegin("Demos", ref demoSelectPose, new Vec2(50 * U.cm, 0));
            UI.Label("Label");
            UI.Label("Label");
            UI.WindowEnd();


            var app = (ISKApplication)Application;
            app.OnStep();
        }

        protected void StartSKThread() => Task.Run(SKThread);

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);
            StartSKThread();
        }

        /*
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // Windows running on a different thread will "launch" the app again
            if (Application != null)
            {
                Services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunching>(del => del(this, args));
                Services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunched>(del => del(this, args));
                return;
            }

            IPlatformApplication.Current = this;
            var mauiApp = CreateMauiApp();

            var rootContext = new MauiContext(mauiApp.Services);

            var applicationContext = rootContext.MakeApplicationScope(this);

            Services = applicationContext.Services;

            //DeploymentManagerAutoInitializer.LogIfFailed(Services);

            Services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunching>(del => del(this, args));

            Application = Services.GetRequiredService<IApplication>();

            //this.SetApplicationHandler(Application, applicationContext);

            //this.CreatePlatformWindow(Application, args);

            Services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunched>(del => del(this, args));

            StartSKThread();
        }
        */
    }
}
#endif