using Microsoft.Maui;
using System;
using PlatformView = StereoKit.UIX.Controls.MauiRefreshView;

namespace StereoKit.Maui.Handlers
{
    public partial class SKRefreshViewHandler : SKViewHandler<IRefreshView, PlatformView>
	{
		protected override PlatformView CreatePlatformView() => throw new NotImplementedException();

		public static void MapIsRefreshing(ISKRefreshViewHandler handler, IRefreshView refreshView) { }
		public static void MapContent(ISKRefreshViewHandler handler, IRefreshView refreshView) { }
		public static void MapRefreshColor(ISKRefreshViewHandler handler, IRefreshView refreshView) { }
		public static void MapRefreshViewBackground(ISKRefreshViewHandler handler, IView view) { }
	}
}
