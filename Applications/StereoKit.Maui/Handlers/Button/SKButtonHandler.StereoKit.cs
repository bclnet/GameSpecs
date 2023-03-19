using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.Button;

namespace StereoKit.Maui.Handlers
{
    public partial class SKButtonHandler : SKViewHandler<IButton, PlatformView>
    {
        protected override PlatformView CreatePlatformView() => new();

        public static void MapStrokeColor(ISKButtonHandler handler, IButtonStroke buttonStroke) { }
        public static void MapStrokeThickness(ISKButtonHandler handler, IButtonStroke buttonStroke) { }
        public static void MapCornerRadius(ISKButtonHandler handler, IButtonStroke buttonStroke) { }
        public static void MapText(ISKButtonHandler handler, IText button) { }
        public static void MapTextColor(ISKButtonHandler handler, ITextStyle button) { }
        public static void MapCharacterSpacing(ISKButtonHandler handler, ITextStyle button) { }
        public static void MapFont(ISKButtonHandler handler, ITextStyle button) { }
        public static void MapPadding(ISKButtonHandler handler, IButton button) { }
        public static void MapImageSource(ISKButtonHandler handler, IImage image) { }

        void OnSetImageSource(object? obj) { }
    }
}