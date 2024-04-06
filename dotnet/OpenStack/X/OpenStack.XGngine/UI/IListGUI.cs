namespace System.NumericsX.OpenStack.Gngine.UI
{
    public interface IListGUI
    {
        void Config(IUserInterface pGUI, string name);
        void Add(int id, string s);
        // use the element count as index for the ids
        void Push(string s);
        bool Del(int id);
        void Clear();
        int Num { get; }
        int GetSelection(out string s, int size, int sel = 0); // returns the id, not the list index (or -1)
        void SetSelection(int sel);
        int NumSelections { get; }
        bool IsConfigured { get; }
        // by default, any modification to the list will trigger a full GUI refresh immediately
        void SetStateChanges(bool enable);
        void Shutdown();
    }
}