using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.Switch;

namespace StereoKit.Maui.Handlers
{
    public partial class SKSwitchHandler : ISKSwitchHandler
    {
        public static IPropertyMapper<ISwitch, ISKSwitchHandler> Mapper = new PropertyMapper<ISwitch, ISKSwitchHandler>(SKViewHandler.ViewMapper)
        {
            [nameof(ISwitch.IsOn)] = MapIsOn,
            [nameof(ISwitch.ThumbColor)] = MapThumbColor,
            [nameof(ISwitch.TrackColor)] = MapTrackColor,
        };

        public static CommandMapper<ISwitch, ISKSwitchHandler> CommandMapper = new(ViewCommandMapper);

        public SKSwitchHandler() : base(Mapper) { }

        public SKSwitchHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKSwitchHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        ISwitch ISKSwitchHandler.VirtualView => VirtualView;

        PlatformView ISKSwitchHandler.PlatformView => PlatformView;
    }
}