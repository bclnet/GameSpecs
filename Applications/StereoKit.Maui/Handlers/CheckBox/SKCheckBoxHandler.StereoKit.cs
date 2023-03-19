using Microsoft.Maui;
using System;
using PlatformView = StereoKit.UIX.Controls.CheckBox;

namespace StereoKit.Maui.Handlers
{
	public partial class SKCheckBoxHandler : SKViewHandler<ICheckBox, PlatformView>
	{
		protected override PlatformView CreatePlatformView() => new();

		public static void MapIsChecked(ISKCheckBoxHandler handler, ICheckBox check) { }
		public static void MapForeground(ISKCheckBoxHandler handler, ICheckBox check) { }
	}
}