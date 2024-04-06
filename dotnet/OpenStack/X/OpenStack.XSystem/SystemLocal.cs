namespace System.NumericsX.OpenStack.System
{
    public class SystemLocal : ISystem
    {
        //idSysLocal sysLocal;
        //idSys sys = &sysLocal;

        static readonly string[] sysLanguageNames = {
            "english", "spanish", "italian", "german", "french", "russian",
            "polish", "korean", "japanese", "chinese", null
        };

        static CVar sys_lang = new("sys_lang", "english", CVAR.SYSTEM | CVAR.ARCHIVE, "", sysLanguageNames, CmdArgs.ArgCompletion_String(sysLanguageNames));

        public void DebugPrintf(string fmt, params object[] args)
            => SysW.DebugPrintf(fmt, args);

        public uint Milliseconds
            => (uint)SysW.Milliseconds;

        public unsafe bool LockMemory(void* ptr, int bytes)
            => SysW.LockMemory(ptr, bytes);

        public unsafe bool UnlockMemory(void* ptr, int bytes)
            => SysW.UnlockMemory(ptr, bytes);

        public IntPtr DLL_Load(string dllName)
            => SysW.DLL_Load(dllName);

        public IntPtr DLL_GetProcAddress(IntPtr dllHandle, string procName)
            => SysW.DLL_GetProcAddress(dllHandle, procName);

        public void DLL_Unload(IntPtr dllHandle)
            => SysW.DLL_Unload(dllHandle);

        public string DLL_GetFileName(string baseName)
            => $"{baseName}{Config.BUILD_LIBRARY_SUFFIX}";

        public SysEvent GenerateMouseButtonEvent(int button, bool down)
            => new SysEvent
            {
                evType = SE.KEY,
                evValue = (int)Key.K_MOUSE1 + button - 1,
                evValue2 = down ? 1 : 0,
                evPtrLength = 0,
                evPtr = IntPtr.Zero
            };

        public SysEvent GenerateMouseMoveEvent(int deltax, int deltay)
            => new SysEvent
            {
                evType = SE.MOUSE,
                evValue = deltax,
                evValue2 = deltay,
                evPtrLength = 0,
                evPtr = IntPtr.Zero
            };

        static bool OpenURL_doexit_spamguard = false;
        public void OpenURL(string url, bool quit)
        {
            //if (OpenURL_doexit_spamguard) { DPrintf($"OpenURL: already in an exit sequence, ignoring {url}\n"); return; }

            //Lib.Printf($"Open URL: {url}\n");

            //if (ShellExecute(IntPtr.Zero, "open", url, null, null, SW_RESTORE) == IntPtr.Zero) { Lib.Error($"Could not open url: '{url}'"); return; }

            //var wnd = GetForegroundWindow();
            //if (wnd != IntPtr.Zero) ShowWindow(wnd, SW_MAXIMIZE);

            //if (quit)
            //{
            //    OpenURL_doexit_spamguard = true;
            //    cmdSystem.BufferCommandText(CMD_EXEC.APPEND, "quit\n");
            //}
        }

        public void StartProcess(string exeName, bool quit)
        {
            //TCHAR szPathOrig[_MAX_PATH];
            //STARTUPINFO si;
            //PROCESS_INFORMATION pi;

            //ZeroMemory(&si, sizeof(si));
            //si.cb = sizeof(si);

            //strncpy(szPathOrig, exePath, _MAX_PATH);

            //if (!CreateProcess(NULL, szPathOrig, NULL, NULL, FALSE, 0, NULL, NULL, &si, &pi)) { Lib.Error($"Could not start process: '{szPathOrig}'", ); return; }

            //if (quit) cmdSystem.BufferCommandText(CMD_EXEC.APPEND, "quit\n");
        }
    }

    partial class SysEx
    {
        string TimeStampToStr(DateTime timeStamp)
        {
            throw new NotImplementedException();
            //static char timeString[MAX_STRING_CHARS];
            //timeString[0] = '\0';

            //tm* time = localtime(&timeStamp);
            //idStr o;

            //idStr lang = cvarSystem->GetCVarString("sys_lang");
            //if (lang.Icmp("english") == 0)
            //{
            //    // english gets "month/day/year  hour:min" + "am" or "pm"
            //    o = va("%02d", time->tm_mon + 1);
            //    o += "/";
            //    o += va("%02d", time->tm_mday);
            //    o += "/";
            //    o += va("%d", time->tm_year + 1900);
            //    o += "\t";
            //    if (time->tm_hour > 12)
            //    {
            //        o += va("%02d", time->tm_hour - 12);
            //    }
            //    else if (time->tm_hour == 0)
            //    {
            //        o += "12";
            //    }
            //    else
            //    {
            //        o += va("%02d", time->tm_hour);
            //    }
            //    o += ":";
            //    o += va("%02d", time->tm_min);
            //    if (time->tm_hour >= 12)
            //    {
            //        o += "pm";
            //    }
            //    else
            //    {
            //        o += "am";
            //    }
            //}
            //else
            //{
            //    // europeans get "day/month/year  24hour:min"
            //    o = va("%02d", time->tm_mday);
            //    o += "/";
            //    o += va("%02d", time->tm_mon + 1);
            //    o += "/";
            //    o += va("%d", time->tm_year + 1900);
            //    o += "\t";
            //    o += va("%02d", time->tm_hour);
            //    o += ":";
            //    o += va("%02d", time->tm_min);
            //}
            //string.Copynz(timeString, oo, sizeof(timeString));

            //return timeString;
        }
    }
}



