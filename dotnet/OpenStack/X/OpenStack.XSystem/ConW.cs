using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using static System.NumericsX.OpenStack.Key;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.OpenStack.System.NativeW;

namespace System.NumericsX.OpenStack.System
{
    public class ConW
    {
        const int COPY_ID = 1;
        const int QUIT_ID = 2;
        const int CLEAR_ID = 3;

        const int ERRORBOX_ID = 10;
        const int ERRORTEXT_ID = 11;

        const int EDIT_ID = 100;
        const int INPUT_ID = 101;

        const int COMMAND_HISTORY = 64;

        const int MAX_EDIT_LINE = 256;

        class WinConData
        {
            public IntPtr hWnd;
            public IntPtr hwndBuffer;

            public IntPtr hwndButtonClear;
            public IntPtr hwndButtonCopy;
            public IntPtr hwndButtonQuit;

            public IntPtr hwndErrorBox;
            public IntPtr hwndErrorText;

            public IntPtr hbmLogo;
            public IntPtr hbmClearBitmap;

            public IntPtr hbrEditBackground;
            public IntPtr hbrErrorBackground;

            public IntPtr hfBufferFont;
            public IntPtr hfButtonFont;

            public IntPtr hwndInputLine;

            public string errorString;

            public string consoleText, returnedText;
            public bool quitOnClose;
            public int windowWidth, windowHeight;

            public WndProcDelegate SysInputLineWndProc;

            public EditField[] historyEditLines = Enumerable.Repeat(new EditField(), COMMAND_HISTORY).ToArray();

            public int nextHistoryLine;// the last line in the history buffer, not masked
            public int historyLine;    // the line being displayed from history buffer will be <= nextHistoryLine

            public EditField consoleField = new();
        }

        static WinConData s_wcd = new();

        const ushort WA_INACTIVE = 0;
        const int LOGPIXELSY = 90;

        const int WM_CREATE = 0x0001;
        const int WM_ACTIVATE = 0x0006;
        const int WM_SETTEXT = 0x000C;
        const int WM_CLOSE = 0x0010;
        const int WM_SETFONT = 0x0030;
        const int WM_COMMAND = 0x0111;
        const int WM_SYSCOMMAND = 0x0112;
        const int WM_TIMER = 0x0113;
        const int WM_CTLCOLORSTATIC = 0x0138;
        const int WM_COPY = 0x0301;
        const int WM_KILLFOCUS = 0x0008;
        const int WM_KEYDOWN = 0x0100;
        const int WM_CHAR = 0x0102;

        static bool ConWndProc_timePolarity;

