using Microsoft.Maui;
using PlatformView = StereoKit.Maui.Controls.MenuBar;

namespace StereoKit.Maui.Handlers
{
    public partial class SKMenuBarItemHandler : SKElementHandler<IMenuBarItem, PlatformView>, ISKMenuBarItemHandler
    {
        protected override PlatformView CreatePlatformElement() => new();

        public void Add(IMenuElement view) { }
        public void Remove(IMenuElement view) { }
        public void Clear() { }
        public void Insert(int index, IMenuElement view) { }
    }
}
