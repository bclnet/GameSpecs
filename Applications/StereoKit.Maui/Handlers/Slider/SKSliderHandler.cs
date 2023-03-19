using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.Slider;

namespace StereoKit.Maui.Handlers
{
    public partial class SKSliderHandler : ISKSliderHandler
    {
        public static IPropertyMapper<ISlider, ISKSliderHandler> Mapper = new PropertyMapper<ISlider, ISKSliderHandler>(SKViewHandler.ViewMapper)
        {
            [nameof(ISlider.Maximum)] = MapMaximum,
            [nameof(ISlider.MaximumTrackColor)] = MapMaximumTrackColor,
            [nameof(ISlider.Minimum)] = MapMinimum,
            [nameof(ISlider.MinimumTrackColor)] = MapMinimumTrackColor,
            [nameof(ISlider.ThumbColor)] = MapThumbColor,
            [nameof(ISlider.ThumbImageSource)] = MapThumbImageSource,
            [nameof(ISlider.Value)] = MapValue,
        };

        public static CommandMapper<ISlider, ISKSliderHandler> CommandMapper = new(ViewCommandMapper);

        public SKSliderHandler() : base(Mapper) { }

        public SKSliderHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKSliderHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        ISlider ISKSliderHandler.VirtualView => VirtualView;

        PlatformView ISKSliderHandler.PlatformView => PlatformView;
    }
}