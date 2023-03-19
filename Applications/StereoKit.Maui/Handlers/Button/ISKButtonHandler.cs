using Microsoft.Maui;
using Microsoft.Maui.Platform;
using PlatformView = StereoKit.UIX.Controls.Button;

namespace StereoKit.Maui.Handlers
{
	public partial interface ISKButtonHandler : IViewHandler
	{
		new IButton VirtualView { get; }
		new PlatformView PlatformView { get; }
		ImageSourcePartLoader ImageSourceLoader { get; }
	}
}
