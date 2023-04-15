using System;

namespace StereoKit.UIX.Controls
{
    public class Image : View
    {
        public Image() => Console.WriteLine("Controls: Image");

        public void Step()
        {
            UI.Label("Image");
            //UI.Image();
        }
    }
}