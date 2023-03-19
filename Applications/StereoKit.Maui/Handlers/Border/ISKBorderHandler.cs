using Microsoft.Maui;
using PlatformView = StereoKit.Maui.Views.Border;

namespace StereoKit.Maui.Handlers
{
	public partial interface ISKBorderHandler : IViewHandler
	{
		new IBorderView VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}