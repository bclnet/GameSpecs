using Microsoft.Maui;
using PlatformView = StereoKit.Maui.Views.MauiPageControl;

namespace StereoKit.Maui.Handlers
{
	public partial interface ISKIndicatorViewHandler : IViewHandler
	{
		new IIndicatorView VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}