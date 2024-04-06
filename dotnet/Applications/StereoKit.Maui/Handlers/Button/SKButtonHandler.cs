using Microsoft.Maui;
using Microsoft.Maui.Platform;
using PlatformView = StereoKit.UIX.Controls.Button;

namespace StereoKit.Maui.Handlers
{
    public partial class SKButtonHandler : ISKButtonHandler
	{
		ImageSourcePartLoader? _imageSourcePartLoader;
		public ImageSourcePartLoader ImageSourceLoader =>
			_imageSourcePartLoader ??= new ImageSourcePartLoader(this, () => (VirtualView as IImageButton), OnSetImageSource);

		public static IPropertyMapper<IImage, ISKButtonHandler> ImageButtonMapper = new PropertyMapper<IImage, ISKButtonHandler>()
		{
			[nameof(IImage.Source)] = MapImageSource
		};

		public static IPropertyMapper<ITextButton, ISKButtonHandler> TextButtonMapper = new PropertyMapper<ITextButton, ISKButtonHandler>()
		{
			[nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(ITextStyle.Font)] = MapFont,
			[nameof(ITextStyle.TextColor)] = MapTextColor,
			[nameof(IText.Text)] = MapText
		};

		public static IPropertyMapper<IButton, ISKButtonHandler> Mapper = new PropertyMapper<IButton, ISKButtonHandler>(TextButtonMapper, ImageButtonMapper, SKViewHandler.ViewMapper)
		{
			[nameof(IButton.Background)] = MapBackground,
			[nameof(IButton.Padding)] = MapPadding,
			[nameof(IButtonStroke.StrokeThickness)] = MapStrokeThickness,
			[nameof(IButtonStroke.StrokeColor)] = MapStrokeColor,
			[nameof(IButtonStroke.CornerRadius)] = MapCornerRadius
		};

		public static CommandMapper<IButton, ISKButtonHandler> CommandMapper = new(ViewCommandMapper);

		public SKButtonHandler() : base(Mapper, CommandMapper) { }

		public SKButtonHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

		public SKButtonHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

		IButton ISKButtonHandler.VirtualView => VirtualView;

		PlatformView ISKButtonHandler.PlatformView => PlatformView;
	}
}