using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.ScrollViewer;

namespace StereoKit.Maui.Handlers
{
    public partial class SKScrollViewHandler : ISKScrollViewHandler
    {
        public static IPropertyMapper<IScrollView, ISKScrollViewHandler> Mapper = new PropertyMapper<IScrollView, ISKScrollViewHandler>(ViewMapper)
        {
            [nameof(IScrollView.Content)] = MapContent,
            [nameof(IScrollView.HorizontalScrollBarVisibility)] = MapHorizontalScrollBarVisibility,
            [nameof(IScrollView.VerticalScrollBarVisibility)] = MapVerticalScrollBarVisibility,
            [nameof(IScrollView.Orientation)] = MapOrientation,
        };

        public static CommandMapper<IScrollView, ISKScrollViewHandler> CommandMapper = new(ViewCommandMapper)
        {
            [nameof(IScrollView.RequestScrollTo)] = MapRequestScrollTo
        };

        public SKScrollViewHandler() : base(Mapper, CommandMapper) { }

        public SKScrollViewHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKScrollViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        IScrollView ISKScrollViewHandler.VirtualView => VirtualView;

        PlatformView ISKScrollViewHandler.PlatformView => PlatformView;
    }
}
