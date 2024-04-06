using Microsoft.Maui;
using System;
using PlatformView = StereoKit.UIX.Controls.Image;

namespace StereoKit.Maui.Handlers
{
    public partial class SKImageHandler : SKViewHandler<IImage, PlatformView>
    {
        protected override PlatformView CreatePlatformView() => new();
        public static void MapAspect(ISKImageHandler handler, IImage image) { }
        public static void MapIsAnimationPlaying(ISKImageHandler handler, IImage image) { }
        public static void MapSource(ISKImageHandler handler, IImage image) { }
        void OnSetImageSource(object? obj) => throw new NotImplementedException();
    }
}