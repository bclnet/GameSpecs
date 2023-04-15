using System;

namespace StereoKit.UIX.Controls
{
    public class Entry : View
    {
        public Entry() => Console.WriteLine("Controls: Entry");

        string? _text;
        public string? Text
        {
            get => _text;
            set => _text = value;
        }

        public void Step()
        {
            UI.Label("Entry");
            UI.Input("id", ref _text);
        }
    }
}