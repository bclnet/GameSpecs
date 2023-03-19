namespace StereoKit.UIX.Controls
{
    public class CheckBox : View
    {
        bool _checked;
        public bool IsChecked
        {
            get => _checked;
            set => _checked = value;
        }

        public Color Color { get; internal set; }

        public void Step()
        {
            UI.Toggle("CheckBox", ref _checked);
        }
    }
}