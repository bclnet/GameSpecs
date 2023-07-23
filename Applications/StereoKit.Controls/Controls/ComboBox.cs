namespace StereoKit.UIX.Controls
{
    public class ComboBox : View
    {
        string? _value;

        public override void OnStep(object? arg)
        {
            UI.Input("id", ref _value);
        }
    }
}