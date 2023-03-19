namespace StereoKit.UIX.Controls
{
    public class ComboBox : View
    {
        string? _value;

        public void Step()
        {
            UI.Label("ComboBox");
            UI.Input("id", ref _value);
        }
    }
}