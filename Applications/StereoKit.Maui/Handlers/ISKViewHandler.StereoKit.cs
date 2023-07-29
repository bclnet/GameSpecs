using Microsoft.Maui;
using NView = StereoKit.UIX.Controls.View;

namespace StereoKit.Maui
{
    public interface ISKPlatformViewHandler : IViewHandler
	{
		new NView? PlatformView { get; }

		new NView? ContainerView { get; }
	}
}