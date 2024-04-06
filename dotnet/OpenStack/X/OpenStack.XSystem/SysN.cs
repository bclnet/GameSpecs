using System.Collections.Generic;
using System.IO;

namespace System.NumericsX.OpenStack.System
{
    public static partial class SysN
    {
        public static void Init() => throw new NotImplementedException();
        public static void Shutdown() => throw new NotImplementedException();
        public static void Error(string fmt, params object[] args) => throw new NotImplementedException();

        public static void Quit() => throw new NotImplementedException();

        // note that this isn't journaled...
        public static string GetClipboardData() => throw new NotImplementedException();
        public static void SetClipboardData(string s) => throw new NotImplementedException();

        // will go to the various text consoles
        // NOT thread safe - never use in the async paths
        public static void Printf(string fmt, params object[] args) => throw new NotImplementedException();

        // guaranteed to be thread-safe
        public static void DebugPrintf(string fmt, params object[] args) => throw new NotImplementedException();

        // allow game to yield CPU time
        // NOTE: due to SDL_TIMESLICE this is very bad portability karma, and should be completely removed
        public static void Sleep(int msec) => throw new NotImplementedException();

        // Sys_Milliseconds should only be used for profiling purposes,
        // any game related timing information should come from event timestamps
        public static uint Milliseconds => throw new NotImplementedException();

        // returns amount of system ram
        public static int GetSystemRam() => throw new NotImplementedException();

        // returns amount of drive space in path
        public static int GetDriveFreeSpace(string path) => throw new NotImplementedException();

        // lock and unlock memory
        public static unsafe bool LockMemory(void* ptr, int bytes) => throw new NotImplementedException();
        public static unsafe bool UnlockMemory(void* ptr, int bytes) => throw new NotImplementedException();

        // set amount of physical work memory
        public static void SetPhysicalWorkMemory(int minBytes, int maxBytes) => throw new NotImplementedException();

        // DLL loading, the path should be a fully qualified OS path to the DLL file to be loaded
        public static IntPtr DLL_Load(string dllName) => throw new NotImplementedException();
        public static IntPtr DLL_GetProcAddress(IntPtr dllHandle, string procName) => throw new NotImplementedException();
        public static void DLL_Unload(IntPtr dllHandle) => throw new NotImplementedException();

        // event generation
        //public static void GenerateEvents() => throw new NotImplementedException();
        //public static SysEvent GetEvent() => throw new NotImplementedException();
        //public static void ClearEvents() => throw new NotImplementedException();
        public static string ConsoleInput() => throw new NotImplementedException();

        // input is tied to windows, so it needs to be started up and shut down whenever the main window is recreated
        //public static void InitInput() => throw new NotImplementedException();
        //public static void ShutdownInput() => throw new NotImplementedException();
        //public static void InitScanTable() => throw new NotImplementedException();
        //public static sbyte GetConsoleKey(bool shifted) => throw new NotImplementedException();
        //// map a scancode key to a char. does nothing on win32, as SE_KEY == SE_CHAR there on other OSes, consider the keyboard mapping
        //public static char MapCharForKey(int key) => throw new NotImplementedException();

        //// keyboard input polling
        //public static int PollKeyboardInputEvents() => throw new NotImplementedException();
        //public static int ReturnKeyboardInputEvent(int n, out int ch, out bool state) => throw new NotImplementedException();
        //public static void EndKeyboardInputEvents() => throw new NotImplementedException();

        //// mouse input polling
        //public static int PollMouseInputEvents() => throw new NotImplementedException();
        //public static int ReturnMouseInputEvent(int n, out int action, out int value) => throw new NotImplementedException();
        //public static void EndMouseInputEvents() => throw new NotImplementedException();

        //// when the console is down, or the game is about to perform a lengthy operation like map loading, the system can release the mouse cursor when in windowed mode
        //public static void GrabMouseCursor(bool grabIt) => throw new NotImplementedException();

        //public static void AddMouseMoveEvent(int dx, int dy) => throw new NotImplementedException();
        //public static void AddMouseButtonEvent(int button, bool pressed) => throw new NotImplementedException();
        //public static void AddKeyEvent(int key, bool pressed) => throw new NotImplementedException();

        public static void ShowWindow(bool show) => throw new NotImplementedException();
        public static bool IsWindowVisible() => throw new NotImplementedException();
        public static void ShowConsole(int visLevel, bool quitOnClose) => throw new NotImplementedException();

        public static void Mkdir(string path) => throw new NotImplementedException();

        public static DateTime FileTimeStamp(FileInfo fp) => throw new NotImplementedException();

        // NOTE: do we need to guarantee the same output on all platforms?
        public static string TimeStampToStr(DateTime timeStamp) => throw new NotImplementedException();

        public static bool GetPath(PATH type, out string path) => throw new NotImplementedException();

        // use fs_debug to verbose Sys_ListFiles
        // returns -1 if directory was not found (the list is cleared)
        public static int ListFiles(string directory, string extension, List<string> list) => throw new NotImplementedException();
    }
}