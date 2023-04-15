using System;

namespace StereoKit.UIX.Controls
{
    public class Switch : View
    {
        public Switch() => Console.WriteLine("Controls: Switch");

        public bool IsToggled { get; internal set; }
        public object OnColor { get; internal set; }
        public object ThumbColor { get; internal set; }
    }
}