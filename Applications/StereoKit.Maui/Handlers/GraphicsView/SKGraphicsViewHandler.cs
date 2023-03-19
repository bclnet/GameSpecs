using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Views.PlatformTouchGraphicsView;

namespace StereoKit.Maui.Handlers
{
    public partial class SKGraphicsViewHandler : ISKGraphicsViewHandler
    {
        public static IPropertyMapper<IGraphicsView, ISKGraphicsViewHandler> Mapper = new PropertyMapper<IGraphicsView, ISKGraphicsViewHandler>(SKViewHandler.ViewMapper)
        {
            [nameof(IGraphicsView.Drawable)] = MapDrawable,
            [nameof(IView.FlowDirection)] = MapFlowDirection
        };

        public static CommandMapper<IGraphicsView, ISKGraphicsViewHandler> CommandMapper = new(ViewCommandMapper)
        {
            [nameof(IGraphicsView.Invalidate)] = MapInvalidate
        };

        public SKGraphicsViewHandler() : base(Mapper, CommandMapper) { }

        public SKGraphicsViewHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKGraphicsViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        IGraphicsView ISKGraphicsViewHandler.VirtualView => VirtualView;

        PlatformView ISKGraphicsViewHandler.PlatformView => PlatformView;

        //protected override void ConnectHandler(PlatformView platformView)
        //{
        //    //#if PLATFORM
        //    //			platformView.Connect(VirtualView);
        //    //#endif
        //    base.ConnectHandler(platformView);
        //}
        //protected override void DisconnectHandler(PlatformView platformView)
        //{
        //    //#if PLATFORM
        //    //			platformView.Disconnect();
        //    //#endif
        //    base.DisconnectHandler(platformView);
        //}
    }
}