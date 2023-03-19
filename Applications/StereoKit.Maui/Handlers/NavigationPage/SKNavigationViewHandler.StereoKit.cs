using Microsoft.Maui;
using System;
using PlatformView = StereoKit.UIX.Views.Frame;

namespace StereoKit.Maui.Handlers
{
	public partial class SKNavigationViewHandler : SKViewHandler<IStackNavigationView, PlatformView>
	{
		protected override PlatformView CreatePlatformView() => new();

		public static void RequestNavigation(ISKNavigationViewHandler arg1, IStackNavigation arg2, object? arg3) => throw new NotImplementedException();
	}
}
