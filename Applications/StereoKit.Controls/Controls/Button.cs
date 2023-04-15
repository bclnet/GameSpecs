using System;

namespace StereoKit.UIX.Controls
{
    public class Button : View
    {
        public Button() => Console.WriteLine("Controls: Button");

        public void Step()
        {
            UI.Button("Button");
        }
    }
}