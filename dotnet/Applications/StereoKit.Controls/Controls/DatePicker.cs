namespace StereoKit.UIX.Controls
{
    public class DatePicker : View
    {
        string? _value;

        public override void OnStep(object? arg)
        {
            UI.Label("DatePicker");
            UI.Input("id", ref _value);
        }
    }
}