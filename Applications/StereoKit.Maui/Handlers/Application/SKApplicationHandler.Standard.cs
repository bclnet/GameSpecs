#if !__ANDROID__ && !WINDOWS
using System;
using Microsoft.Maui;

namespace StereoKit.Maui.Handlers
{
	public partial class SKApplicationHandler : SKElementHandler<IApplication, object>
	{
		protected override object CreatePlatformElement() => throw new NotImplementedException();

		public static void MapTerminate(SKApplicationHandler handler, IApplication application, object? args) { }
		public static void MapOpenWindow(SKApplicationHandler handler, IApplication application, object? args) { }
		public static void MapCloseWindow(SKApplicationHandler handler, IApplication application, object? args) { }
	}
}
#endif