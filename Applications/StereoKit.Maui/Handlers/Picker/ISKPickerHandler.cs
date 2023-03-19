using Microsoft.Maui;
using PlatformView = StereoKit.Maui.Controls.ComboBox;

namespace StereoKit.Maui.Handlers
{
	public partial interface ISKPickerHandler : IViewHandler
	{
		new IPicker VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}