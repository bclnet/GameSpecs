using Microsoft.Maui;

namespace StereoKit.Maui.Handlers
{
    public partial class SKPageHandler : SKContentViewHandler, ISKPageHandler
    {
        public static new IPropertyMapper<IContentView, ISKPageHandler> Mapper = new PropertyMapper<IContentView, ISKPageHandler>(SKContentViewHandler.Mapper)
        {
            [nameof(IContentView.Background)] = MapBackground,
            [nameof(ITitledElement.Title)] = MapTitle,
        };

        public static new CommandMapper<IContentView, ISKPageHandler> CommandMapper = new(SKContentViewHandler.CommandMapper);

        public SKPageHandler() : base(Mapper, CommandMapper) { }

        public SKPageHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKPageHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }
    }
}
