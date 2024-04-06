using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.MenuBar;

namespace StereoKit.Maui.Handlers
{
    public partial class SKMenuBarHandler : SKElementHandler<IMenuBar, PlatformView>, ISKMenuBarHandler
    {
        protected override PlatformView CreatePlatformElement() => new();

        public void Add(IMenuBarItem view) { }
        public void Remove(IMenuBarItem view) { }
        public void Clear() { }
        public void Insert(int index, IMenuBarItem view) { }
    }
}
