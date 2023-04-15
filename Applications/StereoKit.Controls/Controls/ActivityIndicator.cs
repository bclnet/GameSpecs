using System;

namespace StereoKit.UIX.Controls
{
    public class ActivityIndicator : View
    {
        public ActivityIndicator() => Console.WriteLine("Controls: ActivityIndicator");

        public bool IsRunning { get; internal set; }
        public bool Color { get; internal set; }

        public void Step()
        {
            UI.Label("ActivityIndicator");
        }
    }
}