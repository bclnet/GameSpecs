using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.MenuFlyoutItem;

namespace StereoKit.Maui.Handlers
{
    public partial class SKMenuFlyoutItemHandler : SKElementHandler<IMenuFlyoutItem, PlatformView>, ISKMenuFlyoutItemHandler
    {
        public static IPropertyMapper<IMenuFlyoutItem, ISKMenuFlyoutItemHandler> Mapper = new PropertyMapper<IMenuFlyoutItem, ISKMenuFlyoutItemHandler>(ElementMapper)
        {
            //[nameof(IMenuFlyoutSubItem.Text)] = MapText,
            //[nameof(IMenuElement.Source)] = MapSource,
            //[nameof(IMenuElement.IsEnabled)] = MapIsEnabled
        };

        public static CommandMapper<IMenuFlyoutItem, ISKMenuFlyoutItemHandler> CommandMapper = new(ElementCommandMapper);

        public SKMenuFlyoutItemHandler() : base(Mapper, CommandMapper) { }

        IMenuFlyoutItem ISKMenuFlyoutItemHandler.VirtualView => VirtualView;

        PlatformView ISKMenuFlyoutItemHandler.PlatformView => PlatformView;
    }
}
