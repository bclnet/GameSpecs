using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace System.NumericsX.OpenStack.System
{
    // http://pinvoke.net/default.aspx/user32/OpenClipboard.html
    public static class NativeW
    {
        public static ushort LOWORD(uint l) => (ushort)(l & 0xffff);
        public static ushort LOWORD(IntPtr l) => (ushort)((uint)l & 0xffff);
        public static ushort HIWORD(uint l) => (ushort)((l >> 16) & 0xffff);
        public static ushort HIWORD(IntPtr l) => (ushort)(((uint)l >> 16) & 0xffff);
        public static uint RGB(byte r, byte g, byte b) => (uint)(r | g << 8 | b << 16);

        //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);
        public delegate void TimerProc(IntPtr hWnd, uint uMsg, IntPtr nIDEvent, uint dwTime);

        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")] public static extern uint TimeBeginPeriod(uint uMilliseconds);
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")] public static extern uint TimeEndPeriod(uint uMilliseconds);

        [DllImport("SHCore.dll", SetLastError = true)] public static extern bool SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness);

        [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, ShowWindowCmdShow nCmdShow);
        public enum ShowWindowCmdShow : int
        {
            // ShowWindow() Commands
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_FORCEMINIMIZE = 11,
            SW_MAX = 11,
            // Old ShowWindow() Commands
            HIDE_WINDOW = 0,
            SHOW_OPENWINDOW = 1,
            SHOW_ICONWINDOW = 2,
            SHOW_FULLSCREEN = 3,
            SHOW_OPENNOACTIVATE = 4,
            // Identifiers for the WM_SHOWWINDOW message
            SW_PARENTCLOSING = 1,
            SW_OTHERZOOM = 2,
            SW_PARENTOPENING = 3,
            SW_OTHERUNZOOM = 4,
        }
        [DllImport("user32.dll", SetLastError = true)] public static extern int CloseWindow(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] public static extern bool DestroyWindow(IntPtr hwnd);
        [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")] public static extern IntPtr LoadIcon(IntPtr hInstance, int lpIconName);
        [DllImport("user32.dll")] public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll", SetLastError = true)] public static extern bool OpenClipboard(IntPtr hWndNewOwner);
        [DllImport("user32.dll", SetLastError = true)] public static extern bool CloseClipboard();
        [DllImport("user32.dll")] public static extern IntPtr GetClipboardData(uint uFormat);
        [DllImport("user32.dll")] public static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);
        [DllImport("user32.dll")] public static extern bool EmptyClipboard();

        [DllImport("user32.dll", SetLastError = true)] public static extern IntPtr SetFocus(IntPtr hWnd);
        [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")] public static extern bool UpdateWindow(IntPtr hWnd);
        [DllImport("user32.dll")] public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")] static extern int SetWindowLong32(IntPtr hWnd, WindowFieldOffset nIndex, int dwNewLong);
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")] static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, WindowFieldOffset nIndex, IntPtr dwNewLong);
        public static IntPtr SetWindowLongPtr(IntPtr hWnd, WindowFieldOffset nIndex, IntPtr dwNewLong)
            => IntPtr.Size == 8
                ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
                : new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        public enum WindowFieldOffset : int
        {
            GWL_WNDPROC = -4,
            GWL_HINSTANCE = -6,
            GWL_HWNDPARENT = -8,
            GWL_STYLE = -16,
            GWL_EXSTYLE = -20,
            GWL_USERDATA = -21,
            GWL_ID = -12,
            GWLP_WNDPROC = -4,
            GWLP_HINSTANCE = -6,
            GWLP_HWNDPARENT = -8,
            GWLP_USERDATA = -21,
            GWLP_ID = -12,
        }

        [DllImport("user32.dll")] public static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
        [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] public static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
        [DllImport("user32.dll")] public static extern bool TranslateMessage(ref MSG lpMsg);
        [DllImport("user32.dll")] public static extern IntPtr DispatchMessage(ref MSG lpMsg);

        [DllImport("user32.dll")] public static extern void PostQuitMessage(int nExitCode);
        [DllImport("user32.dll")] public static extern int SendMessage(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")] public static extern int SendMessage(IntPtr hWnd, uint uMsg, int wParam, int lParam);
        [DllImport("user32.dll")] public static extern int SendMessage(IntPtr hWnd, uint uMsg, int wParam, ref int lParam);
        [DllImport("user32.dll")] public static extern int SendMessage(IntPtr hWnd, uint uMsg, int wParam, string lParam);
        public enum EditControlMessage : uint
        {
            EM_GETSEL = 0x00B0,
            EM_SETSEL = 0x00B1,
            EM_GETRECT = 0x00B2,
            EM_SETRECT = 0x00B3,
            EM_SETRECTNP = 0x00B4,
            EM_SCROLL = 0x00B5,
            EM_LINESCROLL = 0x00B6,
            EM_SCROLLCARET = 0x00B7,
            EM_GETMODIFY = 0x00B8,
            EM_SETMODIFY = 0x00B9,
            EM_GETLINECOUNT = 0x00BA,
            EM_LINEINDEX = 0x00BB,
            EM_SETHANDLE = 0x00BC,
            EM_GETHANDLE = 0x00BD,
            EM_GETTHUMB = 0x00BE,
            EM_LINELENGTH = 0x00C1,
            EM_REPLACESEL = 0x00C2,
            EM_GETLINE = 0x00C4,
            EM_LIMITTEXT = 0x00C5,
            EM_CANUNDO = 0x00C6,
            EM_UNDO = 0x00C7,
            EM_FMTLINES = 0x00C8,
            EM_LINEFROMCHAR = 0x00C9,
            EM_SETTABSTOPS = 0x00CB,
            EM_SETPASSWORDCHAR = 0x00CC,
            EM_EMPTYUNDOBUFFER = 0x00CD,
            EM_GETFIRSTVISIBLELINE = 0x00CE,
            EM_SETREADONLY = 0x00CF,
            EM_SETWORDBREAKPROC = 0x00D0,
            EM_GETWORDBREAKPROC = 0x00D1,
            EM_GETPASSWORDCHAR = 0x00D2,
            EM_SETMARGINS = 0x00D3,
            EM_GETMARGINS = 0x00D4,
            EM_SETLIMITTEXT = EM_LIMITTEXT,
            EM_GETLIMITTEXT = 0x00D5,
            EM_POSFROMCHAR = 0x00D6,
            EM_CHARFROMPOS = 0x00D7,
            EM_SETIMESTATUS = 0x00D8,
            EM_GETIMESTATUS = 0x00D9,
            EM_ENABLEFEATURE = 0x00DA,
        }

        [DllImport("user32.dll")] public static extern IntPtr SetTimer(IntPtr hWnd, uint nIDEvent, uint uElapse, TimerProc lpTimerFunc);
        [DllImport("user32.dll")] public static extern ushort RegisterClassEx(ref WNDCLASSEX lpwcx); //[return: MarshalAs(UnmanagedType.U2)]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct WNDCLASSEX
        {
            [MarshalAs(UnmanagedType.U4)] public int cbSize;
            [MarshalAs(UnmanagedType.U4)] public int style;
            public IntPtr lpfnWndProc; // not WndProc
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }
        [DllImport("user32.dll", SetLastError = true)] public static extern IntPtr CreateWindowEx(WindowStyleEx dwExStyle, string lpClassName, string lpWindowName, WindowStyle dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);
        [DllImport("user32.dll", SetLastError = true)] public static extern IntPtr CreateWindowEx(WindowStyleEx dwExStyle, ushort regResult, string lpWindowName, WindowStyle dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);
        [DllImport("user32.dll")] public static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")] public static extern IntPtr CallWindowProc(WndProcDelegate lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")] public static extern bool AdjustWindowRectEx(ref RECT lpRect, WindowStyle dwStyle, bool bMenu, uint dwExStyle);
        [Flags]
        public enum WindowStyle : uint
        {
            // Window Styles
            WS_OVERLAPPED = 0x00000000,
            WS_POPUP = 0x80000000,
            WS_CHILD = 0x40000000,
            WS_MINIMIZE = 0x20000000,
            WS_VISIBLE = 0x10000000,
            WS_DISABLED = 0x08000000,
            WS_CLIPSIBLINGS = 0x04000000,
            WS_CLIPCHILDREN = 0x02000000,
            WS_MAXIMIZE = 0x01000000,
            WS_CAPTION = 0x00C00000,
            WS_BORDER = 0x00800000,
            WS_DLGFRAME = 0x00400000,
            WS_VSCROLL = 0x00200000,
            WS_HSCROLL = 0x00100000,
            WS_SYSMENU = 0x00080000,
            WS_THICKFRAME = 0x00040000,
            WS_GROUP = 0x00020000,
            WS_TABSTOP = 0x00010000,
            WS_MINIMIZEBOX = 0x00020000,
            WS_MAXIMIZEBOX = 0x00010000,
            WS_TILED = WS_OVERLAPPED,
            WS_ICONIC = WS_MINIMIZE,
            WS_SIZEBOX = WS_THICKFRAME,
            WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW,
            // Common Window Styles
            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
            WS_CHILDWINDOW = WS_CHILD,
            // Class Styles
            //CS_VREDRAW = 0x0001,
            //CS_HREDRAW = 0x0002,
            //CS_DBLCLKS = 0x0008,
            //CS_OWNDC = 0x0020,
            //CS_CLASSDC = 0x0040,
            //CS_PARENTDC = 0x0080,
            //CS_NOCLOSE = 0x0200,
            //CS_SAVEBITS = 0x0800,
            //CS_BYTEALIGNCLIENT = 0x1000,
            //CS_BYTEALIGNWINDOW = 0x2000,
            //CS_GLOBALCLASS = 0x4000,
            //CS_IME = 0x00010000,
            //CS_DROPSHADOW = 0x00020000,
            // Edit Control Styles
            ES_LEFT = 0x0000,
            ES_CENTER = 0x0001,
            ES_RIGHT = 0x0002,
            ES_MULTILINE = 0x0004,
            ES_UPPERCASE = 0x0008,
            ES_LOWERCASE = 0x0010,
            ES_PASSWORD = 0x0020,
            ES_AUTOVSCROLL = 0x0040,
            ES_AUTOHSCROLL = 0x0080,
            ES_NOHIDESEL = 0x0100,
            ES_OEMCONVERT = 0x0400,
            ES_READONLY = 0x0800,
            ES_WANTRETURN = 0x1000,
            ES_NUMBER = 0x2000,
            // WM_PRINT flags
            //PRF_CHECKVISIBLE = 0x00000001,
            //PRF_NONCLIENT = 0x00000002,
            //PRF_CLIENT = 0x00000004,
            //PRF_ERASEBKGND = 0x00000008,
            //PRF_CHILDREN = 0x00000010,
            //PRF_OWNED = 0x00000020,
            // 3D border styles
            //BDR_RAISEDOUTER = 0x0001,                         
            //BDR_SUNKENOUTER = 0x0002,                         
            //BDR_RAISEDINNER = 0x0004,                         
            //BDR_SUNKENINNER = 0x0008,                         
            //BDR_OUTER = (BDR_RAISEDOUTER | BDR_SUNKENOUTER),
            //BDR_INNER = (BDR_RAISEDINNER | BDR_SUNKENINNER),
            //BDR_RAISED = (BDR_RAISEDOUTER | BDR_RAISEDINNER),
            //BDR_SUNKEN = (BDR_SUNKENOUTER | BDR_SUNKENINNER),
            //EDGE_RAISED = BDR_RAISEDOUTER | BDR_RAISEDINNER,
            //EDGE_SUNKEN = BDR_SUNKENOUTER | BDR_SUNKENINNER,
            //EDGE_ETCHED = BDR_SUNKENOUTER | BDR_RAISEDINNER,
            //EDGE_BUMP = BDR_RAISEDOUTER | BDR_SUNKENINNER,
            // Border flags                                       
            //BF_LEFT = 0x0001,
            //BF_TOP = 0x0002,
            //BF_RIGHT = 0x0004,
            //BF_BOTTOM = 0x0008,
            //BF_TOPLEFT = BF_TOP | BF_LEFT,
            //BF_TOPRIGHT = BF_TOP | BF_RIGHT,
            //BF_BOTTOMLEFT = BF_BOTTOM | BF_LEFT,
            //BF_BOTTOMRIGHT = BF_BOTTOM | BF_RIGHT,
            //BF_RECT = BF_LEFT | BF_TOP | BF_RIGHT | BF_BOTTOM,
            //BF_DIAGONAL = 0x0010,
            //// For diagonal lines, the BF_RECT flags specify the end point of the vector bounded by the rectangle parameter.
            //BF_DIAGONAL_ENDTOPRIGHT = BF_DIAGONAL | BF_TOP | BF_RIGHT,
            //BF_DIAGONAL_ENDTOPLEFT = BF_DIAGONAL | BF_TOP | BF_LEFT,
            //BF_DIAGONAL_ENDBOTTOMLEFT = BF_DIAGONAL | BF_BOTTOM | BF_LEFT,
            //BF_DIAGONAL_ENDBOTTOMRIGHT = BF_DIAGONAL | BF_BOTTOM | BF_RIGHT,
            //BF_MIDDLE = 0x0800,
            //BF_SOFT = 0x1000,
            //BF_ADJUST = 0x2000,
            //BF_FLAT = 0x4000,
            //BF_MONO = 0x8000,
            // Button Control Styles
            BS_PUSHBUTTON = 0x00000000,
            BS_DEFPUSHBUTTON = 0x00000001,
            BS_CHECKBOX = 0x00000002,
            BS_AUTOCHECKBOX = 0x00000003,
            BS_RADIOBUTTON = 0x00000004,
            BS_3STATE = 0x00000005,
            BS_AUTO3STATE = 0x00000006,
            BS_GROUPBOX = 0x00000007,
            BS_USERBUTTON = 0x00000008,
            BS_AUTORADIOBUTTON = 0x00000009,
            BS_PUSHBOX = 0x0000000A,
            BS_OWNERDRAW = 0x0000000B,
            BS_TYPEMASK = 0x0000000F,
            BS_LEFTTEXT = 0x00000020,
            BS_TEXT = 0x00000000,
            BS_ICON = 0x00000040,
            BS_BITMAP = 0x00000080,
            BS_LEFT = 0x00000100,
            BS_RIGHT = 0x00000200,
            BS_CENTER = 0x00000300,
            BS_TOP = 0x00000400,
            BS_BOTTOM = 0x00000800,
            BS_VCENTER = 0x00000C00,
            BS_PUSHLIKE = 0x00001000,
            BS_MULTILINE = 0x00002000,
            BS_NOTIFY = 0x00004000,
            BS_FLAT = 0x00008000,
            BS_RIGHTBUTTON = BS_LEFTTEXT,

            // Static Control Constants
            SS_LEFT = 0x00000000,
            SS_CENTER = 0x00000001,
            SS_RIGHT = 0x00000002,
            SS_ICON = 0x00000003,
            SS_BLACKRECT = 0x00000004,
            SS_GRAYRECT = 0x00000005,
            SS_WHITERECT = 0x00000006,
            SS_BLACKFRAME = 0x00000007,
            SS_GRAYFRAME = 0x00000008,
            SS_WHITEFRAME = 0x00000009,
            SS_USERITEM = 0x0000000A,
            SS_SIMPLE = 0x0000000B,
            SS_LEFTNOWORDWRAP = 0x0000000C,
            SS_OWNERDRAW = 0x0000000D,
            SS_BITMAP = 0x0000000E,
            SS_ENHMETAFILE = 0x0000000F,
            SS_ETCHEDHORZ = 0x00000010,
            SS_ETCHEDVERT = 0x00000011,
            SS_ETCHEDFRAME = 0x00000012,
            SS_TYPEMASK = 0x0000001F,
            SS_REALSIZECONTROL = 0x00000040,
            SS_NOPREFIX = 0x00000080,
            SS_NOTIFY = 0x00000100,
            SS_CENTERIMAGE = 0x00000200,
            SS_RIGHTJUST = 0x00000400,
            SS_REALSIZEIMAGE = 0x00000800,
            SS_SUNKEN = 0x00001000,
            SS_EDITCONTROL = 0x00002000,
            SS_ENDELLIPSIS = 0x00004000,
            SS_PATHELLIPSIS = 0x00008000,
            SS_WORDELLIPSIS = 0x0000C000,
            SS_ELLIPSISMASK = 0x0000C000,
        }
        [Flags]
        public enum WindowStyleEx : uint
        {
            WS_EX_DLGMODALFRAME = 0x00000001,
            WS_EX_NOPARENTNOTIFY = 0x00000004,
            WS_EX_TOPMOST = 0x00000008,
            WS_EX_ACCEPTFILES = 0x00000010,
            WS_EX_TRANSPARENT = 0x00000020,
            WS_EX_MDICHILD = 0x00000040,
            WS_EX_TOOLWINDOW = 0x00000080,
            WS_EX_WINDOWEDGE = 0x00000100,
            WS_EX_CLIENTEDGE = 0x00000200,
            WS_EX_CONTEXTHELP = 0x00000400,
            WS_EX_RIGHT = 0x00001000,
            WS_EX_LEFT = 0x00000000,
            WS_EX_RTLREADING = 0x00002000,
            WS_EX_LTRREADING = 0x00000000,
            WS_EX_LEFTSCROLLBAR = 0x00004000,
            WS_EX_RIGHTSCROLLBAR = 0x00000000,
            WS_EX_CONTROLPARENT = 0x00010000,
            WS_EX_STATICEDGE = 0x00020000,
            WS_EX_APPWINDOW = 0x00040000,
            WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,
            WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,
            WS_EX_LAYERED = 0x00080000,
            WS_EX_NOINHERITLAYOUT = 0x00100000,
            WS_EX_NOREDIRECTIONBITMAP = 0x00200000,
            WS_EX_LAYOUTRTL = 0x00400000,
            WS_EX_COMPOSITED = 0x02000000,
            WS_EX_NOACTIVATE = 0x08000000,
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)] public static extern bool SetWindowText(IntPtr hwnd, string lpString);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)] public static extern bool SetWindowText(IntPtr hwnd, StringBuilder lpString);

        [DllImport("user32.dll", SetLastError = true)] public static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")] public static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("user32.dll", SetLastError = false)] public static extern IntPtr GetDesktopWindow();

        [DllImport("gdi32.dll")] public static extern uint SetBkColor(IntPtr hdc, uint crColor);
        [DllImport("gdi32.dll")] public static extern uint SetTextColor(IntPtr hdc, uint crColor);
        [DllImport("gdi32.dll")] public static extern IntPtr CreateSolidBrush(uint crColor);
        [DllImport("gdi32.dll")] public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateFont(int nHeight, int nWidth, int nEscapement, int nOrientation, FontWeight fnWeight, FontItalic fdwItalic, FontUnderline fdwUnderline, FontStrikeOut fdwStrikeOut,
            FontCharSet fdwCharSet, FontOutputPrecision fdwOutputPrecision, FontClipPrecision fdwClipPrecision, FontQuality fdwQuality, FontPitchAndFamily fdwPitchAndFamily, string lpszFace);
        public enum FontWeight : int
        {
            FW_DONTCARE = 0,
            FW_THIN = 100,
            FW_EXTRALIGHT = 200,
            FW_LIGHT = 300,
            FW_NORMAL = 400,
            FW_MEDIUM = 500,
            FW_SEMIBOLD = 600,
            FW_BOLD = 700,
            FW_EXTRABOLD = 800,
            FW_HEAVY = 900,
            FW_ULTRALIGHT = FW_EXTRALIGHT,
            FW_REGULAR = FW_NORMAL,
            FW_DEMIBOLD = FW_SEMIBOLD,
            FW_ULTRABOLD = FW_EXTRABOLD,
            FW_BLACK = FW_HEAVY,
        }
        public enum FontItalic : uint
        {
        }
        public enum FontUnderline : uint
        {
        }
        public enum FontStrikeOut : uint
        {
            OUT_DEFAULT_PRECIS = 0,
            OUT_STRING_PRECIS = 1,
            OUT_CHARACTER_PRECIS = 2,
            OUT_STROKE_PRECIS = 3,
            OUT_TT_PRECIS = 4,
            OUT_DEVICE_PRECIS = 5,
            OUT_RASTER_PRECIS = 6,
            OUT_TT_ONLY_PRECIS = 7,
            OUT_OUTLINE_PRECIS = 8,
            OUT_SCREEN_OUTLINE_PRECIS = 9,
            OUT_PS_ONLY_PRECIS = 10,
        }
        public enum FontCharSet : uint
        {
            ANSI_CHARSET = 0,
            DEFAULT_CHARSET = 1,
            SYMBOL_CHARSET = 2,
            SHIFTJIS_CHARSET = 128,
            HANGEUL_CHARSET = 129,
            HANGUL_CHARSET = 129,
            GB2312_CHARSET = 134,
            CHINESEBIG5_CHARSET = 136,
            OEM_CHARSET = 255,
            JOHAB_CHARSET = 130,
            HEBREW_CHARSET = 177,
            ARABIC_CHARSET = 178,
            GREEK_CHARSET = 161,
            TURKISH_CHARSET = 162,
            VIETNAMESE_CHARSET = 163,
            THAI_CHARSET = 222,
            EASTEUROPE_CHARSET = 238,
            RUSSIAN_CHARSET = 204,
            MAC_CHARSET = 77,
            BALTIC_CHARSET = 186,
        }
        public enum FontOutputPrecision : uint
        {
            OUT_DEFAULT_PRECIS = 0,
            OUT_STRING_PRECIS = 1,
            OUT_CHARACTER_PRECIS = 2,
            OUT_STROKE_PRECIS = 3,
            OUT_TT_PRECIS = 4,
            OUT_DEVICE_PRECIS = 5,
            OUT_RASTER_PRECIS = 6,
            OUT_TT_ONLY_PRECIS = 7,
            OUT_OUTLINE_PRECIS = 8,
            OUT_SCREEN_OUTLINE_PRECIS = 9,
            OUT_PS_ONLY_PRECIS = 10,
        }
        public enum FontClipPrecision : uint
        {
            CLIP_DEFAULT_PRECIS = 0,
            CLIP_CHARACTER_PRECIS = 1,
            CLIP_STROKE_PRECIS = 2,
            CLIP_MASK = 0xf,
            CLIP_LH_ANGLES = 1 << 4,
            CLIP_TT_ALWAYS = 2 << 4,
            CLIP_DFA_DISABLE = 4 << 4,
            CLIP_EMBEDDED = 8 << 4,
        }
        public enum FontQuality : uint
        {
            DEFAULT_QUALITY = 0,
            DRAFT_QUALITY = 1,
            PROOF_QUALITY = 2,
            NONANTIALIASED_QUALITY = 3,
            ANTIALIASED_QUALITY = 4,
            CLEARTYPE_QUALITY = 5,
            CLEARTYPE_NATURAL_QUALITY = 6,
        }
        [Flags]
        public enum FontPitchAndFamily : uint
        {
            DEFAULT_PITCH = 0,
            FIXED_PITCH = 1,
            VARIABLE_PITCH = 2,
            MONO_FONT = 8,
            // Font Families
            FF_DONTCARE = 0 << 4,
            FF_ROMAN = 1 << 4,
            FF_SWISS = 2 << 4,
            FF_MODERN = 3 << 4,
            FF_SCRIPT = 4 << 4,
            FF_DECORATIVE = 5 << 4,
        }
        public enum FontStyle : uint
        {
            FS_LATIN1 = 0x00000001,
            FS_LATIN2 = 0x00000002,
            FS_CYRILLIC = 0x00000004,
            FS_GREEK = 0x00000008,
            FS_TURKISH = 0x00000010,
            FS_HEBREW = 0x00000020,
            FS_ARABIC = 0x00000040,
            FS_BALTIC = 0x00000080,
            FS_VIETNAMESE = 0x00000100,
            FS_THAI = 0x00010000,
            FS_JISJAPAN = 0x00020000,
            FS_CHINESESIMP = 0x00040000,
            FS_WANSUNG = 0x00080000,
            FS_CHINESETRAD = 0x00100000,
            FS_JOHAB = 0x00200000,
            FS_SYMBOL = 0x80000000,
        }

        [DllImport("kernel32.dll")] public static extern ErrorModes SetErrorMode(ErrorModes uMode);
        [Flags]
        public enum ErrorModes : uint
        {
            SYSTEM_DEFAULT = 0x0,
            SEM_FAILCRITICALERRORS = 0x0001,
            SEM_NOALIGNMENTFAULTEXCEPT = 0x0004,
            SEM_NOGPFAULTERRORBOX = 0x0002,
            SEM_NOOPENFILEERRORBOX = 0x8000
        }
        [DllImport("kernel32.dll")] public static extern bool GetVersionEx(ref OSVERSIONINFOEX osvi);
        [DllImport("kernel32.dll")] public static extern void OutputDebugString(string lpOutputString);

        [DllImport("kernel32.dll")] public static extern IntPtr GlobalLock(IntPtr hMem);
        [DllImport("kernel32.dll")] [return: MarshalAs(UnmanagedType.Bool)] public static extern bool GlobalUnlock(IntPtr hMem);
        [DllImport("kernel32.dll", ExactSpelling = true)] public static extern IntPtr GlobalSize(IntPtr handle);
        [DllImport("kernel32.dll")] public static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);
        [DllImport("kernel32.dll")] [return: MarshalAs(UnmanagedType.Bool)] public static extern bool VirtualLock(IntPtr lpAddress, IntPtr dwSize);
        [DllImport("kernel32.dll")] [return: MarshalAs(UnmanagedType.Bool)] public static extern bool VirtualUnlock(IntPtr lpAddress, IntPtr dwSize);
        [DllImport("kernel32.dll")] public static extern IntPtr GlobalFree(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] public static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] public static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);
        [DllImport("kernel32.dll")] public static extern bool SetProcessWorkingSetSize(IntPtr hProcess, UIntPtr dwMinimumWorkingSetSize, UIntPtr dwMaximumWorkingSetSize);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)] public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)] public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] public static extern bool FreeLibrary(IntPtr hModule);
        [DllImport("kernel32.dll", SetLastError = true)] [PreserveSig] public static extern uint GetModuleFileName(IntPtr hModule, out StringBuilder lpFilename, [MarshalAs(UnmanagedType.U4)] int nSize);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)] public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)] public static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize, AllocationType flAllocationType, MemoryProtection flProtect);
        [DllImport("kernel32.dll")] [return: MarshalAs(UnmanagedType.Bool)] public static extern bool VirtualFree(IntPtr lpAddress, uint dwSize, FreeType flFreeType);

        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern IntPtr ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)] public static extern int RegOpenKeyEx(UIntPtr hKey, string subKey, int ulOptions, int samDesired, out UIntPtr hkResult);
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW", SetLastError = true)] public static extern int RegQueryValueEx(UIntPtr hKey, string lpValueName, int lpReserved, out uint lpType, StringBuilder lpData, ref uint lpcbData);
        [DllImport("advapi32.dll", SetLastError = true)] public static extern int RegCloseKey(UIntPtr hKey);

        public enum SystemIcons
        {
            IDI_APPLICATION = 32512,
            IDI_HAND = 32513,
            IDI_QUESTION = 32514,
            IDI_EXCLAMATION = 32515,
            IDI_ASTERISK = 32516,
            IDI_WINLOGO = 32517,
            IDI_WARNING = IDI_EXCLAMATION,
            IDI_ERROR = IDI_HAND,
            IDI_INFORMATION = IDI_ASTERISK,
        }

        public enum IDC_STANDARD_CURSORS
        {
            IDC_ARROW = 32512,
            IDC_IBEAM = 32513,
            IDC_WAIT = 32514,
            IDC_CROSS = 32515,
            IDC_UPARROW = 32516,
            IDC_SIZE = 32640,
            IDC_ICON = 32641,
            IDC_SIZENWSE = 32642,
            IDC_SIZENESW = 32643,
            IDC_SIZEWE = 32644,
            IDC_SIZENS = 32645,
            IDC_SIZEALL = 32646,
            IDC_NO = 32648,
            IDC_HAND = 32649,
            IDC_APPSTARTING = 32650,
            IDC_HELP = 32651
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left, top, right, bottom;
        }

        public class AsmBlock<TDelegate> : IDisposable
        {
            IntPtr code;
            Delegate del;

            public AsmBlock(byte[] asm86, byte[] asm64)
            {
                var asm = IntPtr.Size == 4 ? asm86
                    : IntPtr.Size == 8 ? asm64
                    : throw new ArgumentOutOfRangeException(nameof(IntPtr.Size));

                code = VirtualAlloc(IntPtr.Zero, new UIntPtr((uint)asm.Length), AllocationType.MEM_COMMIT | AllocationType.MEM_RESERVE, MemoryProtection.PAGE_EXECUTE_READWRITE);
                Marshal.Copy(asm, 0, code, asm.Length);
                del = Marshal.GetDelegateForFunctionPointer(code, typeof(TDelegate));
            }

            public void Invoke(Action<Delegate> action)
                => action(del);

            public void Dispose()
            {
                if (code != IntPtr.Zero) { VirtualFree(code, 0, FreeType.MEM_RELEASE); code = IntPtr.Zero; }
            }
        }

        public const ulong HKEY_LOCAL_MACHINE = 0x80000002L;

        public const int VER_PLATFORM_WIN32s = 0;
        public const int GMEM_MOVEABLE = 0x0002;
        public const int GMEM_DDESHARE = 0x2000;
        //public const int SW_HIDE = 0;
        //public const int SW_MAXIMIZE = 3;
        //public const int SW_SHOW = 5;
        //public const int SW_RESTORE = 9;
        public const int CF_TEXT = 1;
        public const int PM_REMOVE = 0x0001;
        public const int SC_CLOSE = 0xF060;
        public const int COPY_ID = 1;
        //public const int EM_GETSEL = 0x00B0;
        //public const int EM_SETSEL = 0x00B1;
        //public const int EM_REPLACESEL = 0x00C2;

        //[DllImport("kernel32.dll", EntryPoint = "lstrlenW", CharSet = CharSet.Ansi)] public static extern int lstrlen(int src);
        [DllImport("kernel32.dll", EntryPoint = "lstrcpyW", CharSet = CharSet.Ansi)] public static extern int lstrcpy(IntPtr hDest, string src);

        [Flags]
        public enum AllocationType : uint
        {
            MEM_COMMIT = 0x00001000,
            MEM_RESERVE = 0x00002000,
            MEM_RESET = 0x00080000,
            MEM_RESET_UNDO = 0x1000000,
            MEM_LARGE_PAGES = 0x20000000,
            MEM_PHYSICAL = 0x00400000,
            MEM_TOP_DOWN = 0x00100000,
            MEM_WRITE_WATCH = 0x00200000
        }

        [Flags]
        public enum MemoryProtection : uint
        {
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400,
            PAGE_TARGETS_INVALID = 0x40000000,
            PAGE_TARGETS_NO_UPDATE = 0x40000000,
        }

        [Flags]
        public enum FreeType : uint
        {
            MEM_DECOMMIT = 0x00004000,
            MEM_RELEASE = 0x00008000,
            MEM_COALESCE_PLACEHOLDERS = 0x00000001,
            MEM_PRESERVE_PLACEHOLDER = 0x00000002,
        }

        public enum PROCESS_DPI_AWARENESS
        {
            D3_PROCESS_DPI_UNAWARE = 0,
            D3_PROCESS_SYSTEM_DPI_AWARE = 1,
            D3_PROCESS_PER_MONITOR_DPI_AWARE = 2
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
            public static implicit operator Point(POINT p) => new Point(p.X, p.Y);
            public static implicit operator POINT(Point p) => new POINT(p.X, p.Y);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public UIntPtr wParam;
            public IntPtr lParam;
            public int time;
            public POINT pt;
            public int lPrivate;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORYSTATUSEX
        {
            public uint dwLength; //dwLength = sizeof(MEMORYSTATUSEX);
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct OSVERSIONINFOEX
        {
            public int dwOSVersionInfoSize;
            public int dwMajorVersion;
            public int dwMinorVersion;
            public int dwBuildNumber;
            public int dwPlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string szCSDVersion;
            public ushort wServicePackMajor;
            public ushort wServicePackMinor;
            public ushort wSuiteMask;
            public byte wProductType;
            public byte wReserved;
        }

        public enum ColorTypeControl : int
        {
            CTLCOLOR_MSGBOX = 0,
            CTLCOLOR_EDIT = 1,
            CTLCOLOR_LISTBOX = 2,
            CTLCOLOR_BTN = 3,
            CTLCOLOR_DLG = 4,
            CTLCOLOR_SCROLLBAR = 5,
            CTLCOLOR_STATIC = 6,
            CTLCOLOR_MAX = 7,
        }

        public enum ColorType : int
        {
            COLOR_SCROLLBAR = 0,
            COLOR_BACKGROUND = 1,
            COLOR_ACTIVECAPTION = 2,
            COLOR_INACTIVECAPTION = 3,
            COLOR_MENU = 4,
            COLOR_WINDOW = 5,
            COLOR_WINDOWFRAME = 6,
            COLOR_MENUTEXT = 7,
            COLOR_WINDOWTEXT = 8,
            COLOR_CAPTIONTEXT = 9,
            COLOR_ACTIVEBORDER = 10,
            COLOR_INACTIVEBORDER = 11,
            COLOR_APPWORKSPACE = 12,
            COLOR_HIGHLIGHT = 13,
            COLOR_HIGHLIGHTTEXT = 14,
            COLOR_BTNFACE = 15,
            COLOR_BTNSHADOW = 16,
            COLOR_GRAYTEXT = 17,
            COLOR_BTNTEXT = 18,
            COLOR_INACTIVECAPTIONTEXT = 19,
            COLOR_BTNHIGHLIGHT = 20,
            COLOR_3DDKSHADOW = 21,
            COLOR_3DLIGHT = 22,
            COLOR_INFOTEXT = 23,
            COLOR_INFOBK = 24,
            COLOR_HOTLIGHT = 26,
            COLOR_GRADIENTACTIVECAPTION = 27,
            COLOR_GRADIENTINACTIVECAPTION = 28,
            COLOR_MENUHILIGHT = 29,
            COLOR_MENUBAR = 30,
            COLOR_DESKTOP = COLOR_BACKGROUND,
            COLOR_3DFACE = COLOR_BTNFACE,
            COLOR_3DSHADOW = COLOR_BTNSHADOW,
            COLOR_3DHIGHLIGHT = COLOR_BTNHIGHLIGHT,
            COLOR_3DHILIGHT = COLOR_BTNHIGHLIGHT,
            COLOR_BTNHILIGHT = COLOR_BTNHIGHLIGHT,
        }
    }
}