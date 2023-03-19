using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace StereoKit.Maui.Handlers
{
    public partial class SKWindowHandler : IWindowHandler
    {
        public static IPropertyMapper<IWindow, IWindowHandler> Mapper = new PropertyMapper<IWindow, IWindowHandler>(ElementHandler.ElementMapper)
        {
            [nameof(IWindow.Title)] = MapTitle,
            [nameof(IWindow.Content)] = MapContent,
            //[nameof(IWindow.X)] = MapX,
            //[nameof(IWindow.Y)] = MapY,
            //[nameof(IWindow.Width)] = MapWidth,
            //[nameof(IWindow.Height)] = MapHeight,
        };

        public static CommandMapper<IWindow, IWindowHandler> CommandMapper = new(ElementCommandMapper)
        {
            [nameof(IWindow.RequestDisplayDensity)] = MapRequestDisplayDensity,
        };

        public SKWindowHandler() : base(Mapper, CommandMapper) { }

        public SKWindowHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKWindowHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }
    }
}