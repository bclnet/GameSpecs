using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.SwipeItem;

namespace StereoKit.Maui.Handlers
{
    public partial class SKSwipeItemMenuItemHandler : ISKSwipeItemMenuItemHandler
	{
		public static IPropertyMapper<ISwipeItemMenuItem, ISKSwipeItemMenuItemHandler> Mapper = new PropertyMapper<ISwipeItemMenuItem, ISKSwipeItemMenuItemHandler>(SKViewHandler.ElementMapper)
		{
			[nameof(ISwipeItemMenuItem.Visibility)] = MapVisibility,
			[nameof(IView.Background)] = MapBackground,
			[nameof(IMenuElement.Text)] = MapText,
			[nameof(ITextStyle.TextColor)] = MapTextColor,
			[nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(ITextStyle.Font)] = MapFont,
			[nameof(IMenuElement.Source)] = MapSource,
		};

		public static CommandMapper<ISwipeItemMenuItem, ISKSwipeItemMenuItemHandler> CommandMapper = new(SKElementHandler.ElementCommandMapper) { };

		public SKSwipeItemMenuItemHandler() : base(Mapper, CommandMapper) { }

		protected SKSwipeItemMenuItemHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

		protected SKSwipeItemMenuItemHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

		ISwipeItemMenuItem ISKSwipeItemMenuItemHandler.VirtualView => VirtualView;

		PlatformView ISKSwipeItemMenuItemHandler.PlatformView => PlatformView;

//#if !WINDOWS
//		ImageSourcePartLoader? _imageSourcePartLoader;
//		public ImageSourcePartLoader SourceLoader =>
//			_imageSourcePartLoader ??= new ImageSourcePartLoader(this, () => VirtualView, OnSetImageSource);

//		public static void MapSource(ISKSwipeItemMenuItemHandler handler, ISwipeItemMenuItem image) =>
//			MapSourceAsync(handler, image).FireAndForget(handler);

//		public static Task MapSourceAsync(ISKSwipeItemMenuItemHandler handler, ISwipeItemMenuItem image)
//		{
//			if (handler is SwipeItemMenuItemHandler platformHandler)
//				return platformHandler.SourceLoader.UpdateImageSourceAsync();
//			return Task.CompletedTask;
//		}
//#endif
	}
}
