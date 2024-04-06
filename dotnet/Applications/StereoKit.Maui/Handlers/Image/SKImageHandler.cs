using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;
using System.Threading;
using PlatformView = StereoKit.UIX.Controls.Image;

namespace StereoKit.Maui.Handlers
{
    public partial class SKImageHandler : ISKImageHandler
    {
        public static IPropertyMapper<IImage, ISKImageHandler> Mapper = new PropertyMapper<IImage, ISKImageHandler>(SKViewHandler.ViewMapper)
        {
            [nameof(IImage.Background)] = MapBackground,
            [nameof(IImage.Aspect)] = MapAspect,
            [nameof(IImage.IsAnimationPlaying)] = MapIsAnimationPlaying,
            [nameof(IImage.Source)] = MapSource,
        };

        public static CommandMapper<IImage, IImageHandler> CommandMapper = new(SKViewHandler.ViewCommandMapper);

        ImageSourcePartLoader? _imageSourcePartLoader;
        public ImageSourcePartLoader SourceLoader =>
            _imageSourcePartLoader ??= new ImageSourcePartLoader(this, () => VirtualView, OnSetImageSource);

        public SKImageHandler() : base(Mapper) { }

        public SKImageHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKImageHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        IImage ISKImageHandler.VirtualView => VirtualView;

        PlatformView ISKImageHandler.PlatformView => PlatformView;
    }
}
