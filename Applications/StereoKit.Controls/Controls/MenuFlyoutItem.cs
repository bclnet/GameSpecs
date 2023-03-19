#if MENU2
using System;

namespace StereoKit.UI.Controls
{
    public class MenuFlyoutItem
    {
        public Action<object, EventArgs> Click { get; internal set; }
        public object Icon { get; internal set; }
        public string Text { get; internal set; }
    }
}
#endif