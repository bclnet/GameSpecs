using Microsoft.Maui;
using PlatformView = System.Object;

namespace StereoKit.Maui.Handlers
{
    public partial class SKApplicationHandler : SKElementHandler<IApplication, PlatformView>
	{
		protected override PlatformView CreatePlatformElement() => new();

		public static void MapTerminate(SKApplicationHandler handler, IApplication application, object? args) { }
		public static void MapOpenWindow(SKApplicationHandler handler, IApplication application, object? args) { }
		public static void MapCloseWindow(SKApplicationHandler handler, IApplication application, object? args) { }
	}
}