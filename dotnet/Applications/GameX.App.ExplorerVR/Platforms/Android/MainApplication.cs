using Android.App;
using Android.Runtime;
using Microsoft.Maui.Hosting;
using StereoKit.Maui;
using System;

namespace GameX.App.Explorer
{
    [Application]
    public class MainApplication : MauiSKApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership) { }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}