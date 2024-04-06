using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Views.ContentView;

namespace StereoKit.Maui.Handlers
{
	public interface ISKSwipeItemViewHandler : IViewHandler
	{
		new ISwipeItemView VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}