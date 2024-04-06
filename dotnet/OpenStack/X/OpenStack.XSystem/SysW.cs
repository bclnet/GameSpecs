using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.OpenStack.System.NativeW;

namespace System.NumericsX.OpenStack.System
{
    public static partial class SysW
    {
        static IntPtr hWnd;
        internal static IntPtr hInstance;

        internal static readonly CVar win_outputDebugString = new("win_outputDebugString", "0", CVAR.SYSTEM | CVAR.BOOL, "");
        internal static readonly CVar win_outputEditString = new("win_outputEditString", "1", CVAR.SYSTEM | CVAR.BOOL, "");
        internal static readonly CVar win_viewlog = new("win_viewlog", "0", CVAR.SYSTEM | CVAR.INTEGER, "");

        internal static CVar com_skipRenderer;
        internal static CVar net_serverDedicated;

        // The cvar system must already be setup
        public static void Init()
        {
            //CoInitialize(null);

            // make sure the timer is high precision, otherwise NT gets 18ms resolution
            TimeBeginPeriod(1);

#if DEBUG
            //cmdSystem.AddCommand("createResourceIDs", CreateResourceIDs_f, CMD_FL_TOOL, "assigns resource IDs in _resouce.h files");
#endif

            // Windows version
            var osversion = new OSVERSIONINFOEX
            {
                dwOSVersionInfoSize = Marshal.SizeOf<OSVERSIONINFOEX>(),
            };
            if (!GetVersionEx(ref osversion)) Error("Couldn't get OS info");
            if (osversion.dwMajorVersion < 4) Error($"{PlatformW.GAME_NAME} requires Windows version 4 (NT) or greater");
            if (osversion.dwPlatformId == VER_PLATFORM_WIN32s) Error($"{PlatformW.GAME_NAME} doesn't run on Win32s");

            common.Printf($"{SystemRam} MB System Memory\n");
        }

        public static void Shutdown()
        {
            //CoUninitialize();
        }

        public static void Error(string fmt, params object[] args)
        {
            if (fmt != null) fmt = string.Format(fmt, args);

            Console.Write(fmt);

            ConW.AppendText(fmt);
            ConW.AppendText("\n");

            ConW.SetErrorText(fmt);
            ShowConsole(1, true);

            TimeEndPeriod(1);

            ShutdownInput();

            //GLimp_Shutdown();

            // wait for the user to quit
            while (true)
            {
                if (GetMessage(out var msg, IntPtr.Zero, 0, 0) == 0) { common.Quit(); break; }
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }

            ConW.DestroyConsole();

            Environment.Exit(1);
        }

        public static void Quit()
        {
            TimeEndPeriod(1);
            ShutdownInput();
            ConW.DestroyConsole();
            Environment.Exit(0);
        }

        // note that this isn't journaled...
        public unsafe static string GetClipboardData()
        {
            string data = null; IntPtr cliptext;

            if (OpenClipboard(IntPtr.Zero))
            {
                IntPtr hClipboardData;
                if ((hClipboardData = NativeW.GetClipboardData(CF_TEXT)) != IntPtr.Zero)
                    if ((cliptext = GlobalLock(hClipboardData)) != IntPtr.Zero)
                    {
                        data = Marshal.PtrToStringAnsi(cliptext, (int)GlobalSize(hClipboardData));
                        GlobalUnlock(hClipboardData);

                        //data.Split(new[] { '\n', '\r', '\b' });
                    }
                CloseClipboard();
            }
            return data;
        }

        public unsafe static void SetClipboardData(string s)
        {
            IntPtr HMem, PMem; byte[] sBytes = Encoding.ASCII.GetBytes($"{s}\0");

            // allocate memory block
            //HMem = GlobalAlloc(GMEM_MOVEABLE | GMEM_DDESHARE, (UIntPtr)(s != null ? s.Length + 1 : 0));
            HMem = Marshal.AllocHGlobal(sBytes.Length);
            if (HMem == IntPtr.Zero) return;
            // lock allocated memory and obtain a pointer
            PMem = GlobalLock(HMem);
            if (PMem == IntPtr.Zero) return;
            // copy text into allocated memory block
            //lstrcpy(PMem, s);
            Marshal.Copy(sBytes, 0, PMem, sBytes.Length);
            // unlock allocated memory
            GlobalUnlock(HMem);
            // open Clipboard
            if (!OpenClipboard(IntPtr.Zero)) { /*GlobalFree(HMem);*/ Marshal.FreeHGlobal(HMem); return; }
            // remove current Clipboard contents
            EmptyClipboard();
            // supply the memory handle to the Clipboard
            NativeW.SetClipboardData(CF_TEXT, HMem);
            // close Clipboard
            CloseClipboard();
        }

        // will go to the various text consoles
        // NOT thread safe - never use in the async paths
        public static void Printf(string fmt, params object[] args)
        {
            if (args != null) fmt = string.Format(fmt, args);
            Console.Write(fmt);
            if (win_outputDebugString.Bool) OutputDebugString(fmt);
            if (win_outputEditString.Bool) ConW.AppendText(fmt);
        }

