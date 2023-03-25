using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.MauiRefreshView;

namespace StereoKit.Maui.Handlers
{
	public partial interface ISKRefreshViewHandler : IViewHandler
	{
		new IRefreshView VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}