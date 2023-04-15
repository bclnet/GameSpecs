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
	public abstract class MauiSKApplication : Application, IPlatformApplication
	{
        #region SK

        //delegate uint XR_xrConvertTimeToWin32PerformanceCounterKHR(ulong instance, long time, out long performanceCounter);
        //static XR_xrConvertTimeToWin32PerformanceCounterKHR xrConvertTimeToWin32PerformanceCounterKHR;

        protected virtual void SKInitialize(ISKApplication app)
        {
            // Preload the StereoKit library for access to Time.Scale before initialization occurs.
            SK.PreLoadLibrary();
            //Time.Scale = 1;
            Log.Subscribe(app.OnLog);

            // Initialize StereoKit, and the app
            //Backend.OpenXR.RequestExt("XR_KHR_win32_convert_performance_counter_time");
            if (!SK.Initialize(app.Settings)) System.Environment.Exit(1);
            //if (Backend.XRType == BackendXRType.OpenXR && Backend.OpenXR.ExtEnabled("XR_KHR_win32_convert_performance_counter_time"))
            //{
            //    xrConvertTimeToWin32PerformanceCounterKHR = Backend.OpenXR.GetFunction<XR_xrConvertTimeToWin32PerformanceCounterKHR>("xrConvertTimeToWin32PerformanceCounterKHR");
            //    if (xrConvertTimeToWin32PerformanceCounterKHR != null)
            //    {
            //        xrConvertTimeToWin32PerformanceCounterKHR(Backend.OpenXR.Instance, Backend.OpenXR.Time, out long counter);
            //        Log.Info($"XrTime: {counter}");
            //    }
            //}
        }

        protected virtual void SKThread()
        {
            var app = (ISKApplication)Application;
			Console.WriteLine($"2: {Handle}");
			var settings = app.Settings;
			settings.androidActivity = Handle;
			app.Settings = settings;
            SKInitialize(app);
            app.Initialize();
            SK.Run(app.OnStep, () => Log.Info("Done"));
			Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
        }

        #endregion

		protected MauiSKApplication(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership)
		{
			Current = this;
			IPlatformApplication.Current = this;
		}

		protected abstract MauiApp CreateMauiApp();

		public override void OnCreate()
		{
			RegisterActivityLifecycleCallbacks(new ActivityLifecycleCallbacks());

			var mauiApp = CreateMauiApp();

			var rootContext = new MauiContext(mauiApp.Services, this);

			var applicationContext = rootContext.MakeApplicationScope(this);

			Services = applicationContext.Services;

			Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnApplicationCreating>(del => del(this));

			Application = Services.GetRequiredService<IApplication>();

			this.SetApplicationHandler(Application, applicationContext);

			Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnApplicationCreate>(del => del(this));

			//SKThread();
            //Task.Run(SKThread);

			base.OnCreate();
		}

		public override void OnLowMemory()
		{
			Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnApplicationLowMemory>(del => del(this));

			base.OnLowMemory();
		}

		public override void OnTrimMemory(TrimMemory level)
		{
			Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnApplicationTrimMemory>(del => del(this, level));

			base.OnTrimMemory(level);
		}

		public override void OnConfigurationChanged(Configuration newConfig)
		{
			Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnApplicationConfigurationChanged>(del => del(this, newConfig));

			base.OnConfigurationChanged(newConfig);
		}

		public static MauiSKApplication Current { get; private set; } = null!;

		public IServiceProvider Services { get; protected set; } = null!;

		public IApplication Application { get; protected set; } = null!;

		public class ActivityLifecycleCallbacks : Java.Lang.Object, IActivityLifecycleCallbacks
		{
			public void OnActivityCreated(Activity activity, Bundle? savedInstanceState) =>
				Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnCreate>(del => del(activity, savedInstanceState));

			public void OnActivityStarted(Activity activity) =>
				Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnStart>(del => del(activity));

			public void OnActivityResumed(Activity activity) =>
				Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnResume>(del => del(activity));

			public void OnActivityPaused(Activity activity) =>
				Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnPause>(del => del(activity));

			public void OnActivityStopped(Activity activity) =>
				Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnStop>(del => del(activity));

			public void OnActivitySaveInstanceState(Activity activity, Bundle outState) =>
				Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnSaveInstanceState>(del => del(activity, outState));

			public void OnActivityDestroyed(Activity activity) =>
				Current.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnDestroy>(del => del(activity));
		}
	}
}
#endif