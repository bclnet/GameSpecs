using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Views.ContentView;

namespace StereoKit.Maui.Handlers
{
    public partial interface ISKContentViewHandler : IViewHandler
    {
        new IContentView VirtualView { get; }
        new PlatformView PlatformView { get; }
    }
}