        static IntPtr ConWndProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam)
        {
            switch (uMsg)
            {
                case WM_ACTIVATE:
                    if (LOWORD(wParam) != WA_INACTIVE) SetFocus(s_wcd.hwndInputLine);
                    break;
                case WM_CLOSE:
#if ID_DEDICATED
                    if (cvarSystem.IsInitialized())
                    {
#else
                    if (cvarSystem.IsInitialized() && SysW.com_skipRenderer.Bool)
                    {
#endif
                        cmdSystem.BufferCommandText(CMD_EXEC.APPEND, "quit\n");
                    }
                    else if (s_wcd.quitOnClose) PostQuitMessage(0);
                    else { SysW.ShowConsole(0, false); SysW.win_viewlog.Bool = false; }
                    return IntPtr.Zero;

                case WM_CTLCOLORSTATIC:
                    if (lParam == s_wcd.hwndBuffer)
                    {
                        SetBkColor(wParam, RGB(0x00, 0x00, 0x80));
                        SetTextColor(wParam, RGB(0xff, 0xff, 0x00));
                        return s_wcd.hbrEditBackground;
                    }
                    else if (lParam == s_wcd.hwndErrorBox)
                    {
                        if (ConWndProc_timePolarity) { SetBkColor(wParam, RGB(0x80, 0x80, 0x80)); SetTextColor(wParam, RGB(0xff, 0x0, 0x00)); }
                        else { SetBkColor(wParam, RGB(0x80, 0x80, 0x80)); SetTextColor(wParam, RGB(0x00, 0x0, 0x00)); }
                        return s_wcd.hbrErrorBackground;
                    }
                    break;
                case WM_SYSCOMMAND:
                    if (wParam == (IntPtr)SC_CLOSE) PostQuitMessage(0);
                    break;
                case WM_COMMAND:
                    if (wParam == (IntPtr)COPY_ID)
                    {
                        SendMessage(s_wcd.hwndBuffer, (uint)EditControlMessage.EM_SETSEL, 0, -1);
                        SendMessage(s_wcd.hwndBuffer, WM_COPY, 0, 0);
                    }
                    else if (wParam == (IntPtr)QUIT_ID)
                    {
#if ID_DEDICATED
                        cmdSystem.BufferCommandText(CMD_EXEC.APPEND, "quit\n");
#else
                        if (s_wcd.quitOnClose) PostQuitMessage(0);
                        else cmdSystem.BufferCommandText(CMD_EXEC.APPEND, "quit\n");
#endif
                    }
                    else if (wParam == (IntPtr)CLEAR_ID)
                    {
                        SendMessage(s_wcd.hwndBuffer, (uint)EditControlMessage.EM_SETSEL, 0, -1);
                        SendMessage(s_wcd.hwndBuffer, (uint)EditControlMessage.EM_REPLACESEL, 0, string.Empty);
                        UpdateWindow(s_wcd.hwndBuffer);
                    }
                    break;
                case WM_CREATE:
                    s_wcd.hbrEditBackground = CreateSolidBrush(RGB(0x00, 0x00, 0x80));
                    s_wcd.hbrErrorBackground = CreateSolidBrush(RGB(0x80, 0x80, 0x80));
                    SetTimer(hWnd, 1, 1000, null);
                    break;
                case WM_TIMER:
                    if (wParam == (IntPtr)1)
                    {
                        ConWndProc_timePolarity = !ConWndProc_timePolarity;
                        if (s_wcd.hwndErrorBox != IntPtr.Zero) InvalidateRect(s_wcd.hwndErrorBox, IntPtr.Zero, false);
                    }
                    break;
            }

            return DefWindowProc(hWnd, uMsg, wParam, lParam);
        }

        static IntPtr InputLineWndProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam)
        {
            Key key;
            switch (uMsg)
            {
                case WM_KILLFOCUS:
                    if (wParam == s_wcd.hWnd || wParam == s_wcd.hwndErrorBox) { SetFocus(hWnd); return IntPtr.Zero; }
                    break;

                case WM_KEYDOWN:
                    key = SysW.Win_MapKey((int)lParam);

                    // command history
                    if (key == K_UPARROW || key == K_KP_UPARROW)
                    {
                        if (s_wcd.nextHistoryLine - s_wcd.historyLine < COMMAND_HISTORY && s_wcd.historyLine > 0) s_wcd.historyLine--;
                        s_wcd.consoleField = s_wcd.historyEditLines[s_wcd.historyLine % COMMAND_HISTORY];

                        SetWindowText(s_wcd.hwndInputLine, s_wcd.consoleField.Buffer);
                        SendMessage(s_wcd.hwndInputLine, (uint)EditControlMessage.EM_SETSEL, s_wcd.consoleField.Cursor, s_wcd.consoleField.Cursor);
                        return IntPtr.Zero;
                    }

                    if (key == K_DOWNARROW || key == K_KP_DOWNARROW)
                    {
                        if (s_wcd.historyLine == s_wcd.nextHistoryLine) return IntPtr.Zero;
                        s_wcd.historyLine++;
                        s_wcd.consoleField = s_wcd.historyEditLines[s_wcd.historyLine % COMMAND_HISTORY];

                        SetWindowText(s_wcd.hwndInputLine, s_wcd.consoleField.Buffer);
                        SendMessage(s_wcd.hwndInputLine, (uint)EditControlMessage.EM_SETSEL, s_wcd.consoleField.Cursor, s_wcd.consoleField.Cursor);
                        return IntPtr.Zero;
                    }
                    break;

                case WM_CHAR:
                    key = SysW.Win_MapKey((int)lParam);

                    GetWindowText(s_wcd.hwndInputLine, s_wcd.consoleField.Buffer, MAX_EDIT_LINE);
                    var cursor = 0;
                    SendMessage(s_wcd.hwndInputLine, (uint)EditControlMessage.EM_GETSEL, 0, ref cursor);
                    s_wcd.consoleField.Cursor = cursor;

                    // enter the line
                    if (key == K_ENTER || key == K_KP_ENTER)
                    {
                        s_wcd.consoleText += $"{s_wcd.consoleField.Buffer}\n";
                        SetWindowText(s_wcd.hwndInputLine, string.Empty);

                        SysW.Printf($"]{s_wcd.consoleField.Buffer}\n");

                        // copy line to history buffer
                        s_wcd.historyEditLines[s_wcd.nextHistoryLine % COMMAND_HISTORY].Set(s_wcd.consoleField);
                        s_wcd.nextHistoryLine++;
                        s_wcd.historyLine = s_wcd.nextHistoryLine;

                        s_wcd.consoleField.Clear();

                        return IntPtr.Zero;
                    }

                    // command completion
                    if (key == K_TAB)
                    {
                        s_wcd.consoleField.AutoComplete();

                        SetWindowText(s_wcd.hwndInputLine, s_wcd.consoleField.Buffer);
                        //s_wcd.consoleField.SetWidthInChars(s_wcd.consoleField.Buffer.Length);
                        SendMessage(s_wcd.hwndInputLine, (uint)EditControlMessage.EM_SETSEL, s_wcd.consoleField.Cursor, s_wcd.consoleField.Cursor);

                        return IntPtr.Zero;
                    }

                    // clear autocompletion buffer on normal key input
                    if ((key >= K_SPACE && key <= K_BACKSPACE) || (key >= K_KP_SLASH && key <= K_KP_PLUS) || (key >= K_KP_STAR && key <= K_KP_EQUALS)) s_wcd.consoleField.ClearAutoComplete();
                    break;
            }

            return CallWindowProc(s_wcd.SysInputLineWndProc, hWnd, uMsg, wParam, lParam);
        }

