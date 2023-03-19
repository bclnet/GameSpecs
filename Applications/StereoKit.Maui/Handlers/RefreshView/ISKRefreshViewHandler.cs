using Microsoft.Maui;
using PlatformView = StereoKit.Maui.Controls2.MauiRefreshView;

namespace StereoKit.Maui.Handlers
{
	public partial interface ISKRefreshViewHandler : IViewHandler
	{
		new IRefreshView VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}