        // guaranteed to be thread-safe
        public static void DebugPrintf(string fmt, params object[] args)
        {
            if (args != null) fmt = string.Format(fmt, args);
            Console.Write(fmt);
            OutputDebugString(fmt);
        }

        // allow game to yield CPU time
        // NOTE: due to SDL_TIMESLICE this is very bad portability karma, and should be completely removed
        public static void Sleep(int msec) => throw new NotImplementedException();

        // Sys_Milliseconds should only be used for profiling purposes,
        // any game related timing information should come from event timestamps
        public static int Milliseconds => throw new NotImplementedException();

        // returns amount of physical memory in MB
        // returns amount of system ram
        public static unsafe int SystemRam
        {
            get
            {
                var statex = new MEMORYSTATUSEX { dwLength = (uint)sizeof(MEMORYSTATUSEX) };
                GlobalMemoryStatusEx(ref statex);
                var physRam = (int)(statex.ullTotalPhys / (1024 * 1024));
                physRam = (physRam + 8) & ~15;
                return physRam;
            }
        }

        // returns amount of drive space in path
        // returns in megabytes
        public static int GetDriveFreeSpace(string path)
             => GetDiskFreeSpaceEx(path, out var lpFreeBytesAvailable, out var _, out var _)
                ? (int)(lpFreeBytesAvailable / (1024.0 * 1024.0))
                : 0;

        // lock and unlock memory
        public static unsafe bool LockMemory(void* ptr, int bytes)
            => VirtualLock((IntPtr)ptr, (IntPtr)bytes);

        public static unsafe bool UnlockMemory(void* ptr, int bytes)
            => VirtualUnlock((IntPtr)ptr, (IntPtr)bytes);

        // set amount of physical work memory
        public static void SetPhysicalWorkMemory(int minBytes, int maxBytes)
            => SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, (UIntPtr)minBytes, (UIntPtr)maxBytes);

        // DLL loading, the path should be a fully qualified OS path to the DLL file to be loaded
        public static IntPtr DLL_Load(string dllName)
        {
            var libHandle = LoadLibrary(dllName);
            if (libHandle != IntPtr.Zero)
            {
                // since we can't have LoadLibrary load only from the specified path, check it did the right thing
                GetModuleFileName(libHandle, out var loadedPathBuf, 1000);
                var loadedPath = loadedPathBuf?.ToString();
                if (string.Equals(dllName, loadedPath, StringComparison.OrdinalIgnoreCase))
                {
                    SysW.Printf($"ERROR: LoadLibrary '{dllName}' wants to load '{loadedPath}'\n");
                    DLL_Unload(libHandle);
                    return IntPtr.Zero;
                }
            }
            return libHandle;
        }
        public static IntPtr DLL_GetProcAddress(IntPtr dllHandle, string procName)
            => GetProcAddress(dllHandle, procName);

        public static void DLL_Unload(IntPtr dllHandle)
        {
            if (dllHandle == IntPtr.Zero) return;
            if (!FreeLibrary(dllHandle))
            {
                var lastError = Marshal.GetLastWin32Error();
                var msg = new Win32Exception(lastError).Message;
                SysW.Error($"Sys_DLL_Unload: FreeLibrary failed - {msg} ({lastError})");
            }
        }

        // event generation
        public static string ConsoleInput() => throw new NotImplementedException();

        public static void ShowWindow(bool show)
            => NativeW.ShowWindow(hWnd, show ? ShowWindowCmdShow.SW_SHOW : ShowWindowCmdShow.SW_HIDE);
        public static bool IsWindowVisible
            => NativeW.IsWindowVisible(hWnd);
        public static void ShowConsole(int visLevel, bool quitOnClose) => throw new NotImplementedException();

        public static void Mkdir(string path)
            => Directory.CreateDirectory(path);

        public static DateTime FileTimeStamp(FileInfo fp)
            => fp.LastWriteTime;

        // NOTE: do we need to guarantee the same output on all platforms?
        public static string TimeStampToStr(DateTime timeStamp) => throw new NotImplementedException();

