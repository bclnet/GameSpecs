using Microsoft.Maui;
using PlatformView = StereoKit.Maui.Views.DrawerView;

namespace StereoKit.Maui.Handlers
{
	public partial interface ISKFlyoutViewHandler : IViewHandler
	{
		new IFlyoutView VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}