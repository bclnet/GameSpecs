using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Views.ContentView;

namespace StereoKit.Maui.Handlers
{
	public partial class SKSwipeItemViewHandler : SKViewHandler<ISwipeItemView, PlatformView>, ISKSwipeItemViewHandler
	{
		public static IPropertyMapper<ISwipeItemView, ISKSwipeItemViewHandler> Mapper = new PropertyMapper<ISwipeItemView, ISKSwipeItemViewHandler>(SKViewHandler.ViewMapper)
		{
			[nameof(ISwipeItemView.Content)] = MapContent,
			[nameof(ISwipeItemView.Visibility)] = MapVisibility
		};

		public static CommandMapper<ISwipeItemView, ISKSwipeItemViewHandler> CommandMapper = new(SKViewHandler.ViewCommandMapper);

		public SKSwipeItemViewHandler() : base(Mapper, CommandMapper) { }

		protected SKSwipeItemViewHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

		protected SKSwipeItemViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

		ISwipeItemView ISKSwipeItemViewHandler.VirtualView => VirtualView;

		PlatformView ISKSwipeItemViewHandler.PlatformView => PlatformView;
	}
}
