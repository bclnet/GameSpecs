using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using System.Collections.Generic;
using PlatformView = StereoKit.UIX.Controls.MenuBar;

namespace StereoKit.Maui.Handlers
{
    public partial class SKMenuBarHandler : ISKMenuBarHandler
    {
        public static IPropertyMapper<IMenuBar, ISKMenuBarHandler> Mapper = new PropertyMapper<IMenuBar, ISKMenuBarHandler>(ElementMapper);

        public static CommandMapper<IMenuBar, ISKMenuBarHandler> CommandMapper = new(ElementCommandMapper)
        {
            [nameof(ISKMenuBarHandler.Add)] = MapAdd,
            [nameof(ISKMenuBarHandler.Remove)] = MapRemove,
            [nameof(ISKMenuBarHandler.Clear)] = MapClear,
            [nameof(ISKMenuBarHandler.Insert)] = MapInsert,
        };

        public SKMenuBarHandler() : this(Mapper, CommandMapper) { }

        public SKMenuBarHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper) { }

        public static void MapAdd(ISKMenuBarHandler handler, IMenuBar layout, object? arg)
        {
            if (arg is MenuBarHandlerUpdate args)
                handler.Add(args.MenuBarItem);
        }

        public static void MapRemove(ISKMenuBarHandler handler, IMenuBar layout, object? arg)
        {
            if (arg is MenuBarHandlerUpdate args)
                handler.Remove(args.MenuBarItem);
        }

        public static void MapInsert(ISKMenuBarHandler handler, IMenuBar layout, object? arg)
        {
            if (arg is MenuBarHandlerUpdate args)
                handler.Insert(args.Index, args.MenuBarItem);
        }

        public static void MapClear(ISKMenuBarHandler handler, IMenuBar layout, object? arg)
            => handler.Clear();

        IMenuBar ISKMenuBarHandler.VirtualView => VirtualView;

        PlatformView ISKMenuBarHandler.PlatformView => PlatformView;

        private protected override void OnDisconnectHandler(object platformView)
        {
            base.OnDisconnectHandler(platformView);
            foreach (var item in VirtualView)
                item?.Handler?.DisconnectHandler();
        }
    }
}
