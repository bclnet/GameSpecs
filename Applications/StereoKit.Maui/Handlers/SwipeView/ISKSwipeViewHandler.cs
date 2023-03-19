using Microsoft.Maui;
using PlatformView = StereoKit.Maui.Controls.SwipeView;

namespace StereoKit.Maui.Handlers
{
	public partial interface ISKSwipeViewHandler : IViewHandler
	{
		new ISwipeView VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}
