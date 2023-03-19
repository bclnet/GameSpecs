using Microsoft.Maui;
using PlatformView = StereoKit.Maui.Controls.MauiPageControl;

namespace StereoKit.Maui.Handlers
{
	public partial interface ISKIndicatorViewHandler : IViewHandler
	{
		new IIndicatorView VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}