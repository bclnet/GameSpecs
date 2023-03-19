using Microsoft.Maui;
using PlatformView = StereoKit.Maui.Controls.Window;

namespace StereoKit.Maui.Handlers
{
    public partial interface ISKWindowHandler : IElementHandler
	{
		new IWindow VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}
