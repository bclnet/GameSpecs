using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.MenuFlyoutSubItem;

namespace StereoKit.Maui.Handlers
{
    public partial class SKMenuFlyoutSubItemHandler
    {
        protected override PlatformView CreatePlatformElement() => new();

        public void Add(IMenuElement view) { }
        public void Remove(IMenuElement view) { }
        public void Clear() { }
        public void Insert(int index, IMenuElement view) { }
    }
}
