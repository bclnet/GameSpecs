using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.Switch;

namespace StereoKit.Maui.Handlers
{
    public partial class SKSwitchHandler : SKViewHandler<ISwitch, PlatformView>
    {
        protected override PlatformView CreatePlatformView() => new();

        public static void MapIsOn(ISKSwitchHandler handler, ISwitch view) { }
        public static void MapTrackColor(ISKSwitchHandler handler, ISwitch view) { }
        public static void MapThumbColor(ISKSwitchHandler handler, ISwitch view) { }
    }
}