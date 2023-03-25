using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.MauiPageControl;

namespace StereoKit.Maui.Handlers
{
    public partial class SKIndicatorViewHandler : ISKIndicatorViewHandler
    {
        public static IPropertyMapper<IIndicatorView, ISKIndicatorViewHandler> Mapper = new PropertyMapper<IIndicatorView, ISKIndicatorViewHandler>(ViewMapper)
        {
            [nameof(IIndicatorView.Count)] = MapCount,
            [nameof(IIndicatorView.Position)] = MapPosition,
            [nameof(IIndicatorView.HideSingle)] = MapHideSingle,
            [nameof(IIndicatorView.MaximumVisible)] = MapMaximumVisible,
            [nameof(IIndicatorView.IndicatorSize)] = MapIndicatorSize,
            [nameof(IIndicatorView.IndicatorColor)] = MapIndicatorColor,
            [nameof(IIndicatorView.SelectedIndicatorColor)] = MapSelectedIndicatorColor,
            [nameof(IIndicatorView.IndicatorsShape)] = MapIndicatorShape
        };

        public static CommandMapper<IIndicatorView, ISKIndicatorViewHandler> CommandMapper = new(ViewCommandMapper);

        public SKIndicatorViewHandler() : base(Mapper) { }

        public SKIndicatorViewHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKIndicatorViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        IIndicatorView ISKIndicatorViewHandler.VirtualView => VirtualView;

        PlatformView ISKIndicatorViewHandler.PlatformView => PlatformView;
    }
}
