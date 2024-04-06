using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.ComboBox;

namespace StereoKit.Maui.Handlers
{
    public partial class SKPickerHandler : SKViewHandler<IPicker, PlatformView>
    {
        protected override PlatformView CreatePlatformView() => new();

        public static void MapReload(ISKPickerHandler handler, IPicker picker, object? args) { }
        internal static void MapItems(ISKPickerHandler handler, IPicker picker) { }

        public static void MapTitle(ISKPickerHandler handler, IPicker view) { }
        public static void MapTitleColor(ISKPickerHandler handler, IPicker view) { }
        public static void MapSelectedIndex(ISKPickerHandler handler, IPicker view) { }
        public static void MapCharacterSpacing(ISKPickerHandler handler, IPicker view) { }
        public static void MapFont(ISKPickerHandler handler, IPicker view) { }
        public static void MapTextColor(ISKPickerHandler handler, IPicker view) { }
        public static void MapHorizontalTextAlignment(ISKPickerHandler handler, IPicker view) { }
        public static void MapVerticalTextAlignment(ISKPickerHandler handler, IPicker view) { }
    }
}