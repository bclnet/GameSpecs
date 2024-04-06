using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using StereoKit.Maui.Platform;
using PlatformView = StereoKit.Maui.Platform.LayoutViewGroup;

namespace StereoKit.Maui.Handlers
{
    public partial class SKLayoutHandler : ISKLayoutHandler
    {
        public static IPropertyMapper<ILayout, ISKLayoutHandler> Mapper = new PropertyMapper<ILayout, ISKLayoutHandler>(ViewMapper)
        {
            [nameof(ILayout.Background)] = MapBackground,
            [nameof(ILayout.ClipsToBounds)] = MapClipsToBounds,
            [nameof(IView.InputTransparent)] = MapInputTransparent,
        };

        public static CommandMapper<ILayout, ISKLayoutHandler> CommandMapper = new(ViewCommandMapper)
        {
            [nameof(ILayoutHandler.Add)] = MapAdd,
            [nameof(ILayoutHandler.Remove)] = MapRemove,
            [nameof(ILayoutHandler.Clear)] = MapClear,
            [nameof(ILayoutHandler.Insert)] = MapInsert,
            [nameof(ILayoutHandler.Update)] = MapUpdate,
            [nameof(ILayoutHandler.UpdateZIndex)] = MapUpdateZIndex,
        };

        public SKLayoutHandler() : base(Mapper, CommandMapper) { }

        public SKLayoutHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        ILayout ISKLayoutHandler.VirtualView => VirtualView;

        PlatformView ISKLayoutHandler.PlatformView => PlatformView;

        public static void MapBackground(ISKLayoutHandler handler, ILayout layout)
            => ((PlatformView?)handler.PlatformView)?.UpdateBackground(layout);

        public static void MapClipsToBounds(ISKLayoutHandler handler, ILayout layout)
            => ((PlatformView?)handler.PlatformView)?.UpdateClipsToBounds(layout);

        public static void MapAdd(ISKLayoutHandler handler, ILayout layout, object? arg)
        {
            if (arg is LayoutHandlerUpdate args)
                handler.Add(args.View);
        }

        public static void MapRemove(ISKLayoutHandler handler, ILayout layout, object? arg)
        {
            if (arg is LayoutHandlerUpdate args)
                handler.Remove(args.View);
        }

        public static void MapInsert(ISKLayoutHandler handler, ILayout layout, object? arg)
        {
            if (arg is LayoutHandlerUpdate args)
                handler.Insert(args.Index, args.View);
        }

        public static void MapClear(ISKLayoutHandler handler, ILayout layout, object? arg)
            => handler.Clear();

        static void MapUpdate(ISKLayoutHandler handler, ILayout layout, object? arg)
        {
            if (arg is LayoutHandlerUpdate args)
                handler.Update(args.Index, args.View);
        }

        static void MapUpdateZIndex(ISKLayoutHandler handler, ILayout layout, object? arg)
        {
            if (arg is IView view)
                handler.UpdateZIndex(view);
        }

        /// <summary>
        /// Converts a FlowDirection to the appropriate FlowDirection for cross-platform layout 
        /// </summary>
        /// <param name="flowDirection"></param>
        /// <returns>The FlowDirection to assume for cross-platform layout</returns>
        internal static FlowDirection GetLayoutFlowDirection(FlowDirection flowDirection)
            => FlowDirection.LeftToRight;
    }
}
