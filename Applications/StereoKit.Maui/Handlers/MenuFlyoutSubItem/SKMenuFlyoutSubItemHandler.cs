using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using PlatformView = StereoKit.UIX.Controls.MenuFlyoutSubItem;

namespace StereoKit.Maui.Handlers
{
    public partial class SKMenuFlyoutSubItemHandler : SKElementHandler<IMenuFlyoutSubItem, PlatformView>, ISKMenuFlyoutSubItemHandler
    {
        public static IPropertyMapper<IMenuFlyoutSubItem, ISKMenuFlyoutSubItemHandler> Mapper = new PropertyMapper<IMenuFlyoutSubItem, ISKMenuFlyoutSubItemHandler>(ElementMapper)
        {
            //[nameof(IMenuFlyoutSubItem.Text)] = MapText,
            //[nameof(IMenuFlyoutSubItem.Source)] = MapSource,
            //[nameof(IMenuFlyoutSubItem.IsEnabled)] = MapIsEnabled,
        };

        public static CommandMapper<IMenuFlyoutSubItem, ISKMenuFlyoutSubItemHandler> CommandMapper = new(ElementCommandMapper)
        {
            [nameof(ISKMenuFlyoutSubItemHandler.Add)] = MapAdd,
            [nameof(ISKMenuFlyoutSubItemHandler.Remove)] = MapRemove,
            [nameof(ISKMenuFlyoutSubItemHandler.Clear)] = MapClear,
            [nameof(ISKMenuFlyoutSubItemHandler.Insert)] = MapInsert,
        };

        public SKMenuFlyoutSubItemHandler() : this(Mapper, CommandMapper) { }

        public SKMenuFlyoutSubItemHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper) { }

        public static void MapAdd(ISKMenuFlyoutSubItemHandler handler, IMenuElement layout, object? arg)
        {
            if (arg is MenuFlyoutSubItemHandlerUpdate args)
                handler.Add(args.MenuElement);
        }

        public static void MapRemove(ISKMenuFlyoutSubItemHandler handler, IMenuElement layout, object? arg)
        {
            if (arg is MenuFlyoutSubItemHandlerUpdate args)
                handler.Remove(args.MenuElement);
        }

        public static void MapInsert(ISKMenuFlyoutSubItemHandler handler, IMenuElement layout, object? arg)
        {
            if (arg is MenuFlyoutSubItemHandlerUpdate args)
                handler.Insert(args.Index, args.MenuElement);
        }

        public static void MapClear(ISKMenuFlyoutSubItemHandler handler, IMenuElement layout, object? arg)
            => handler.Clear();

        IMenuFlyoutSubItem ISKMenuFlyoutSubItemHandler.VirtualView => VirtualView;

        PlatformView ISKMenuFlyoutSubItemHandler.PlatformView => PlatformView;

        private protected override void OnDisconnectHandler(object platformView)
        {
            base.OnDisconnectHandler(platformView);
            foreach (var item in VirtualView)
                item?.Handler?.DisconnectHandler();
        }
    }
}
