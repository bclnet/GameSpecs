using System;

namespace StereoKit.UIX.Controls
{
    public class MenuFlyoutItem
    {
        public Action<object, EventArgs> Click { get; set; }
        public object Icon { get; set; }
        public string Text { get; set; }
        public void UpdateIsEnabled(bool isEnabled) { }
    }
}
