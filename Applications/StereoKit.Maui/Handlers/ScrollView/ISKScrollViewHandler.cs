using Microsoft.Maui;
using PlatformView = StereoKit.Maui.Controls.ScrollViewer;

namespace StereoKit.Maui.Handlers
{
    public partial interface ISKScrollViewHandler : IViewHandler
    {
        new IScrollView VirtualView { get; }
        new PlatformView PlatformView { get; }
    }
}