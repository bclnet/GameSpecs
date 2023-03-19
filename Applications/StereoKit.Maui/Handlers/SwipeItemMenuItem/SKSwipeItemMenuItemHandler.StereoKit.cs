using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using System;
using PlatformView = StereoKit.UIX.Controls.SwipeItem;

namespace StereoKit.Maui.Handlers
{
    public partial class SKSwipeItemMenuItemHandler : ElementHandler<ISwipeItemMenuItem, PlatformView>
    {
        protected override PlatformView CreatePlatformElement() => new();

        public static void MapTextColor(ISKSwipeItemMenuItemHandler handler, ITextStyle view) { }
        public static void MapCharacterSpacing(ISKSwipeItemMenuItemHandler handler, ITextStyle view) { }
        public static void MapFont(ISKSwipeItemMenuItemHandler handler, ITextStyle view) { }
        public static void MapText(ISKSwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) { }
        public static void MapBackground(ISKSwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) { }
        public static void MapVisibility(ISKSwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) { }
        public static void MapSource(ISKSwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) { }

        void OnSetImageSource(object? obj) => throw new NotImplementedException();
    }
}
