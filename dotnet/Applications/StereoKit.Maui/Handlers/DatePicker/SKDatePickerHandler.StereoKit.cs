using Microsoft.Maui;
using System;
using PlatformView = StereoKit.UIX.Controls.DatePicker;

namespace StereoKit.Maui.Handlers
{
    public partial class SKDatePickerHandler : SKViewHandler<IDatePicker, PlatformView>
    {
        protected override PlatformView CreatePlatformView() => new();

        public static void MapFormat(ISKDatePickerHandler handler, IDatePicker datePicker) { }
        public static void MapDate(ISKDatePickerHandler handler, IDatePicker datePicker) { }
        public static void MapMinimumDate(ISKDatePickerHandler handler, IDatePicker datePicker) { }
        public static void MapMaximumDate(ISKDatePickerHandler handler, IDatePicker datePicker) { }
        public static void MapCharacterSpacing(ISKDatePickerHandler handler, IDatePicker datePicker) { }
        public static void MapFont(ISKDatePickerHandler handler, IDatePicker datePicker) { }
        public static void MapTextColor(ISKDatePickerHandler handler, IDatePicker datePicker) { }
    }
}