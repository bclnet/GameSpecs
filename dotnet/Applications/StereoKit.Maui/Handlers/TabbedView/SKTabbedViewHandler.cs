using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.View;

namespace StereoKit.Maui.Handlers
{
    public partial class SKTabbedViewHandler : SKViewHandler<ITabbedView, PlatformView>, ISKTabbedViewHandler
    {
        public static IPropertyMapper<ITabbedView, ISKTabbedViewHandler> Mapper = new PropertyMapper<ITabbedView, ISKTabbedViewHandler>(SKViewHandler.ViewMapper);

        public static CommandMapper<ITabbedView, ISKTabbedViewHandler> CommandMapper = new(ViewCommandMapper);

        public SKTabbedViewHandler() : base(Mapper, CommandMapper) { }

        public SKTabbedViewHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKTabbedViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        protected override PlatformView CreatePlatformView() => new();
    }
}
