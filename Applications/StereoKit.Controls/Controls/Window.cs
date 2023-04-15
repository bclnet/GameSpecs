using System;
using System.Collections.Generic;

namespace StereoKit.UIX.Controls
{
    public class Window : View
    {
        public Window() => Console.WriteLine("Controls: Window");

        public Rect WindowPositionSize { get; set; }
        public event Action<object, object> KeyEvent;

        public event Action<object, object> Resized;

        public void Close() { }

        public ICollection<object> GetDefaultLayer() => default;
    }
}