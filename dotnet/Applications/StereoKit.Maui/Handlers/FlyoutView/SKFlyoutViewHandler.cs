using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Views.DrawerView;

namespace StereoKit.Maui.Handlers
{
    public partial class SKFlyoutViewHandler : ISKFlyoutViewHandler
    {
        public static IPropertyMapper<IFlyoutView, ISKFlyoutViewHandler> Mapper = new PropertyMapper<IFlyoutView, ISKFlyoutViewHandler>(SKViewHandler.ViewMapper)
        {
            //[nameof(IFlyoutView.Flyout)] = MapFlyout,
            //[nameof(IFlyoutView.Detail)] = MapDetail,
            //[nameof(IFlyoutView.IsPresented)] = MapIsPresented,
            //[nameof(IFlyoutView.FlyoutBehavior)] = MapFlyoutBehavior,
            //[nameof(IFlyoutView.FlyoutWidth)] = MapFlyoutWidth,
            //[nameof(IFlyoutView.IsGestureEnabled)] = MapIsGestureEnabled,
            //[nameof(IToolbarElement.Toolbar)] = MapToolbar,
        };

        public static CommandMapper<IFlyoutView, ISKFlyoutViewHandler> CommandMapper = new(ViewCommandMapper);

        public SKFlyoutViewHandler() : base(Mapper) { }

        public SKFlyoutViewHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKFlyoutViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        IFlyoutView ISKFlyoutViewHandler.VirtualView => VirtualView;

        PlatformView ISKFlyoutViewHandler.PlatformView => PlatformView;
    }
}
