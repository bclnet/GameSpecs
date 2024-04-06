using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.MauiRefreshView;

namespace StereoKit.Maui.Handlers
{
    public partial class SKRefreshViewHandler : ISKRefreshViewHandler
    {
        public static IPropertyMapper<IRefreshView, ISKRefreshViewHandler> Mapper = new PropertyMapper<IRefreshView, ISKRefreshViewHandler>(SKViewHandler.ViewMapper)
        {
            [nameof(IRefreshView.IsRefreshing)] = MapIsRefreshing,
            [nameof(IRefreshView.Content)] = MapContent,
            [nameof(IRefreshView.RefreshColor)] = MapRefreshColor,
            [nameof(IView.Background)] = MapBackground,
            [nameof(IView.IsEnabled)] = MapIsEnabled,
        };

        public static CommandMapper<IRefreshView, ISKRefreshViewHandler> CommandMapper = new(ViewCommandMapper);

        public SKRefreshViewHandler() : base(Mapper, CommandMapper) { }

        public SKRefreshViewHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKRefreshViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        IRefreshView ISKRefreshViewHandler.VirtualView => VirtualView;

        PlatformView ISKRefreshViewHandler.PlatformView => PlatformView;
    }
}
