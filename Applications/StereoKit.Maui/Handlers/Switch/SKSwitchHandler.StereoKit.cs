using Microsoft.Maui;
using System;

namespace StereoKit.Maui.Handlers
{
    public partial class SKSwitchHandler : SKViewHandler<ISwitch, object>
    {
        protected override object CreatePlatformView() => throw new NotImplementedException();

        public static void MapIsOn(ISKSwitchHandler handler, ISwitch view) { }
        public static void MapTrackColor(ISKSwitchHandler handler, ISwitch view) { }
        public static void MapThumbColor(ISKSwitchHandler handler, ISwitch view) { }
    }
}