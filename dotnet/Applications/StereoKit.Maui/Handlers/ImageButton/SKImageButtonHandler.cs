using Microsoft.Maui;
using Microsoft.Maui.Platform;
using PlatformImage = System.Object;
using PlatformImageView = StereoKit.UIX.Controls.Image;
using PlatformView = StereoKit.UIX.Controls.Button;

namespace StereoKit.Maui.Handlers
{
    public partial class SKImageButtonHandler : ISKImageButtonHandler
    {
        public static IPropertyMapper<IImage, ISKImageHandler> ImageMapper = new PropertyMapper<IImage, ISKImageHandler>(SKImageHandler.Mapper);

        public static IPropertyMapper<IImageButton, ISKImageButtonHandler> Mapper = new PropertyMapper<IImageButton, ISKImageButtonHandler>(ImageMapper)
        {
            [nameof(IButtonStroke.StrokeThickness)] = MapStrokeThickness,
            [nameof(IButtonStroke.StrokeColor)] = MapStrokeColor,
            [nameof(IButtonStroke.CornerRadius)] = MapCornerRadius,
            [nameof(IImageButton.Padding)] = MapPadding,
            //[nameof(IImageButton.Background)] = MapBackground,
        };

        public static CommandMapper<IImageButton, ISKImageButtonHandler> CommandMapper = new(SKViewHandler.ViewCommandMapper);

        ImageSourcePartLoader? _imageSourcePartLoader;
        public ImageSourcePartLoader SourceLoader =>
            _imageSourcePartLoader ??= new ImageSourcePartLoader(this, () => VirtualView, OnSetImageSource);

        public SKImageButtonHandler() : base(Mapper) { }

        public SKImageButtonHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKImageButtonHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        IImageButton ISKImageButtonHandler.VirtualView => VirtualView;

        IImage ISKImageHandler.VirtualView => VirtualView;

        PlatformImageView ISKImageHandler.PlatformView => default; // PlatformView;

        PlatformView ISKImageButtonHandler.PlatformView => PlatformView;

        ImageSourcePartLoader ISKImageHandler.SourceLoader => SourceLoader;
    }
}
