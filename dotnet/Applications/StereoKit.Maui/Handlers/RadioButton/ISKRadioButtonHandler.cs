using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.RadioButton;

namespace StereoKit.Maui.Handlers
{
	public partial interface ISKRadioButtonHandler : IViewHandler
	{
		new IRadioButton VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}