using Microsoft.Maui;
using System;
using PlatformView = StereoKit.UIX.Controls.RadioButton;

namespace StereoKit.Maui.Handlers
{
	public partial class SKRadioButtonHandler : SKViewHandler<IRadioButton, PlatformView>
	{
		protected override PlatformView CreatePlatformView() => new();

		public static void MapBackground(ISKRadioButtonHandler handler, IRadioButton radioButton) { }
		public static void MapIsChecked(ISKRadioButtonHandler handler, IRadioButton radioButton) { }
		public static void MapContent(ISKRadioButtonHandler handler, IRadioButton radioButton) { }
		public static void MapTextColor(ISKRadioButtonHandler handler, ITextStyle textStyle) { }
		public static void MapCharacterSpacing(ISKRadioButtonHandler handler, ITextStyle textStyle) { }
		public static void MapFont(ISKRadioButtonHandler handler, ITextStyle textStyle) { }
		public static void MapStrokeColor(ISKRadioButtonHandler handler, IRadioButton radioButton) { }
		public static void MapStrokeThickness(ISKRadioButtonHandler handler, IRadioButton radioButton) { }
		public static void MapCornerRadius(ISKRadioButtonHandler handler, IRadioButton radioButton) { }
	}
}