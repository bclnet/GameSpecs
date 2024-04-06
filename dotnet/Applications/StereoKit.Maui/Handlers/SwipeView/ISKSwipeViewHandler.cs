using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.SwipeView;

namespace StereoKit.Maui.Handlers
{
	public partial interface ISKSwipeViewHandler : IViewHandler
	{
		new ISwipeView VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}
