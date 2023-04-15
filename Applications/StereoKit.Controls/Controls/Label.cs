using System;

namespace StereoKit.UIX.Controls
{
    public class Label : View
    {
        public Label() => Console.WriteLine("Controls: Label");

        public void Step()
        {
            UI.Label("Label");
        }
    }
}