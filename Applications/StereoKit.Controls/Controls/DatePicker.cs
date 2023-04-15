using System;

namespace StereoKit.UIX.Controls
{
    public class DatePicker : View
    {
        public DatePicker() => Console.WriteLine("Controls: DatePicker");

        string? _value;

        public void Step()
        {
            UI.Label("DatePicker");
            UI.Input("id", ref _value);
        }
    }
}