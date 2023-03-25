using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.MenuFlyoutSeparator;

namespace StereoKit.Maui.Handlers
{
	public interface ISKMenuFlyoutSeparatorHandler : IElementHandler
	{
		new PlatformView PlatformView { get; }
		new IMenuFlyoutSeparator VirtualView { get; }
	}
}
