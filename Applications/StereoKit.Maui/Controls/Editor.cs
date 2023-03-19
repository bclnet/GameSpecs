using Microsoft.Maui;
using System;

namespace StereoKit.Maui.Controls
{
    public class Editor
    {
        public string Text { get; internal set; }
        public object TextColor { get; internal set; }
        public object HorizontalTextAlignment { get; internal set; }
        public object FontSize { get; internal set; }
        public FontAttributes FontAttributes { get; internal set; }
        public string? FontFamily { get; internal set; }
        public string Placeholder { get; internal set; }
    }
}