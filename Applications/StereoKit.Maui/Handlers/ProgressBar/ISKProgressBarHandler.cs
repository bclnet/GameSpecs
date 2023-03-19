using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.ProgressBar;

namespace StereoKit.Maui.Handlers
{
    public partial interface ISKProgressBarHandler : IViewHandler
    {
        new IProgress VirtualView { get; }
        new PlatformView PlatformView { get; }
    }
}