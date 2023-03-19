using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using PlatformView = StereoKit.UIX.Controls.MenuBarItem;

namespace StereoKit.Maui.Handlers
{
    public partial class SKMenuBarItemHandler : ISKMenuBarItemHandler
	{
		public static IPropertyMapper<IMenuBarItem, ISKMenuBarItemHandler> Mapper = new PropertyMapper<IMenuBarItem, ISKMenuBarItemHandler>(ElementMapper)
		{
			//[nameof(IMenuBarItem.Text)] = MapText,
			//[nameof(IMenuBarItem.IsEnabled)] = MapIsEnabled,
		};

		public static CommandMapper<IMenuBarItem, ISKMenuBarItemHandler> CommandMapper = new(ElementCommandMapper)
		{
			[nameof(IMenuBarItemHandler.Add)] = MapAdd,
			[nameof(IMenuBarItemHandler.Remove)] = MapRemove,
			[nameof(IMenuBarItemHandler.Clear)] = MapClear,
			[nameof(IMenuBarItemHandler.Insert)] = MapInsert,
		};

		public SKMenuBarItemHandler() : this(Mapper, CommandMapper) { }

		public SKMenuBarItemHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper) { }

		public static void MapAdd(ISKMenuBarItemHandler handler, IMenuBarItem layout, object? arg)
		{
			if (arg is MenuBarItemHandlerUpdate args)
				handler.Add(args.MenuElement);
		}

		public static void MapRemove(ISKMenuBarItemHandler handler, IMenuBarItem layout, object? arg)
		{
			if (arg is MenuBarItemHandlerUpdate args)
				handler.Remove(args.MenuElement);
		}

		public static void MapInsert(ISKMenuBarItemHandler handler, IMenuBarItem layout, object? arg)
		{
			if (arg is MenuBarItemHandlerUpdate args)
				handler.Insert(args.Index, args.MenuElement);
		}

		public static void MapClear(ISKMenuBarItemHandler handler, IMenuBarItem layout, object? arg)
			=> handler.Clear();

		IMenuBarItem ISKMenuBarItemHandler.VirtualView => VirtualView;
		
		PlatformView ISKMenuBarItemHandler.PlatformView => PlatformView;

		private protected override void OnDisconnectHandler(object platformView)
		{
			base.OnDisconnectHandler(platformView);
			foreach (var item in VirtualView)
				item?.Handler?.DisconnectHandler();
		}
	}
}
