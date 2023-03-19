using Microsoft.Maui;
using PlatformView = StereoKit.Maui.Controls.TimePicker;

namespace StereoKit.Maui.Handlers
{
    public partial interface ISKTimePickerHandler : IViewHandler
	{
		new ITimePicker VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}