using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.Switch;

namespace StereoKit.Maui.Handlers
{
	public partial interface ISKSwitchHandler : IViewHandler
	{
		new ISwitch VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}