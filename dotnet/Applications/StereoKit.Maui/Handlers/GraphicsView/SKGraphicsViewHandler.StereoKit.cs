using Microsoft.Maui;
using System;
using PlatformView = StereoKit.UIX.Views.PlatformTouchGraphicsView;

namespace StereoKit.Maui.Handlers
{
    public partial class SKGraphicsViewHandler : SKViewHandler<IGraphicsView, PlatformView>
	{
		protected override PlatformView CreatePlatformView() => new();

		public static void MapDrawable(ISKGraphicsViewHandler handler, IGraphicsView graphicsView) { }
		public static void MapFlowDirection(ISKGraphicsViewHandler handler, IGraphicsView graphicsView) { }
		public static void MapInvalidate(ISKGraphicsViewHandler handler, IGraphicsView graphicsView, object? arg) { }
	}
}