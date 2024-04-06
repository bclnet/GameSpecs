#if __ANDROID__
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;

namespace StereoKit.Maui.Platform
{
	internal static partial class MauiContextExtensions
	{
		public static Android.App.Activity GetPlatformWindow(this IMauiContext mauiContext) =>
			mauiContext.Services.GetRequiredService<Android.App.Activity>();
    }
}
#endif