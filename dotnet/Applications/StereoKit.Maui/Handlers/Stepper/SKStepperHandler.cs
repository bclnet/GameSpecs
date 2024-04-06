using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.MauiStepper;

namespace StereoKit.Maui.Handlers
{
    public partial class SKStepperHandler : ISKStepperHandler
    {
        public static IPropertyMapper<IStepper, ISKStepperHandler> Mapper = new PropertyMapper<IStepper, SKStepperHandler>(SKViewHandler.ViewMapper)
        {
            [nameof(IStepper.Interval)] = MapIncrement,
            [nameof(IStepper.Maximum)] = MapMaximum,
            [nameof(IStepper.Minimum)] = MapMinimum,
            [nameof(IStepper.Value)] = MapValue,
            [nameof(IStepper.IsEnabled)] = MapIsEnabled,
            [nameof(IStepper.Background)] = MapBackground,
        };

        public static CommandMapper<IStepper, ISKStepperHandler> CommandMapper = new(ViewCommandMapper);

        public SKStepperHandler() : base(Mapper) { }

        public SKStepperHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKStepperHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        IStepper ISKStepperHandler.VirtualView => VirtualView;

        PlatformView ISKStepperHandler.PlatformView => PlatformView;
    }
}