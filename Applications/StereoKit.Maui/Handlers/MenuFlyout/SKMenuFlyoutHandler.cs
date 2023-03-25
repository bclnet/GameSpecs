using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using System.Collections.Generic;
using PlatformView = StereoKit.UIX.Controls.MenuFlyout;

namespace StereoKit.Maui.Handlers
{
    public partial class SKMenuFlyoutHandler : ISKMenuFlyoutHandler
    {
        public static IPropertyMapper<IMenuFlyout, ISKMenuFlyoutHandler> Mapper = new PropertyMapper<IMenuFlyout, ISKMenuFlyoutHandler>(ElementMapper);

        public static CommandMapper<IMenuFlyout, ISKMenuFlyoutHandler> CommandMapper = new(ElementCommandMapper)
        {
            [nameof(ISKMenuFlyoutHandler.Add)] = MapAdd,
            [nameof(ISKMenuFlyoutHandler.Remove)] = MapRemove,
            [nameof(ISKMenuFlyoutHandler.Clear)] = MapClear,
            [nameof(ISKMenuFlyoutHandler.Insert)] = MapInsert,
        };

        public SKMenuFlyoutHandler() : this(Mapper, CommandMapper) { }

        public SKMenuFlyoutHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper) { }

        public static void MapAdd(ISKMenuFlyoutHandler handler, IMenuFlyout menuElement, object? arg)
        {
            if (arg is ContextFlyoutItemHandlerUpdate args)
                handler.Add(args.MenuElement);
        }

        public static void MapRemove(ISKMenuFlyoutHandler handler, IMenuFlyout menuElement, object? arg)
        {
            if (arg is ContextFlyoutItemHandlerUpdate args)
                handler.Remove(args.MenuElement);
        }

        public static void MapInsert(ISKMenuFlyoutHandler handler, IMenuFlyout menuElement, object? arg)
        {
            if (arg is ContextFlyoutItemHandlerUpdate args)
                handler.Insert(args.Index, args.MenuElement);
        }

        public static void MapClear(ISKMenuFlyoutHandler handler, IMenuFlyout menuElement, object? arg)
            => handler.Clear();

        IMenuFlyout ISKMenuFlyoutHandler.VirtualView => VirtualView;

        PlatformView ISKMenuFlyoutHandler.PlatformView => PlatformView;

        private protected override void OnDisconnectHandler(object platformView)
        {
            base.OnDisconnectHandler(platformView);
            foreach (var item in VirtualView)
                item?.Handler?.DisconnectHandler();
        }
    }
}
