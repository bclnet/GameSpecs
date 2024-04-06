using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.MenuFlyoutSeparator;

namespace StereoKit.Maui.Handlers
{
    public partial class SKMenuFlyoutSeparatorHandler : SKElementHandler<IMenuFlyoutSeparator, PlatformView>, ISKMenuFlyoutSeparatorHandler
    {
        public static IPropertyMapper<IMenuFlyoutSeparator, ISKMenuFlyoutSeparatorHandler> Mapper = new PropertyMapper<IMenuFlyoutSeparator, ISKMenuFlyoutSeparatorHandler>(ElementMapper);

        public static CommandMapper<IMenuFlyoutSeparator, ISKMenuFlyoutSeparatorHandler> CommandMapper = new(ElementCommandMapper);

        public SKMenuFlyoutSeparatorHandler() : this(Mapper, CommandMapper) { }

        public SKMenuFlyoutSeparatorHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper) { }

        IMenuFlyoutSeparator ISKMenuFlyoutSeparatorHandler.VirtualView => VirtualView;

        PlatformView ISKMenuFlyoutSeparatorHandler.PlatformView => PlatformView;
    }
}