        public static void CreateConsole()
        {
            var DEDCLASS = PlatformW.WIN32_CONSOLE_CLASS;
            var DEDSTYLE = WindowStyle.WS_POPUPWINDOW | WindowStyle.WS_CAPTION | WindowStyle.WS_MINIMIZEBOX;

            var wc = new WNDCLASSEX
            {
                cbSize = Marshal.SizeOf(typeof(WNDCLASSEX)),
                style = 0,
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate<WndProcDelegate>(ConWndProc),
                cbClsExtra = 0,
                cbWndExtra = 0,
                hInstance = SysW.hInstance,
                hIcon = LoadIcon(SysW.hInstance, (int)SystemIcons.IDI_APPLICATION),
                hCursor = LoadCursor(IntPtr.Zero, (int)IDC_STANDARD_CURSORS.IDC_ARROW),
                hbrBackground = (IntPtr)ColorType.COLOR_WINDOW,
                lpszMenuName = null,
                lpszClassName = DEDCLASS
            };

            var regResult = RegisterClassEx(ref wc);
            if (regResult == 0) return;

            RECT rect;
            rect.left = 0;
            rect.right = 540;
            rect.top = 0;
            rect.bottom = 450;
            AdjustWindowRectEx(ref rect, DEDSTYLE, false, 0);

            var hDC = GetDC(GetDesktopWindow());
            int swidth = GetDeviceCaps(hDC, 8), sheight = GetDeviceCaps(hDC, 10);
            ReleaseDC(GetDesktopWindow(), hDC);

            s_wcd.windowWidth = rect.right - rect.left + 1;
            s_wcd.windowHeight = rect.bottom - rect.top + 1;

            //s_wcd.hbmLogo = LoadBitmap( win32.hInstance, MAKEINTRESOURCE( IDB_BITMAP_LOGO) );

            s_wcd.hWnd = CreateWindowEx(0,
                regResult, PlatformW.GAME_NAME, DEDSTYLE,
                (swidth - 600) / 2, (sheight - 450) / 2, rect.right - rect.left + 1, rect.bottom - rect.top + 1,
                IntPtr.Zero, IntPtr.Zero, SysW.hInstance, IntPtr.Zero);
            if (s_wcd.hWnd == IntPtr.Zero) return;

            //
            // create fonts
            //
            hDC = GetDC(s_wcd.hWnd);
            var nHeight = -intX.MulDiv(8, GetDeviceCaps(hDC, LOGPIXELSY), 72);

            s_wcd.hfBufferFont = CreateFont(nHeight, 0, 0, 0, FontWeight.FW_LIGHT, 0, 0, 0, FontCharSet.DEFAULT_CHARSET, FontOutputPrecision.OUT_DEFAULT_PRECIS, FontClipPrecision.CLIP_DEFAULT_PRECIS, FontQuality.DEFAULT_QUALITY, FontPitchAndFamily.FF_MODERN | FontPitchAndFamily.FIXED_PITCH, "Courier New");

            ReleaseDC(s_wcd.hWnd, hDC);

            //
            // create the input line
            //
            s_wcd.hwndInputLine = CreateWindowEx(0, "edit", null, WindowStyle.WS_CHILD | WindowStyle.WS_VISIBLE | WindowStyle.WS_BORDER | WindowStyle.ES_LEFT | WindowStyle.ES_AUTOHSCROLL,
                6, 400, 528, 20,
                s_wcd.hWnd,
                (IntPtr)INPUT_ID,  // child window ID
                SysW.hInstance, IntPtr.Zero);

            //
            // create the buttons
            //
            s_wcd.hwndButtonCopy = CreateWindowEx(0, "button", null, WindowStyle.BS_PUSHBUTTON | WindowStyle.WS_VISIBLE | WindowStyle.WS_CHILD | WindowStyle.BS_DEFPUSHBUTTON,
                5, 425, 72, 24,
                s_wcd.hWnd, (IntPtr)COPY_ID,   // child window ID
                SysW.hInstance, IntPtr.Zero);
            SendMessage(s_wcd.hwndButtonCopy, WM_SETTEXT, 0, "copy");

            s_wcd.hwndButtonClear = CreateWindowEx(0, "button", null, WindowStyle.BS_PUSHBUTTON | WindowStyle.WS_VISIBLE | WindowStyle.WS_CHILD | WindowStyle.BS_DEFPUSHBUTTON,
                82, 425, 72, 24,
                s_wcd.hWnd, (IntPtr)CLEAR_ID,  // child window ID
                SysW.hInstance, IntPtr.Zero);
            SendMessage(s_wcd.hwndButtonClear, WM_SETTEXT, 0, "clear");

            s_wcd.hwndButtonQuit = CreateWindowEx(0, "button", null, WindowStyle.BS_PUSHBUTTON | WindowStyle.WS_VISIBLE | WindowStyle.WS_CHILD | WindowStyle.BS_DEFPUSHBUTTON,
                462, 425, 72, 24,
                s_wcd.hWnd, (IntPtr)QUIT_ID,   // child window ID
                SysW.hInstance, IntPtr.Zero);
            SendMessage(s_wcd.hwndButtonQuit, WM_SETTEXT, 0, "quit");

            //
            // create the scrollbuffer
            //
            s_wcd.hwndBuffer = CreateWindowEx(0, "edit", null, WindowStyle.WS_CHILD | WindowStyle.WS_VISIBLE | WindowStyle.WS_VSCROLL | WindowStyle.WS_BORDER | WindowStyle.ES_LEFT | WindowStyle.ES_MULTILINE | WindowStyle.ES_AUTOVSCROLL | WindowStyle.ES_READONLY,
                6, 40, 526, 354,
                s_wcd.hWnd, (IntPtr)EDIT_ID,   // child window ID
                SysW.hInstance, IntPtr.Zero);
            SendMessage(s_wcd.hwndBuffer, WM_SETFONT, s_wcd.hfBufferFont, IntPtr.Zero);
            s_wcd.SysInputLineWndProc = Marshal.GetDelegateForFunctionPointer<WndProcDelegate>(SetWindowLongPtr(s_wcd.hwndInputLine, WindowFieldOffset.GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate<WndProcDelegate>(InputLineWndProc)));
            SendMessage(s_wcd.hwndInputLine, WM_SETFONT, s_wcd.hfBufferFont, IntPtr.Zero);

            // don't show it now that we have a splash screen up
            if (SysW.win_viewlog.Bool)
            {
                ShowWindow(s_wcd.hWnd, ShowWindowCmdShow.SW_SHOWDEFAULT);
                UpdateWindow(s_wcd.hWnd);
                SetForegroundWindow(s_wcd.hWnd);
                SetFocus(s_wcd.hwndInputLine);
            }

            s_wcd.consoleField.Clear();

            for (var i = 0; i < COMMAND_HISTORY; i++) s_wcd.historyEditLines[i].Clear();
        }

