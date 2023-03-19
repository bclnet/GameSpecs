using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using System;

namespace StereoKit.Maui.Handlers
{
    public partial class SKWindowHandler : ElementHandler<IWindow, object>
	{
		protected override object CreatePlatformElement() => throw new NotImplementedException();

		public static void MapTitle(IWindowHandler handler, IWindow window) { }

		public static void MapX(IWindowHandler handler, IWindow view) { }

		public static void MapY(IWindowHandler handler, IWindow view) { }

		public static void MapWidth(IWindowHandler handler, IWindow view) { }

		public static void MapHeight(IWindowHandler handler, IWindow view) { }

		public static void MapContent(IWindowHandler handler, IWindow window) { }

		public static void MapRequestDisplayDensity(IWindowHandler handler, IWindow window, object? args) { }
	}
}