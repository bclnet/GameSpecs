using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.Toolbar;

namespace StereoKit.Maui.Handlers
{
    public partial class SKToolbarHandler : ISKToolbarHandler
    {
        public static IPropertyMapper<IToolbar, ISKToolbarHandler> Mapper = new PropertyMapper<IToolbar, ISKToolbarHandler>(ElementMapper)
        {
            [nameof(IToolbar.Title)] = MapTitle,
        };

        public static CommandMapper<IToolbar, ISKToolbarHandler> CommandMapper = new();

        public SKToolbarHandler() : base(Mapper, CommandMapper) { }

        public SKToolbarHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKToolbarHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        IToolbar ISKToolbarHandler.VirtualView => VirtualView;
        PlatformView ISKToolbarHandler.PlatformView => PlatformView;
    }
}
