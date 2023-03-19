using Microsoft.Maui;
using PlatformView = StereoKit.Maui.Controls.MauiStepper;

namespace StereoKit.Maui.Handlers
{
    public partial interface ISKStepperHandler : IViewHandler
    {
        new IStepper VirtualView { get; }
        new PlatformView PlatformView { get; }
    }
}