        public static void DestroyConsole()
        {
            if (s_wcd.hWnd != IntPtr.Zero)
            {
                ShowWindow(s_wcd.hWnd, ShowWindowCmdShow.SW_HIDE);
                CloseWindow(s_wcd.hWnd);
                DestroyWindow(s_wcd.hWnd);
                s_wcd.hWnd = IntPtr.Zero;
            }
        }

        public static void ShowConsole(int visLevel, bool quitOnClose)
        {
            s_wcd.quitOnClose = quitOnClose;
            if (s_wcd.hWnd == IntPtr.Zero) return;

            switch (visLevel)
            {
                case 0: ShowWindow(s_wcd.hWnd, ShowWindowCmdShow.SW_HIDE); break;
                case 1: ShowWindow(s_wcd.hWnd, ShowWindowCmdShow.SW_SHOWNORMAL); SendMessage(s_wcd.hwndBuffer, (uint)EditControlMessage.EM_LINESCROLL, 0, 0xffff); break;
                case 2: ShowWindow(s_wcd.hWnd, ShowWindowCmdShow.SW_MINIMIZE); break;
                default: SysW.Error($"Invalid visLevel {visLevel} sent to SysX.ShowConsole\n"); break;
            }
        }

        string ConsoleInput()
        {
            if (s_wcd.consoleText[0] == 0) return null;

            s_wcd.returnedText = s_wcd.consoleText;
            s_wcd.consoleText = string.Empty;

            return s_wcd.returnedText;
        }

