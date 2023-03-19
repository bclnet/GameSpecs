using Microsoft.Maui;
using PlatformView = StereoKit.Maui.Controls.Button;

namespace StereoKit.Maui.Handlers
{
	public interface ISKImageButtonHandler : ISKImageHandler
	{
		new IImageButton VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}