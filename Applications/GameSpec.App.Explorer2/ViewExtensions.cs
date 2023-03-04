namespace GameSpec.App.Explorer
{
    public static class ViewExtensions
    {
        public static void WriteLine(this Label label, string line)
        {
            if (label.Text.Length != 0) label.Text += "\n";
            label.Text += line;
            //label.ScrollToEnd();
        }
    }
}
