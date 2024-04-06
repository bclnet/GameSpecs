using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Views.ContentView;

namespace StereoKit.Maui.Handlers
{
    public partial class SKSwipeItemViewHandler : SKViewHandler<ISwipeItemView, PlatformView>, ISKSwipeItemViewHandler
	{
		protected override PlatformView CreatePlatformView() => new();

		public static void MapContent(ISKSwipeItemViewHandler handler, ISwipeItemView page) { }
		public static void MapVisibility(ISKSwipeItemViewHandler handler, ISwipeItemView view) { }
	}
}
