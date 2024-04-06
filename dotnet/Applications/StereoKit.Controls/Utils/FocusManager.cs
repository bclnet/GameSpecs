using StereoKit.UIX.Controls;

namespace StereoKit.UIX.Utils
{
    public class FocusManager
    {
        public static readonly FocusManager Instance = new();

        public bool SetCurrentFocusView(View platformView) { return true; }

        public View GetCurrentFocusView() { return new(); }

        public void ClearFocus() { }
    }
}