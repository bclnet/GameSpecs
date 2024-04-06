using Microsoft.Maui;
using System;
using PlatformView = StereoKit.UIX.Controls.Button;

namespace StereoKit.Maui.Handlers
{
    public partial class SKImageButtonHandler : SKViewHandler<IImageButton, PlatformView>
    {
        protected override PlatformView CreatePlatformView() => new();

        public static void MapStrokeColor(ISKImageButtonHandler handler, IButtonStroke buttonStroke) { }
        public static void MapStrokeThickness(ISKImageButtonHandler handler, IButtonStroke buttonStroke) { }
        public static void MapCornerRadius(ISKImageButtonHandler handler, IButtonStroke buttonStroke) { }
        public static void MapPadding(ISKImageButtonHandler handler, IImageButton imageButton) { }

        void OnSetImageSource(object? obj) => throw new NotImplementedException();
    }
}