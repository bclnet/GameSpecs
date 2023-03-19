using Microsoft.Maui;
using System;

namespace StereoKit.Maui.Controls
{
    public class Switch : View
    {
        public bool IsToggled { get; internal set; }
        public object OnColor { get; internal set; }
        public object ThumbColor { get; internal set; }
    }
}