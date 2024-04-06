using System;

namespace GameX.App.ExplorerVR.Controls
{
    public class VisualElement : Element
    {
        // Occurs when a VisualElement has been constructed and added to the object tree.
        // This event may occur before the VisualElement has been measured so should not
        // be relied on for size information.
        public event EventHandler Loaded;
        // Occurs when this VisualElement is no longer connected to the main object tree.
        public event EventHandler Unloaded;

        public override void Step() { }
    }
}
