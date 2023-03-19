using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.Window;

namespace StereoKit.Maui.Handlers
{
    public partial interface ISKWindowHandler : IElementHandler
	{
		new IWindow VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}
