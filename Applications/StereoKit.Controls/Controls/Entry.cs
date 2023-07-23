namespace StereoKit.UIX.Controls
{
    public class Entry : View
    {
        string? _text;
        public string? Text
        {
            get => _text;
            set => _text = value;
        }

        public override void OnStep(object? arg)
        {
            UI.Label("Entry");
            UI.Input("id", ref _text);
        }
    }
}