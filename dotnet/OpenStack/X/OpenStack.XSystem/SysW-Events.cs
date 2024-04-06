using System.Collections.Generic;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.System
{
    struct KbdPoll
    {
        public int key;
        public bool state;

        public KbdPoll(int k, bool s)
        {
            key = k;
            state = s;
        }
    }

    struct MousePoll
    {
        public int action;
        public int value;

        public MousePoll(M a, int v)
        {
            action = (int)a;
            value = v;
        }
    }

    public partial class SysW
    {
        static readonly string[] kbdNames = {
            "english", "french", "german", "italian", "spanish", "turkish", "norwegian", "brazilian", null
        };

        static readonly CVar in_kbd = new("in_kbd", "english", CVAR.SYSTEM | CVAR.ARCHIVE | CVAR.NOCHEAT, "keyboard layout", kbdNames, CmdArgs.ArgCompletion_String(kbdNames));

        static readonly List<KbdPoll> kbd_polls = new();
        static readonly List<MousePoll> mouse_polls = new();
        static readonly List<SysEvent> event_list = new();

        /*
        static byte mapkey(SDL_Keycode key)
        {
            switch (key)
            {
                case SDLK_BACKSPACE: return K_BACKSPACE;
                case SDLK_PAUSE: return K_PAUSE;
            }

            if (key <= SDLK_z)
                return key & 0xff;

            switch (key)
            {
                case SDLK_APPLICATION: return K_COMMAND;
                case SDLK_CAPSLOCK: return K_CAPSLOCK;
                case SDLK_SCROLLLOCK: return K_SCROLL;
                case SDLK_POWER: return K_POWER;

                case SDLK_UP: return K_UPARROW;
                case SDLK_DOWN: return K_DOWNARROW;
                case SDLK_LEFT: return K_LEFTARROW;
                case SDLK_RIGHT: return K_RIGHTARROW;

                case SDLK_LGUI: return K_LWIN;
                case SDLK_RGUI: return K_RWIN;
                case SDLK_MENU: return K_MENU;

                case SDLK_LALT: case SDLK_RALT: return K_ALT;
                case SDLK_RCTRL: case SDLK_LCTRL: return K_CTRL;
                case SDLK_RSHIFT: case SDLK_LSHIFT: return K_SHIFT;
                case SDLK_INSERT: return K_INS;
                case SDLK_DELETE: return K_DEL;
                case SDLK_PAGEDOWN: return K_PGDN;
                case SDLK_PAGEUP: return K_PGUP;
                case SDLK_HOME: return K_HOME;
                case SDLK_END: return K_END;

                case SDLK_F1: return K_F1;
                case SDLK_F2: return K_F2;
                case SDLK_F3: return K_F3;
                case SDLK_F4: return K_F4;
                case SDLK_F5: return K_F5;
                case SDLK_F6: return K_F6;
                case SDLK_F7: return K_F7;
                case SDLK_F8: return K_F8;
                case SDLK_F9: return K_F9;
                case SDLK_F10: return K_F10;
                case SDLK_F11: return K_F11;
                case SDLK_F12: return K_F12;
                // K_INVERTED_EXCLAMATION;
                case SDLK_F13: return K_F13;
                case SDLK_F14: return K_F14;
                case SDLK_F15: return K_F15;

                case SDLK_KP_7: return K_KP_HOME;
                case SDLK_KP_8: return K_KP_UPARROW;
                case SDLK_KP_9: return K_KP_PGUP;
                case SDLK_KP_4: return K_KP_LEFTARROW;
                case SDLK_KP_5: return K_KP_5;
                case SDLK_KP_6: return K_KP_RIGHTARROW;
                case SDLK_KP_1: return K_KP_END;
                case SDLK_KP_2: return K_KP_DOWNARROW;
                case SDLK_KP_3: return K_KP_PGDN;
                case SDLK_KP_ENTER: return K_KP_ENTER;
                case SDLK_KP_0: return K_KP_INS;
                case SDLK_KP_PERIOD: return K_KP_DEL;
                case SDLK_KP_DIVIDE: return K_KP_SLASH;
                // K_SUPERSCRIPT_TWO;
                case SDLK_KP_MINUS: return K_KP_MINUS;
                // K_ACUTE_ACCENT;
                case SDLK_KP_PLUS: return K_KP_PLUS;
                case SDLK_NUMLOCKCLEAR: return K_KP_NUMLOCK;
                case SDLK_KP_MULTIPLY: return K_KP_STAR;
                case SDLK_KP_EQUALS: return K_KP_EQUALS;

                // K_MASCULINE_ORDINATOR;
                // K_GRAVE_A;
                // K_AUX1;
                // K_CEDILLA_C;
                // K_GRAVE_E;
                // K_AUX2;
                // K_AUX3;
                // K_AUX4;
                // K_GRAVE_I;
                // K_AUX5;
                // K_AUX6;
                // K_AUX7;
                // K_AUX8;
                // K_TILDE_N;
                // K_GRAVE_O;
                // K_AUX9;
                // K_AUX10;
                // K_AUX11;
                // K_AUX12;
                // K_AUX13;
                // K_AUX14;
                // K_GRAVE_U;
                // K_AUX15;
                // K_AUX16;

                case SDLK_PRINTSCREEN: return K_PRINT_SCR;
                case SDLK_MODE: return K_RIGHT_ALT;
            }

            return 0;
        }
        */

        static void PushConsoleEvent(string s)
        {
            //char* b;
            //size_t len;

            //len = strlen(s) + 1;
            //b = (char*)Mem_Alloc(len);
            //strcpy(b, s);

            //SDL_Event evnt;

            //evnt.type = SDL_USEREVENT;

            //evnt.user.code = SE.CONSOLE;

            //evnt.user.data1 = (void*)len;
            //evnt.user.data2 = b;

            //SDL_PushEvent(evnt);
        }


        public static void InitInput()
        {
            kbd_polls.Capacity = 64;
            mouse_polls.Capacity = 64;
            in_kbd.SetModified();
        }

        public static void ShutdownInput()
        {
            kbd_polls.Clear();
            mouse_polls.Clear();
        }

        // Windows has its own version due to the tools
        //public static void InitScanTable()
        //{
        //}

        static readonly char[] GetConsoleKey_keys = { '`', '~' };
        public static char GetConsoleKey(bool shifted)
        {
            if (in_kbd.IsModified)
            {
                var lang = in_kbd.String;
                if (lang.Length != 0)
                {
                    if (string.Equals(lang, "french", StringComparison.OrdinalIgnoreCase)) { GetConsoleKey_keys[0] = '<'; GetConsoleKey_keys[1] = '>'; }
                    else if (string.Equals(lang, "german", StringComparison.OrdinalIgnoreCase)) { GetConsoleKey_keys[0] = '^'; GetConsoleKey_keys[1] = (char)176; }
                    else if (string.Equals(lang, "italian", StringComparison.OrdinalIgnoreCase)) { GetConsoleKey_keys[0] = '\\'; GetConsoleKey_keys[1] = '|'; }
                    else if (string.Equals(lang, "spanish", StringComparison.OrdinalIgnoreCase)) { GetConsoleKey_keys[0] = (char)186; GetConsoleKey_keys[1] = (char)170; }
                    else if (string.Equals(lang, "turkish", StringComparison.OrdinalIgnoreCase)) { GetConsoleKey_keys[0] = '"'; GetConsoleKey_keys[1] = (char)233; }
                    else if (string.Equals(lang, "norwegian", StringComparison.OrdinalIgnoreCase)) { GetConsoleKey_keys[0] = (char)124; GetConsoleKey_keys[1] = (char)167; }
                    else if (string.Equals(lang, "brazilian", StringComparison.OrdinalIgnoreCase)) { GetConsoleKey_keys[0] = '\''; GetConsoleKey_keys[1] = '"'; }
                }
                in_kbd.ClearModified();
            }
            return shifted ? GetConsoleKey_keys[1] : GetConsoleKey_keys[0];
        }

        public static char MapCharForKey(int key)
            => (char)(key & 0xff);

        public static void GrabMouseCursor(bool grabIt)
        {
            throw new NotImplementedException();
            //var flags = grabIt
            //    ? GRAB_ENABLE | GRAB_HIDECURSOR | GRAB_SETSTATE
            //    : GRAB_SETSTATE;
            //GLimp_GrabInput(flags);
        }

        public static void AddKeyEvent(int key, bool pressed)
        {
            kbd_polls.Add(new KbdPoll(key, pressed));
            event_list.Add(new SysEvent
            {
                evType = SE.KEY,
                evValue = key,
                evValue2 = pressed ? 1 : 0,
                evPtrLength = 0,
                evPtr = IntPtr.Zero
            });
        }

        public static void AddMouseMoveEvent(int dx, int dy)
        {
            mouse_polls.Add(new MousePoll(M.M_DELTAX, dx));
            mouse_polls.Add(new MousePoll(M.M_DELTAY, dy));
            event_list.Add(system.GenerateMouseMoveEvent(dx, dy));
        }

        public static void AddMouseButtonEvent(int button, bool pressed)
        {
            switch (button)
            {
                case 1:
                    mouse_polls.Add(new MousePoll(M.M_ACTION1, pressed ? 1 : 0));
                    event_list.Add(system.GenerateMouseButtonEvent(1, pressed));
                    break;
                case 2:
                    mouse_polls.Add(new MousePoll(M.M_ACTION3, pressed ? 1 : 0));
                    event_list.Add(system.GenerateMouseButtonEvent(3, pressed));
                    break;
                case 3:
                    mouse_polls.Add(new MousePoll(M.M_ACTION2, pressed ? 1 : 0));
                    event_list.Add(system.GenerateMouseButtonEvent(2, pressed));
                    break;
            }
        }

        #region event generation

        static int GetEvent_eventIndex = 0;
        public static SysEvent GetEvent()
        {
            if (event_list.Count > GetEvent_eventIndex) return event_list[GetEvent_eventIndex++];

            // nothing left in the list, so clear
            GetEvent_eventIndex = 0;
            event_list.SetNum(0, false);

            return SysEvent.None;
        }

        public static void ClearEvents()
        {
            kbd_polls.SetNum(0, false);
            mouse_polls.SetNum(0, false);
            event_list.SetNum(0, false);
        }

        //string Android_GetCommand();

        public static void GenerateEvents()
        {
            var s = SysW.ConsoleInput();
            if (s != null) PushConsoleEvent(s);

            //var cmd = Android_GetCommand();
            //if (cmd) cmdSystem.BufferCommandText(CMD_EXEC_NOW, cmd);
        }

        #endregion

        #region keyboard input polling

        public static int PollKeyboardInputEvents()
            => kbd_polls.Count;

        public static bool ReturnKeyboardInputEvent(int n, out int key, out bool state)
        {
            if (n >= kbd_polls.Count) { key = default; state = default; return false; }

            key = kbd_polls[n].key;
            state = kbd_polls[n].state;
            return true;
        }

        public static void EndKeyboardInputEvents()
            => kbd_polls.SetNum(0, false);

        #endregion

        #region mouse input polling

        public static int PollMouseInputEvents()
            => mouse_polls.Count;

        public static bool ReturnMouseInputEvent(int n, out int action, out int value)
        {
            if (n >= mouse_polls.Count) { action = value = default; return false; }

            action = mouse_polls[n].action;
            value = mouse_polls[n].value;
            return true;
        }

        public static void EndMouseInputEvents()
            => mouse_polls.SetNum(0, false);

        #endregion
    }
}
