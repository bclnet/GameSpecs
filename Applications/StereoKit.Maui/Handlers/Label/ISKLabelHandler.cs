using Microsoft.Maui;
using PlatformView = StereoKit.Maui.Controls.Label;

namespace StereoKit.Maui.Handlers
{
	public partial interface ISKLabelHandler : IViewHandler
	{
		new ILabel VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}