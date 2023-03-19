using Microsoft.Maui;
using System;

namespace StereoKit.Maui.Controls
{
    public class MenuFlyoutItem
    {
        public Action<object, UI.Xaml.RoutedEventArgs> Click { get; internal set; }
        public object Icon { get; internal set; }
        public string Text { get; internal set; }
    }
}