using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using PlatformView = StereoKit.Maui.Controls.Window;

namespace StereoKit.Maui.Handlers
{
    public partial class SKWindowHandler : ElementHandler<IWindow, PlatformView>
	{
		protected override PlatformView CreatePlatformElement() => new();

		public static void MapTitle(ISKWindowHandler handler, IWindow window) { }
		public static void MapX(ISKWindowHandler handler, IWindow view) { }
		public static void MapY(ISKWindowHandler handler, IWindow view) { }
		public static void MapWidth(ISKWindowHandler handler, IWindow view) { }
		public static void MapHeight(ISKWindowHandler handler, IWindow view) { }
		public static void MapContent(ISKWindowHandler handler, IWindow window) { }
		public static void MapRequestDisplayDensity(ISKWindowHandler handler, IWindow window, object? args) { }
	}
}