#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIStepper;
#elif MONOANDROID
using PlatformView = Microsoft.Maui.Platform.MauiStepper;
#elif WINDOWS
using PlatformView = Microsoft.Maui.Platform.MauiStepper;
#elif TIZEN
using PlatformView = Microsoft.Maui.Platform.MauiStepper;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class StepperHandler : ISKStepperHandler
	{
		public static IPropertyMapper<IStepper, ISKStepperHandler> Mapper = new PropertyMapper<IStepper, StepperHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IStepper.Interval)] = MapIncrement,
			[nameof(IStepper.Maximum)] = MapMaximum,
			[nameof(IStepper.Minimum)] = MapMinimum,
			[nameof(IStepper.Value)] = MapValue,
#if ANDROID
			[nameof(IStepper.IsEnabled)] = MapIsEnabled,
#elif WINDOWS
			[nameof(IStepper.Background)] = MapBackground,
#endif
		};

		public static CommandMapper<IStepper, ISKStepperHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		public StepperHandler() : base(Mapper)
		{
		}

		public StepperHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public StepperHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		IStepper ISKStepperHandler.VirtualView => VirtualView;

		PlatformView ISKStepperHandler.PlatformView => PlatformView;
	}
}