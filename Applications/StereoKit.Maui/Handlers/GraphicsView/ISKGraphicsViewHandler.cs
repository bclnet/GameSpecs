using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Views.PlatformTouchGraphicsView;

namespace StereoKit.Maui.Handlers
{
	public partial interface ISKGraphicsViewHandler : IViewHandler
	{
		new IGraphicsView VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}