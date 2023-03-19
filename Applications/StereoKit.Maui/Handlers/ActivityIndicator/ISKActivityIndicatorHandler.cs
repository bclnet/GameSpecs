using Microsoft.Maui;
using PlatformView = StereoKit.Maui.Controls.ActivityIndicator;

namespace StereoKit.Maui.Handlers
{
    public partial interface ISKActivityIndicatorHandler : IViewHandler
	{
		new IActivityIndicator VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}