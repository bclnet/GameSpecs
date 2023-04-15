using System;

namespace StereoKit.UIX.Controls
{
    public class ComboBox : View
    {
        public ComboBox() => Console.WriteLine("Controls: ComboBox");

        string? _value;

        public void Step()
        {
            UI.Label("ComboBox");
            UI.Input("id", ref _value);
        }
    }
}