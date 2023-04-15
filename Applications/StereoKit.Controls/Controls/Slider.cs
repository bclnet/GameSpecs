using System;

namespace StereoKit.UIX.Controls
{
    public class Slider : View
    {
        public Slider() => Console.WriteLine("Controls: Slider");

        public float MinValue { get; internal set; }
        public float MaxValue { get; internal set; }
        public float CurrentValue { get; internal set; }
        public object SlidedTrackColor { get; internal set; }
        public object BgTrackColor { get; internal set; }
        public object ThumbColor { get; internal set; }
    }
}