using System.Collections.Generic;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.UI
{
    public class ListGUILocal : List<string>, IListGUI
    {
        IUserInterface gui;
        string name;
        int water;
        List<int> ids = new();
        bool stateUpdates;

        public ListGUILocal() { gui = null; water = 0; stateUpdates = true; }

        void StateChanged()
        {
            if (!stateUpdates) return;

            int i;
            for (i = 0; i < Count; i++) gui.SetStateString($"{name}_item_{i}", base[i]);
            for (i = Count; i < water; i++) gui.SetStateString($"{name}_item_{i}", "");
            water = Count;
            gui.StateChanged(com_frameTime);
        }

        public void Config(IUserInterface gui, string name)
        {
            this.gui = gui;
            this.name = name;
        }

        public void Add(int id, string s)
        {
            var i = ids.FindIndex(x => x == id);
            if (i == -1) { Add(s); ids.Add(id); }
            else base[i] = s;
            StateChanged();
        }

        // use the element count as index for the ids
        public void Push(string s)
        {
            Add(s);
            ids.Add(ids.Count);
            StateChanged();
        }

        public bool Del(int id)
        {
            var i = ids.FindIndex(x => x == id);
            if (i == -1) return false;
            ids.RemoveAt(i);
            this.RemoveAt(i);
            StateChanged();
            return true;
        }

        public new void Clear()
        {
            ids.Clear();
            base.Clear();
            if (gui != null) StateChanged(); // will clear all the GUI variables and will set m_water back to 0
        }

        public int Num => base.Count;

        public int GetSelection(out string s, int size, int sel = 0) // returns the id, not the list index (or -1)
        {
            var sel2 = gui.State.GetInt($"{name}_sel_{sel}", "-1");
            if (sel2 == -1 || sel2 >= ids.Count) { s = string.Empty; return -1; }
            s = gui.State.GetString($"{name}_item_{sel2}", "");
            if (sel2 >= ids.Count) sel2 = 0; // don't let overflow
            gui.SetStateInt($"{name}_selid_0", ids[sel2]);
            return ids[sel2];
        }

        public void SetSelection(int sel)
        {
            gui.SetStateInt($"{name}_sel_0", sel);
            StateChanged();
        }

        public int NumSelections
            => gui.State.GetInt($"{name}_numsel");

        public bool IsConfigured
            => gui != null;

        public void SetStateChanges(bool enable)
        {
            stateUpdates = enable;
            StateChanged();
        }

        public void Shutdown()
        {
            gui = null;
            name = string.Empty;
            Clear();
        }
    }
}
