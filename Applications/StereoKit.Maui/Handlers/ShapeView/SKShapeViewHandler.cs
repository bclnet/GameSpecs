using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.MauiShapeView;

namespace StereoKit.Maui.Handlers
{
    public partial class SKShapeViewHandler : ISKShapeViewHandler
    {
        public static IPropertyMapper<IShapeView, ISKShapeViewHandler> Mapper = new PropertyMapper<IShapeView, ISKShapeViewHandler>(SKViewHandler.ViewMapper)
        {
            [nameof(IShapeView.Background)] = MapBackground,
            [nameof(IShapeView.Shape)] = MapShape,
            [nameof(IShapeView.Aspect)] = MapAspect,
            [nameof(IShapeView.Fill)] = MapFill,
            [nameof(IShapeView.Stroke)] = MapStroke,
            [nameof(IShapeView.StrokeThickness)] = MapStrokeThickness,
            [nameof(IShapeView.StrokeDashPattern)] = MapStrokeDashPattern,
            [nameof(IShapeView.StrokeDashOffset)] = MapStrokeDashOffset,
            [nameof(IShapeView.StrokeLineCap)] = MapStrokeLineCap,
            [nameof(IShapeView.StrokeLineJoin)] = MapStrokeLineJoin,
            [nameof(IShapeView.StrokeMiterLimit)] = MapStrokeMiterLimit
        };

        public static CommandMapper<IShapeView, ISKShapeViewHandler> CommandMapper = new(ViewCommandMapper);

        public SKShapeViewHandler() : base(Mapper) { }

        public SKShapeViewHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKShapeViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        IShapeView ISKShapeViewHandler.VirtualView => VirtualView;

        PlatformView ISKShapeViewHandler.PlatformView => PlatformView;
    }
}