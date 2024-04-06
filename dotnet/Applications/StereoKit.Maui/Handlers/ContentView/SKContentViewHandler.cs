using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Views.ContentView;

namespace StereoKit.Maui.Handlers
{
    public partial class SKContentViewHandler : ISKContentViewHandler
    {
        public static IPropertyMapper<IContentView, ISKContentViewHandler> Mapper =
            new PropertyMapper<IContentView, ISKContentViewHandler>(ViewMapper)
            {
                [nameof(IContentView.Content)] = MapContent,
            };

        public static CommandMapper<IContentView, ISKContentViewHandler> CommandMapper = new(ViewCommandMapper);

        public SKContentViewHandler() : base(Mapper, CommandMapper) { }

        public SKContentViewHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKContentViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        IContentView ISKContentViewHandler.VirtualView => VirtualView;

        PlatformView ISKContentViewHandler.PlatformView => PlatformView;
    }
}
