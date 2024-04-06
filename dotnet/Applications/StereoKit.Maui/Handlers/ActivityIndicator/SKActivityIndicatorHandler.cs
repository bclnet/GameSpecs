using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.ActivityIndicator;

namespace StereoKit.Maui.Handlers
{
    public partial class SKActivityIndicatorHandler : ISKActivityIndicatorHandler
    {
        public static IPropertyMapper<IActivityIndicator, ISKActivityIndicatorHandler> Mapper = new PropertyMapper<IActivityIndicator, ISKActivityIndicatorHandler>(SKViewHandler.ViewMapper)
        {
            [nameof(IActivityIndicator.Color)] = MapColor,
            [nameof(IActivityIndicator.IsRunning)] = MapIsRunning,
        };

        public static CommandMapper<IActivityIndicator, ISKActivityIndicatorHandler> CommandMapper = new(ViewCommandMapper);

        public SKActivityIndicatorHandler() : base(Mapper, CommandMapper) { }

        public SKActivityIndicatorHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKActivityIndicatorHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        IActivityIndicator ISKActivityIndicatorHandler.VirtualView => VirtualView;

        PlatformView ISKActivityIndicatorHandler.PlatformView => PlatformView;
    }
}