        public static bool GetPath(PATH type, out string path)
        {
            string GetRegistryPath(string subkey, string name)
            {
                var sam = 0x0001 | (Environment.Is64BitProcess ? 0x0200 : 0);
                if (RegOpenKeyEx((UIntPtr)HKEY_LOCAL_MACHINE, subkey, 0, sam, out var res) != 0) return null;
                var w = new StringBuilder(); var len = 0U;
                if (RegQueryValueEx(res, name, 0, out var type, w, ref len) != 0) { RegCloseKey(res); return null; }
                RegCloseKey(res);
                return type != 1UL ? string.Empty : w.ToString();
            }

            string GetHomeDir()
                => $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/My Games/dhewm3";

            string s, buf; FileInfo st;

            switch (type)
            {
                case PATH.BASE:
                    // try <path to exe>/base first
                    if (GetPath(PATH.EXE, out path))
                    {
                        path = Path.GetDirectoryName(path);

                        s = Path.Combine(path, Config.BASE_GAMEDIR);
                        st = new FileInfo(s);
                        if (st.Exists && (st.Attributes & FileAttributes.Directory) != 0) { common.Warning($"using path of executable: {path}"); return true; }
                        else
                        {
                            s = $"{path}/demo/demo00.pk4";
                            st = new FileInfo(s);
                            if (st.Exists && (st.Attributes & FileAttributes.Directory) == 0) { common.Warning($"using path of executable (seems to contain demo game data): {path}"); return true; }
                        }

                        common.Warning($"base path '{s}' does not exist");
                    }

                    // Note: apparently there is no registry entry for the Doom 3 Demo

                    // fallback to vanilla doom3 cd install
                    buf = GetRegistryPath("SOFTWARE\\id\\Doom 3", "InstallPath");
                    if (buf != null) { path = buf; return true; }

                    // fallback to steam doom3 install
                    buf = GetRegistryPath("SOFTWARE\\Valve\\Steam", "InstallPath");
                    if (buf != null)
                    {
                        path = Path.Combine(buf, "steamapps\\common\\doom 3");
                        st = new FileInfo(path);
                        if (st.Exists && (st.Attributes & FileAttributes.Directory) != 0) return true;
                    }

                    common.Warning("vanilla doom3 path not found either");
                    return false;

                case PATH.CONFIG:
                case PATH.SAVE:
                    buf = GetHomeDir();
                    if (buf == null) { Error("ERROR: Couldn't get dir to home path"); path = default; return false; }

                    path = buf;
                    return true;

                case PATH.EXE:
                    GetModuleFileName(IntPtr.Zero, out var b, 1000);
                    path = PathX.BackSlashesToSlashes(b.ToString());
                    return true;
            }

            path = default;
            return false;
        }

        // use fs_debug to verbose Sys_ListFiles
        // returns -1 if directory was not found (the list is cleared)
        public static int ListFiles(string directory, string extension, List<string> list)
        {
            if (extension == null) extension = "*";

            // passing a slash as extension will find directories
            bool flag;
            if (extension[0] == '/' && extension[1] == 0) { extension = "*"; flag = false; }
            else flag = true;

            // search
            list.Clear();
            list.AddRange(flag
                ? Directory.GetFiles(directory, extension)
                : Directory.GetFileSystemEntries(directory, extension));
            return list.Count == 0 ? -1 : list.Count;
        }

        static void Win_Frame()
        {
            // if "viewlog" has been modified, show or hide the log console
            if (win_viewlog.IsModified)
            {
                if (!com_skipRenderer.Bool && net_serverDedicated.Integer != 1) ShowConsole(win_viewlog.Integer, false);
                win_viewlog.ClearModified();
            }
        }

        static void SetHighDPIMode()
            => SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.D3_PROCESS_PER_MONITOR_DPI_AWARE);

        public static int Main(string[] args)
        {
            //var hcurSave = SetCursor(LoadCursor(0, IDC_WAIT));

#if !ID_DEDICATED
            // tell windows we're high dpi aware, otherwise display scaling screws up the game
            SetHighDPIMode();
#endif

            SetPhysicalWorkMemory(192 << 20, 1024 << 20);

            hInstance = GetModuleHandle(null);

            // done before Com/Sys_Init since we need this for error output
            ConW.CreateConsole();

            // no abort/retry/fail errors
            SetErrorMode(ErrorModes.SEM_FAILCRITICALERRORS);

#if DEBUG
            // disable the painfully slow MS heap check every 1024 allocs
            //_CrtSetDbgFlag(0);
#endif

            if (args.Length > 1) common.Init(args.Length - 1, args.AsSpan(1));
            else common.Init(0, null);

            // hide or show the early console as necessary
            if (win_viewlog.Integer != 0 || com_skipRenderer.Bool || net_serverDedicated.Integer != 0) ShowConsole(1, true);
            else ShowConsole(0, false);

#if SET_THREAD_AFFINITY
            // give the main thread an affinity for the first cpu
            SetThreadAffinityMask(GetCurrentThread(), 1);
#endif

            //::SetCursor(hcurSave); // DG: I think SDL handles the cursor fine..

            // Launch the script debugger
            if (Environment.CommandLine.Contains("+debugger"))
            {
                //DebuggerClientInit(lpCmdLine);
                return 0;
            }

            //::SetFocus(SysX.hWnd); // DG: let SDL handle focus, otherwise input is fucked up! (#100)

            // main game loop
            while (true)
            {
#if ID_DEDICATED
                // Since this is a Dedicated Server, process all Windowing Messages Now.
                while (PeekMessage(out var msg, IntPtr.Zero, 0, 0, PM_REMOVE)) { TranslateMessage(ref msg); DispatchMessage(ref msg); }

                // Give the OS a little time to recuperate.
                Sleep(10);
#endif

                Win_Frame();

                // run the game
                common.Frame();
            }
        }
    }
}