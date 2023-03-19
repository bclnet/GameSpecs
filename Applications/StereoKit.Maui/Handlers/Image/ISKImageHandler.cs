using Microsoft.Maui;
using Microsoft.Maui.Platform;
using PlatformView = StereoKit.UIX.Controls.Image;

namespace StereoKit.Maui.Handlers
{
	public partial interface ISKImageHandler : IViewHandler
	{
		new IImage VirtualView { get; }
		ImageSourcePartLoader SourceLoader { get; }
		new PlatformView PlatformView { get; }
	}
}