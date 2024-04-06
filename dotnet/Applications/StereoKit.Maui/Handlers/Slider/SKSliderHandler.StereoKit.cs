using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.Slider;

namespace StereoKit.Maui.Handlers
{
    public partial class SKSliderHandler : SKViewHandler<ISlider, PlatformView>
    {
        protected override PlatformView CreatePlatformView() => new();

        public static void MapMinimum(IViewHandler handler, ISlider slider) { }
        public static void MapMaximum(IViewHandler handler, ISlider slider) { }
        public static void MapValue(IViewHandler handler, ISlider slider) { }
        public static void MapMinimumTrackColor(IViewHandler handler, ISlider slider) { }
        public static void MapMaximumTrackColor(IViewHandler handler, ISlider slider) { }
        public static void MapThumbColor(IViewHandler handler, ISlider slider) { }
        public static void MapThumbImageSource(IViewHandler handler, ISlider slider) { }
    }
}