        static long AppendText_totalChars;
        public static unsafe void AppendText(string pMsg)
        {
            const int CONSOLE_BUFFER_SIZE = 16384;
            const int BUFFERLength = CONSOLE_BUFFER_SIZE * 2;
            var buffer = stackalloc char[CONSOLE_BUFFER_SIZE * 2];
            char* b = buffer;
            int i = 0;

            // if the message is REALLY long, use just the last portion of it
            var msg = pMsg.Length > CONSOLE_BUFFER_SIZE
                ? Encoding.ASCII.GetBytes(pMsg.Substring(pMsg.Length - CONSOLE_BUFFER_SIZE, CONSOLE_BUFFER_SIZE))
                : Encoding.ASCII.GetBytes(pMsg);

            // copy into an intermediate buffer
            while (msg[i] != 0 && ((b - buffer) < BUFFERLength - 1))
            {
                if (msg[i] == '\n' && msg[i + 1] == '\r') { b[0] = '\r'; b[1] = '\n'; b += 2; i++; }
                else if (msg[i] == '\r') { b[0] = '\r'; b[1] = '\n'; b += 2; }
                else if (msg[i] == '\n') { b[0] = '\r'; b[1] = '\n'; b += 2; }
                else if (stringX.IsColor(msg, i)) { i++; }
                else { *b = (char)msg[i]; b++; }
                i++;
            }
            *b = '\x0';
            var bufLen = b - buffer;

            AppendText_totalChars += bufLen;

            // replace selection instead of appending if we're overflowing
            if (AppendText_totalChars > 0x7000)
            {
                SendMessage(s_wcd.hwndBuffer, (uint)EditControlMessage.EM_SETSEL, 0, -1);
                AppendText_totalChars = bufLen;
            }

            // put this text into the windows console
            SendMessage(s_wcd.hwndBuffer, (uint)EditControlMessage.EM_LINESCROLL, 0, 0xffff);
            SendMessage(s_wcd.hwndBuffer, (uint)EditControlMessage.EM_SCROLLCARET, 0, 0);
            SendMessage(s_wcd.hwndBuffer, (uint)EditControlMessage.EM_REPLACESEL, 0, new string(buffer));
        }

        public static void SetErrorText(string buf)
        {
            s_wcd.errorString = buf;
            if (s_wcd.hwndErrorBox == IntPtr.Zero)
            {
                s_wcd.hwndErrorBox = CreateWindowEx(0, "static", null, WindowStyle.WS_CHILD | WindowStyle.WS_VISIBLE | WindowStyle.SS_SUNKEN,
                    6, 5, 526, 30,
                    s_wcd.hWnd, (IntPtr)ERRORBOX_ID, // child window ID
                    SysW.hInstance, IntPtr.Zero);
                SendMessage(s_wcd.hwndErrorBox, WM_SETFONT, s_wcd.hfBufferFont, IntPtr.Zero);
                SetWindowText(s_wcd.hwndErrorBox, s_wcd.errorString);

                DestroyWindow(s_wcd.hwndInputLine);
                s_wcd.hwndInputLine = IntPtr.Zero;
            }
        }
    }
};