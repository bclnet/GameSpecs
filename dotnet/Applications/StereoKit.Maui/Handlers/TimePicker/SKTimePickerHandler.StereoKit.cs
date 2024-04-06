using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.TimePicker;

namespace StereoKit.Maui.Handlers
{
    public partial class SKTimePickerHandler : SKViewHandler<ITimePicker, PlatformView>
    {
        protected override PlatformView CreatePlatformView() => new();

        public static void MapFormat(ISKTimePickerHandler handler, ITimePicker view) { }
        public static void MapTime(ISKTimePickerHandler handler, ITimePicker view) { }
        public static void MapCharacterSpacing(ISKTimePickerHandler handler, ITimePicker view) { }
        public static void MapFont(ISKTimePickerHandler handler, ITimePicker view) { }
        public static void MapTextColor(ISKTimePickerHandler handler, ITimePicker timePicker) { }
    }
}