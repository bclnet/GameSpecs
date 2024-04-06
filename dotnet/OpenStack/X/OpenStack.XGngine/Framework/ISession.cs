using System.NumericsX.OpenStack.Gngine.Render;
using System.NumericsX.OpenStack.Gngine.UI;

namespace System.NumericsX.OpenStack.Gngine.Framework
{
    // needed by the gui system for the load game menu
    public struct LogStats
    {
        public short health;
        public short heartRate;
        public short stamina;
        public short combat;
    }

    public enum MSG
    {
        OK,
        ABORT,
        OKCANCEL,
        YESNO,
        PROMPT,
        CDKEY,
        INFO,
        WAIT
    }

    public delegate string HandleGuiCommand(string s);

    public abstract class ISession
    {
        public const int MAX_LOGGED_STATS = 60 * 120;       // log every half second

        // The render world and sound world used for this session.
        public IRenderWorld rw;
        public ISoundWorld sw;

        // The renderer and sound system will write changes to writeDemo. Demos can be recorded and played at the same time when splicing.
        public VFileDemo readDemo;
        public VFileDemo writeDemo;
        public int renderdemoVersion;

        // Called in an orderly fashion at system startup, so commands, cvars, files, etc are all available.
        public abstract void Init();

        // Shut down the session.
        public abstract void Shutdown();

        // Called on errors and game exits.
        public abstract void Stop();

        // Redraws the screen, handling games, guis, console, etc during normal once-a-frame updates, outOfSequence will be false,
        // but when the screen is updated in a modal manner, as with utility output, the mouse cursor will be released if running windowed.
        public abstract void UpdateScreen(bool outOfSequence = true);

        // Called when console prints happen, allowing the loading screen to redraw if enough time has passed.
        public abstract void PacifierUpdate();

        // Called every frame, possibly spinning in place if we are above maxFps, or we haven't advanced at least one demo frame.
        // Returns the number of milliseconds since the last frame.
        public abstract void Frame();

        // Returns true if a multiplayer game is running.
        // CVars and commands are checked differently in multiplayer mode.
        bool IsMultiplayer { get; }

        // Processes the given event.
        public abstract bool ProcessEvent(SysEvent event_);

        // Activates the main menu
        public abstract void StartMenu(bool playIntro = false);

        public abstract void SetGUI(IUserInterface gui, HandleGuiCommand handle);

        // Updates gui and dispatched events to it
        public abstract void GuiFrameEvents();

        // fires up the optional GUI event, also returns them if you set wait to true
        // if MSG_PROMPT and wait, returns the prompt string or NULL if aborted
        // if MSG_CDKEY and want, returns the cd key or NULL if aborted
        // network tells wether one should still run the network loop in a wait dialog
        public abstract string MessageBox(MSG type, string message, string title = null, bool wait = false, string fire_yes = null, string fire_no = null, bool network = false);

        public abstract void StopBox();

        // monitor this download in a progress box to either abort or completion
        public abstract void DownloadProgressBox(BackgroundDownload bgl, string title, int progress_start = 0, int progress_end = 100);

        public abstract void SetPlayingSoundWorld();

        // this is used by the sound system when an OnDemand sound is loaded, so the game action doesn't advance and get things out of sync
        public abstract void TimeHitch(int msec);

        // read and write the cd key data to files
        // doesn't perform any validity checks
        public abstract void ReadCDKey();

        public abstract void WriteCDKey();

        // returns NULL for if xp is true and xp key is not valid or not present
        public abstract string GetCDKey(bool xp);

        // check keys for validity when typed in by the user ( with checksum verification ) store the new set of keys if they are found valid
        public abstract bool CheckKey(string key, bool netConnect, bool[] offline_valid);

        // verify the current set of keys for validity strict -> keys in state CDKEY_CHECKING state are not ok
        public abstract bool CDKeysAreValid(bool strict);

        // wipe the key on file if the network check finds it invalid
        public abstract void ClearCDKey(bool[] valid);

        // configure gui variables for mainmenu.gui and cd key state
        public abstract void SetCDKeyGuiVars();

        public abstract bool WaitingForGameAuth { get; }

        // got reply from master about the keys. if !valid, auth_msg given
        public abstract void CDKeysAuthReply(bool valid, string authMsg);

        public abstract string CurrentMapName { get; }

        public abstract int SaveGameVersion { get; }
    }
}
