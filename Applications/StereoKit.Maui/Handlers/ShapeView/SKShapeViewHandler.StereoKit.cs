using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.MauiShapeView;

namespace StereoKit.Maui.Handlers
{
    public partial class SKShapeViewHandler : SKViewHandler<IShapeView, PlatformView>
    {
        protected override PlatformView CreatePlatformView() => new();

        public static void MapBackground(ISKShapeViewHandler handler, IShapeView shapeView) { }
        public static void MapShape(ISKShapeViewHandler handler, IShapeView shapeView) { }
        public static void MapAspect(ISKShapeViewHandler handler, IShapeView shapeView) { }
        public static void MapFill(ISKShapeViewHandler handler, IShapeView shapeView) { }
        public static void MapStroke(ISKShapeViewHandler handler, IShapeView shapeView) { }
        public static void MapStrokeThickness(ISKShapeViewHandler handler, IShapeView shapeView) { }
        public static void MapStrokeDashPattern(ISKShapeViewHandler handler, IShapeView shapeView) { }
        public static void MapStrokeDashOffset(ISKShapeViewHandler handler, IShapeView shapeView) { }
        public static void MapStrokeLineCap(ISKShapeViewHandler handler, IShapeView shapeView) { }
        public static void MapStrokeLineJoin(ISKShapeViewHandler handler, IShapeView shapeView) { }
        public static void MapStrokeMiterLimit(ISKShapeViewHandler handler, IShapeView shapeView) { }
    }
}