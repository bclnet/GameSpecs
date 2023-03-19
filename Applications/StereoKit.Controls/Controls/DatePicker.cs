namespace StereoKit.UIX.Controls
{
    public class DatePicker : View
    {
        string? _value;

        public void Step()
        {
            UI.Label("DatePicker");
            UI.Input("id", ref _value);
        }
    }
}