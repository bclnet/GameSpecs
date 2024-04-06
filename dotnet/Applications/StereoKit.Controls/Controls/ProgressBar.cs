namespace StereoKit.UIX.Controls
{
    public class ProgressBar : View
    {
        public override void OnStep(object? arg)
            => UI.Label("PROGRESSBAR");
    }
}