using Microsoft.Maui;
using PlatformView = StereoKit.Maui.Controls.ProgressBar;

namespace StereoKit.Maui.Handlers
{
    public partial interface ISKProgressBarHandler : IViewHandler
    {
        new IProgress VirtualView { get; }
        new PlatformView PlatformView { get; }
    }
}