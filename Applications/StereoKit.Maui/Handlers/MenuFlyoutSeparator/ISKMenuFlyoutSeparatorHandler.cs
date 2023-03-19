#if MENU2
using Microsoft.Maui;
using PlatformView = StereoKit.Maui.Controls.MenuFlyoutSeparator;

namespace StereoKit.Maui.Handlers
{
	public interface ISKMenuFlyoutSeparatorHandler : IElementHandler
	{
		new PlatformView PlatformView { get; }
		new IMenuFlyoutSeparator VirtualView { get; }
	}
}
#endif