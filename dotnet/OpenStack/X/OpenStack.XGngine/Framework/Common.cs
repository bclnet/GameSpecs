namespace System.NumericsX.OpenStack.Gngine.Framework
{
    public class C
    {
        public const string STRTABLE_ID = "#str_";

        public static readonly CVar vr_refresh;
        public static readonly CVar vr_supersampling;
        public static readonly CVar vr_msaa;

        public static readonly CVar com_version;
        public static readonly CVar com_skipRenderer;
        public static readonly CVar com_asyncInput;
        public static readonly CVar com_asyncSound;
        public static readonly CVar com_purgeAll;
        public static readonly CVar com_developer;
        public static readonly CVar com_allowConsole;
        public static readonly CVar com_speeds;
        public static readonly CVar com_showFPS;
        public static readonly CVar com_showMemoryUsage;
        public static readonly CVar com_showAsyncStats;
        public static readonly CVar com_showSoundDecoders;
        public static readonly CVar com_makingBuild;
        public static readonly CVar com_updateLoadSize;

        public static int time_gameFrame;          // game logic time
        public static int time_gameDraw;           // game present time
        public static int time_frontend;           // renderer frontend time
        public static int time_backend;            // renderer backend time

        //public static int com_frameTime;           // time for the current frame in milliseconds
        //public static volatile int com_ticNumber;          // 60 hz tics, incremented by async function
        public static EDITOR com_editors;         // current active editor(s)
        public static bool com_editorActive;       // true if an editor has focus

#if true //_WIN32
        public const string DMAP_MSGID = "DMAPOutput";
        public const string DMAP_DONE = "DMAPDone";
        public static IntPtr com_hwndMsg;
        public static bool com_outputMsg;
#endif
    }

    //public struct MemInfo
    //{
    //    public string filebase;

    //    public int total;
    //    public int assetTotals;

    //    // memory manager totals
    //    public int memoryManagerTotal;

    //    // subsystem totals
    //    public int gameSubsystemTotal;
    //    public int renderSubsystemTotal;

    //    // asset totals
    //    public int imageAssetsTotal;
    //    public int modelAssetsTotal;
    //    public int soundAssetsTotal;
    //}
}