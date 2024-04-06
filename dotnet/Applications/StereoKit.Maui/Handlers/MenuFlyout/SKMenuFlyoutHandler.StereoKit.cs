using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.MenuFlyout;

namespace StereoKit.Maui.Handlers
{
    public partial class SKMenuFlyoutHandler : SKElementHandler<IMenuFlyout, PlatformView>, ISKMenuFlyoutHandler
    {
        protected override PlatformView CreatePlatformElement() => new();

        public void Add(IMenuElement view) { }
        public void Remove(IMenuElement view) { }
        public void Clear() { }
        public void Insert(int index, IMenuElement view) { }
    }
}
