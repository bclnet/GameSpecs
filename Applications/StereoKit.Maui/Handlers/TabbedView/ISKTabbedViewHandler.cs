using Microsoft.Maui;

namespace StereoKit.Maui.Handlers
{
    public partial interface ISKTabbedViewHandler : IViewHandler
	{
		new ITabbedView VirtualView { get; }
	}
}
