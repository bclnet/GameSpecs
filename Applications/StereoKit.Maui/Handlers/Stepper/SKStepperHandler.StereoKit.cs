using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.MauiStepper;

namespace StereoKit.Maui.Handlers
{
    public partial class SKStepperHandler : SKViewHandler<IStepper, PlatformView>
	{
		protected override PlatformView CreatePlatformView() => new();

		public static void MapMinimum(IViewHandler handler, IStepper stepper) { }
		public static void MapMaximum(IViewHandler handler, IStepper stepper) { }
		public static void MapIncrement(IViewHandler handler, IStepper stepper) { }
		public static void MapValue(IViewHandler handler, IStepper stepper) { }
	}
}