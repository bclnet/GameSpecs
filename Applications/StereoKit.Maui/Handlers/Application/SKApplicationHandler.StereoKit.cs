using PlatformView = System.Object;

namespace Microsoft.Maui.Handlers
{
    public partial class SKApplicationHandler : ElementHandler<IApplication, PlatformView>
	{
		protected override PlatformView CreatePlatformElement() => new();

		public static void MapTerminate(SKApplicationHandler handler, IApplication application, object? args) { }
		public static void MapOpenWindow(SKApplicationHandler handler, IApplication application, object? args) { }
		public static void MapCloseWindow(SKApplicationHandler handler, IApplication application, object? args) { }
	}
}