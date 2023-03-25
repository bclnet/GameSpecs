using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.MauiShapeView;

namespace StereoKit.Maui.Handlers
{
    public partial interface ISKShapeViewHandler : IViewHandler
    {
        new IShapeView VirtualView { get; }
        new PlatformView PlatformView { get; }
    }
}