using Microsoft.Maui;
using StereoKit.Maui.Platform;
using PlatformView = StereoKit.UIX.Views.Border;

namespace StereoKit.Maui.Handlers
{
    public partial class SKBorderHandler : ISKBorderHandler
    {
        public static IPropertyMapper<IBorderView, ISKBorderHandler> Mapper = new PropertyMapper<IBorderView, ISKBorderHandler>(ViewMapper)
        {
            [nameof(IContentView.Background)] = MapBackground,
            [nameof(IContentView.Content)] = MapContent,
            [nameof(IBorderStroke.Shape)] = MapStrokeShape,
            [nameof(IBorderStroke.Stroke)] = MapStroke,
            [nameof(IBorderStroke.StrokeThickness)] = MapStrokeThickness,
            [nameof(IBorderStroke.StrokeLineCap)] = MapStrokeLineCap,
            [nameof(IBorderStroke.StrokeLineJoin)] = MapStrokeLineJoin,
            [nameof(IBorderStroke.StrokeDashPattern)] = MapStrokeDashPattern,
            [nameof(IBorderStroke.StrokeDashOffset)] = MapStrokeDashOffset,
            [nameof(IBorderStroke.StrokeMiterLimit)] = MapStrokeMiterLimit
        };

        public static CommandMapper<IBorderView, SKBorderHandler> CommandMapper = new(ViewCommandMapper);

        public SKBorderHandler() : base(Mapper, CommandMapper) { }

        public SKBorderHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKBorderHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        IBorderView ISKBorderHandler.VirtualView => VirtualView;

        PlatformView ISKBorderHandler.PlatformView => PlatformView;

        public static void MapBackground(ISKBorderHandler handler, IBorderView border)
            => ((PlatformView?)handler.PlatformView)?.UpdateBackground(border);

        public static void MapStrokeShape(ISKBorderHandler handler, IBorderView border)
        {
            ((PlatformView?)handler.PlatformView)?.UpdateStrokeShape(border);
            MapBackground(handler, border);
        }

        public static void MapStroke(ISKBorderHandler handler, IBorderView border)
        {
            ((PlatformView?)handler.PlatformView)?.UpdateStroke(border);
            MapBackground(handler, border);
        }

        public static void MapStrokeThickness(ISKBorderHandler handler, IBorderView border)
        {
            ((PlatformView?)handler.PlatformView)?.UpdateStrokeThickness(border);
            MapBackground(handler, border);
        }

        public static void MapStrokeLineCap(ISKBorderHandler handler, IBorderView border)
            => ((PlatformView?)handler.PlatformView)?.UpdateStrokeLineCap(border);

        public static void MapStrokeLineJoin(ISKBorderHandler handler, IBorderView border)
            => ((PlatformView?)handler.PlatformView)?.UpdateStrokeLineJoin(border);

        public static void MapStrokeDashPattern(ISKBorderHandler handler, IBorderView border)
            => ((PlatformView?)handler.PlatformView)?.UpdateStrokeDashPattern(border);

        public static void MapStrokeDashOffset(ISKBorderHandler handler, IBorderView border)
            => ((PlatformView?)handler.PlatformView)?.UpdateStrokeDashOffset(border);

        public static void MapStrokeMiterLimit(ISKBorderHandler handler, IBorderView border)
            => ((PlatformView?)handler.PlatformView)?.UpdateStrokeMiterLimit(border);
    }
}
