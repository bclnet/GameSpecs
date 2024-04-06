using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.SwipeView;

namespace StereoKit.Maui.Handlers
{
    public partial class SKSwipeViewHandler : ISKSwipeViewHandler
    {
        public static IPropertyMapper<ISwipeView, ISKSwipeViewHandler> Mapper = new PropertyMapper<ISwipeView, ISKSwipeViewHandler>(SKViewHandler.ViewMapper)
        {
            [nameof(IContentView.Content)] = MapContent,
            [nameof(ISwipeView.SwipeTransitionMode)] = MapSwipeTransitionMode,
            [nameof(ISwipeView.LeftItems)] = MapLeftItems,
            [nameof(ISwipeView.TopItems)] = MapTopItems,
            [nameof(ISwipeView.RightItems)] = MapRightItems,
            [nameof(ISwipeView.BottomItems)] = MapBottomItems,
        };

        public static CommandMapper<ISwipeView, ISKSwipeViewHandler> CommandMapper = new(ViewCommandMapper)
        {
            [nameof(ISwipeView.RequestOpen)] = MapRequestOpen,
            [nameof(ISwipeView.RequestClose)] = MapRequestClose,
        };

        public SKSwipeViewHandler() : base(Mapper, CommandMapper) { }

        protected SKSwipeViewHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        protected SKSwipeViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        ISwipeView ISKSwipeViewHandler.VirtualView => VirtualView;

        PlatformView ISKSwipeViewHandler.PlatformView => PlatformView;
    }
}
