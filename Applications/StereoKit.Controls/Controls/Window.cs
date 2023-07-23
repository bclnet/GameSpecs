using StereoKit.Maui.Platform;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StereoKit.UIX.Controls
{
    public class Window : View
    {
        public Rect WindowPositionSize { get; set; }
        public event Action<object, object> KeyEvent;

        public event Action<object, object> Resized;

        public void Close() { }

        List<object> Layers = new();
        public List<object> GetDefaultLayer() => Layers;

        Pose Pose = new(new Vec3(0, 0, -0.6f), Quat.LookDir(-Vec3.Forward));

        public override void OnStep(object? arg)
        {
            var nav = ((NavigationStack)arg!).FirstOrDefault();
            UI.WindowBegin("Window", ref Pose, new Vec2(50 * U.cm, 0));
            nav?.OnStep(null);
            UI.WindowEnd();
        }
    }
}