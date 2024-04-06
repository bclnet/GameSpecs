using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.Slider;

namespace StereoKit.Maui.Handlers
{
	public partial interface ISKSliderHandler : IViewHandler
	{
		new ISlider VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}