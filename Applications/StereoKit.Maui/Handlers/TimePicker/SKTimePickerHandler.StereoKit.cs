using Microsoft.Maui;
using System;

namespace StereoKit.Maui.Handlers
{
    public partial class SKTimePickerHandler : SKViewHandler<ITimePicker, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static void MapFormat(ISKTimePickerHandler handler, ITimePicker view) { }
		public static void MapTime(ISKTimePickerHandler handler, ITimePicker view) { }
		public static void MapCharacterSpacing(ISKTimePickerHandler handler, ITimePicker view) { }
		public static void MapFont(ISKTimePickerHandler handler, ITimePicker view) { }
		public static void MapTextColor(ISKTimePickerHandler handler, ITimePicker timePicker) { }
	}
}