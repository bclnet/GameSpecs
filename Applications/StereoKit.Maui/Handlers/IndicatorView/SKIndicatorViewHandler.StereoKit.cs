using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.MauiPageControl;

namespace StereoKit.Maui.Handlers
{
    public partial class SKIndicatorViewHandler : SKViewHandler<IIndicatorView, PlatformView>
	{
		protected override PlatformView CreatePlatformView() => new();

		public static void MapCount(ISKIndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapPosition(ISKIndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapHideSingle(ISKIndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapMaximumVisible(ISKIndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapIndicatorSize(ISKIndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapIndicatorColor(ISKIndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapSelectedIndicatorColor(ISKIndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapIndicatorShape(ISKIndicatorViewHandler handler, IIndicatorView indicator) { }
	}
}
