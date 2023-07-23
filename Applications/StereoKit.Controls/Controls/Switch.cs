namespace StereoKit.UIX.Controls
{
    public class Switch : View
    {
        public bool IsToggled { get; internal set; }
        public object OnColor { get; internal set; }
        public object ThumbColor { get; internal set; }

        public override void OnStep(object? arg)
            => UI.Label("SWITCH");
    }
}