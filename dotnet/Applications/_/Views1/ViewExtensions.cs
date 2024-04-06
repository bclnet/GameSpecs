using Microsoft.Maui.Controls;

namespace GameX.App.Explorer.Views
{
    public static class ViewExtensions
    {
        public static void WriteLine(this Label source, string line)
        {
            source.Text ??= string.Empty;
            if (source.Text.Length != 0) source.Text += "\n";
            source.Text += line;
            //label.ScrollToEnd();
        }

        public static void WriteLine(this Entry source, string line)
        {
            source.Text ??= string.Empty;
            if (source.Text.Length != 0) source.Text += "\n";
            source.Text += line;
            //label.ScrollToEnd();
        }
    }
}
