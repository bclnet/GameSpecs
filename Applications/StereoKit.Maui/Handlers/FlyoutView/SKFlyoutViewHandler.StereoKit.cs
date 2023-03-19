using Microsoft.Maui;
using PlatformView = StereoKit.Maui.Views.DrawerView;

namespace StereoKit.Maui.Handlers
{
    public partial class SKFlyoutViewHandler : SKViewHandler<IFlyoutView, PlatformView>
	{
		protected override PlatformView CreatePlatformView() => new();
	}
}
