namespace System.NumericsX.OpenStack
{
    public interface IConsole
    {
        void Init();
        void Shutdown();

        // can't be combined with Init, because Init happens before renderer is started
        void LoadGraphics();

        bool ProcessEvent(SysEvent e, bool forceAccept);

        // the system code can release the mouse pointer when the console is active
        bool Active { get; }

        // clear the timers on any recent prints that are displayed in the notify lines
        void ClearNotifyLines();

        // some console commands, like timeDemo, will force the console closed before they start
        void Close();

        void Draw(bool forceFullScreen);
        void Print(string text);

        void SaveHistory();
        void LoadHistory();
    }
}
