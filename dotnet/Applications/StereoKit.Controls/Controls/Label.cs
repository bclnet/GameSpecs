namespace StereoKit.UIX.Controls
{
    public class Label : View
    {
        public string Text { get; set; }

        public override void OnStep(object? arg)
            => UI.Label(Text);
    }
}