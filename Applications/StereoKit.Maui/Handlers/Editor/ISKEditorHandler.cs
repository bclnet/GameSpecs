using Microsoft.Maui;
using PlatformView = StereoKit.Maui.Controls.Editor;

namespace StereoKit.Maui.Handlers
{
	public partial interface ISKEditorHandler : IViewHandler
	{
		new IEditor VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}