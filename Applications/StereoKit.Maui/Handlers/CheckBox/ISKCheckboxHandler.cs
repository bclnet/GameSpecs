using Microsoft.Maui;
using PlatformView = StereoKit.Maui.Controls.CheckBox;

namespace StereoKit.Maui.Handlers
{
    public partial interface ISKCheckBoxHandler : IViewHandler
	{
		new ICheckBox VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}