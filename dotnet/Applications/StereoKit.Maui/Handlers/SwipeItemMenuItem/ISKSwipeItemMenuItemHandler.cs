using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.SwipeItem;

namespace StereoKit.Maui.Handlers
{
	public partial interface ISKSwipeItemMenuItemHandler : IElementHandler
	{
		new ISwipeItemMenuItem VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}