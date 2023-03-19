using Microsoft.Maui;
using PlatformView = StereoKit.Maui.Controls.Entry;

namespace StereoKit.Maui.Handlers
{
    public partial interface ISKEntryHandler : IViewHandler
    {
        new IEntry VirtualView { get; }
        new PlatformView PlatformView { get; }
    }
}