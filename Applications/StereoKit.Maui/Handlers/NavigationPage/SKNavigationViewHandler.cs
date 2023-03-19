using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Views.Frame;

namespace StereoKit.Maui.Handlers
{
    public partial class SKNavigationViewHandler : ISKNavigationViewHandler
    {
        public static IPropertyMapper<IStackNavigationView, ISKNavigationViewHandler> Mapper = new PropertyMapper<IStackNavigationView, ISKNavigationViewHandler>(ViewMapper);

        public static CommandMapper<IStackNavigationView, ISKNavigationViewHandler> CommandMapper = new(ViewCommandMapper)
        {
            [nameof(IStackNavigation.RequestNavigation)] = RequestNavigation
        };

        public SKNavigationViewHandler() : base(Mapper, CommandMapper) { }

        public SKNavigationViewHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKNavigationViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        IStackNavigationView ISKNavigationViewHandler.VirtualView => VirtualView;

        PlatformView ISKNavigationViewHandler.PlatformView => PlatformView;
    }
}
