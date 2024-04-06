using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace StereoKit.Maui.Handlers
{
    public partial class SKWindowHandler : ISKWindowHandler
    {
        public static IPropertyMapper<IWindow, ISKWindowHandler> Mapper = new PropertyMapper<IWindow, ISKWindowHandler>(ElementHandler.ElementMapper)
        {
            [nameof(IWindow.Title)] = MapTitle,
            [nameof(IWindow.Content)] = MapContent,
            //[nameof(IWindow.X)] = MapX,
            //[nameof(IWindow.Y)] = MapY,
            //[nameof(IWindow.Width)] = MapWidth,
            //[nameof(IWindow.Height)] = MapHeight,
        };

        public static CommandMapper<IWindow, ISKWindowHandler> CommandMapper = new(ElementCommandMapper)
        {
            [nameof(IWindow.RequestDisplayDensity)] = MapRequestDisplayDensity,
        };

        public SKWindowHandler() : base(Mapper, CommandMapper) { }

        public SKWindowHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKWindowHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }
    }
}