using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.Editor;

namespace StereoKit.Maui.Handlers
{
	public partial interface ISKEditorHandler : IViewHandler
	{
		new IEditor VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}