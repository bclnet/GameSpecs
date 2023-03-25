using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.MenuFlyoutItem;

namespace StereoKit.Maui.Handlers
{
	public interface ISKMenuFlyoutItemHandler : IElementHandler
	{
		new IMenuFlyoutItem VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}
