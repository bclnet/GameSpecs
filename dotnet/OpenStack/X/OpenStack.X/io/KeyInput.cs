using System;
using System.IO;
using System.Text;
using static System.NumericsX.OpenStack.Key;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.Platform;

namespace System.NumericsX.OpenStack
{
    internal struct Key2
    {
        public bool down;
        public int repeats;        // if > 1, it is autorepeating
        public string binding;
        public int usercmdAction;  // for testing by the asyncronous usercmd generation
    }

    public static class KeyInput
    {
        //        static const char* cheatCodes[] = {
        //        "iddqd",		// Invincibility
        //        "idkfa",		// All weapons, keys, ammo, and 200% armor
        //        "idfa",			// Reset ammunition
        //        "idspispopd",	// Walk through walls
        //        "idclip",		// Walk through walls
        //        "idchoppers",	// Chainsaw
        ///*
        //	"idbeholds",	// Berserker strength
        //	"idbeholdv",	// Temporary invincibility
        //	"idbeholdi",	// Temporary invisibility
        //	"idbeholda",	// Full automap
        //	"idbeholdr",	// Anti-radiation suit
        //	"idbeholdl",	// Light amplification visor
        //	"idclev",		// Level select
        //	"iddt",			// Toggle full map; full map and objects; normal map
        //	"idmypos",		// Display coordinates and heading
        //	"idmus",		// Change music to indicated level
        //	"fhhall",		// Kill all enemies in level
        //	"fhshh",		// Invisible to enemies until attack
        //*/
        //        NULL
        //};
        //        char lastKeys[32];
        //        int lastKeyIndex;

        // keys that can be set without a special name
        static string unnamedkeys = "*,-=./[\\]1234567890abcdefghijklmnopqrstuvwxyz";

        // names not in this list can either be lowercase ascii, or '0xnn' hex sequences
        static readonly (string n, Key k, string si)[] keynames = new (string, Key, string)[]{
                ("TAB",             K_TAB,              "#str_07018"),
                ("ENTER",           K_ENTER,            "#str_07019"),
                ("ESCAPE",          K_ESCAPE,           "#str_07020"),
                ("SPACE",           K_SPACE,            "#str_07021"),
                ("BACKSPACE",       K_BACKSPACE,        "#str_07022"),
                ("UPARROW",         K_UPARROW,          "#str_07023"),
                ("DOWNARROW",       K_DOWNARROW,        "#str_07024"),
                ("LEFTARROW",       K_LEFTARROW,        "#str_07025"),
                ("RIGHTARROW",      K_RIGHTARROW,       "#str_07026"),

                ("ALT",             K_ALT,              "#str_07027"),
                ("RIGHTALT",        K_RIGHT_ALT,        "#str_07027"),
                ("CTRL",            K_CTRL,             "#str_07028"),
                ("SHIFT",           K_SHIFT,            "#str_07029"),

                ("LWIN",            K_LWIN,             "#str_07030"),
                ("RWIN",            K_RWIN,             "#str_07031"),
                ("MENU",            K_MENU,             "#str_07032"),

                ("COMMAND",         K_COMMAND,          "#str_07033"),

                ("CAPSLOCK",        K_CAPSLOCK,         "#str_07034"),
                ("SCROLL",          K_SCROLL,           "#str_07035"),
                ("PRINTSCREEN",     K_PRINT_SCR,        "#str_07179"),

                ("F1",              K_F1,               "#str_07036"),
                ("F2",              K_F2,               "#str_07037"),
                ("F3",              K_F3,               "#str_07038"),
                ("F4",              K_F4,               "#str_07039"),
                ("F5",              K_F5,               "#str_07040"),
                ("F6",              K_F6,               "#str_07041"),
                ("F7",              K_F7,               "#str_07042"),
                ("F8",              K_F8,               "#str_07043"),
                ("F9",              K_F9,               "#str_07044"),
                ("F10",             K_F10,              "#str_07045"),
                ("F11",             K_F11,              "#str_07046"),
                ("F12",             K_F12,              "#str_07047"),

                ("INS",             K_INS,              "#str_07048"),
                ("DEL",             K_DEL,              "#str_07049"),
                ("PGDN",            K_PGDN,             "#str_07050"),
                ("PGUP",            K_PGUP,             "#str_07051"),
                ("HOME",            K_HOME,             "#str_07052"),
                ("END",             K_END,              "#str_07053"),

                ("MOUSE1",          K_MOUSE1,           "#str_07054"),
                ("MOUSE2",          K_MOUSE2,           "#str_07055"),
                ("MOUSE3",          K_MOUSE3,           "#str_07056"),
                ("MOUSE4",          K_MOUSE4,           "#str_07057"),
                ("MOUSE5",          K_MOUSE5,           "#str_07058"),
                ("MOUSE6",          K_MOUSE6,           "#str_07059"),
                ("MOUSE7",          K_MOUSE7,           "#str_07060"),
                ("MOUSE8",          K_MOUSE8,           "#str_07061"),

                ("MWHEELUP",        K_MWHEELUP,         "#str_07131"),
                ("MWHEELDOWN",      K_MWHEELDOWN,       "#str_07132"),

                ("JOY1",            K_JOY1,             "#str_07062"),
                ("JOY2",            K_JOY2,             "#str_07063"),
                ("JOY3",            K_JOY3,             "#str_07064"),
                ("JOY4",            K_JOY4,             "#str_07065"),
                ("JOY5",            K_JOY5,             "#str_07066"),
                ("JOY6",            K_JOY6,             "#str_07067"),
                ("JOY7",            K_JOY7,             "#str_07068"),
                ("JOY8",            K_JOY8,             "#str_07069"),
                ("JOY9",            K_JOY9,             "#str_07070"),
                ("JOY10",           K_JOY10,            "#str_07071"),
                ("JOY11",           K_JOY11,            "#str_07072"),
                ("JOY12",           K_JOY12,            "#str_07073"),
                ("JOY13",           K_JOY13,            "#str_07074"),
                ("JOY14",           K_JOY14,            "#str_07075"),
                ("JOY15",           K_JOY15,            "#str_07076"),
                ("JOY16",           K_JOY16,            "#str_07077"),
                ("JOY17",           K_JOY17,            "#str_07078"),
                ("JOY18",           K_JOY18,            "#str_07079"),
                ("JOY19",           K_JOY19,            "#str_07080"),
                ("JOY20",           K_JOY20,            "#str_07081"),
                ("JOY21",           K_JOY21,            "#str_07082"),
                ("JOY22",           K_JOY22,            "#str_07083"),
                ("JOY23",           K_JOY23,            "#str_07084"),
                ("JOY24",           K_JOY24,            "#str_07085"),
                ("JOY25",           K_JOY25,            "#str_07086"),
                ("JOY26",           K_JOY26,            "#str_07087"),
                ("JOY27",           K_JOY27,            "#str_07088"),
                ("JOY28",           K_JOY28,            "#str_07089"),
                ("JOY29",           K_JOY29,            "#str_07090"),
                ("JOY30",           K_JOY30,            "#str_07091"),
                ("JOY31",           K_JOY31,            "#str_07092"),
                ("JOY32",           K_JOY32,            "#str_07093"),

                ("AUX1",            K_AUX1,             "#str_07094"),
                ("AUX2",            K_AUX2,             "#str_07095"),
                ("AUX3",            K_AUX3,             "#str_07096"),
                ("AUX4",            K_AUX4,             "#str_07097"),
                ("AUX5",            K_AUX5,             "#str_07098"),
                ("AUX6",            K_AUX6,             "#str_07099"),
                ("AUX7",            K_AUX7,             "#str_07100"),
                ("AUX8",            K_AUX8,             "#str_07101"),
                ("AUX9",            K_AUX9,             "#str_07102"),
                ("AUX10",           K_AUX10,            "#str_07103"),
                ("AUX11",           K_AUX11,            "#str_07104"),
                ("AUX12",           K_AUX12,            "#str_07105"),
                ("AUX13",           K_AUX13,            "#str_07106"),
                ("AUX14",           K_AUX14,            "#str_07107"),
                ("AUX15",           K_AUX15,            "#str_07108"),
                ("AUX16",           K_AUX16,            "#str_07109"),

                ("KP_HOME",         K_KP_HOME,          "#str_07110"),
                ("KP_UPARROW",      K_KP_UPARROW,       "#str_07111"),
                ("KP_PGUP",         K_KP_PGUP,          "#str_07112"),
                ("KP_LEFTARROW",    K_KP_LEFTARROW,     "#str_07113"),
                ("KP_5",            K_KP_5,             "#str_07114"),
                ("KP_RIGHTARROW",   K_KP_RIGHTARROW,    "#str_07115"),
                ("KP_END",          K_KP_END,           "#str_07116"),
                ("KP_DOWNARROW",    K_KP_DOWNARROW,     "#str_07117"),
                ("KP_PGDN",         K_KP_PGDN,          "#str_07118"),
                ("KP_ENTER",        K_KP_ENTER,         "#str_07119"),
                ("KP_INS",          K_KP_INS,           "#str_07120"),
                ("KP_DEL",          K_KP_DEL,           "#str_07121"),
                ("KP_SLASH",        K_KP_SLASH,         "#str_07122"),
                ("KP_MINUS",        K_KP_MINUS,         "#str_07123"),
                ("KP_PLUS",         K_KP_PLUS,          "#str_07124"),
                ("KP_NUMLOCK",      K_KP_NUMLOCK,       "#str_07125"),
                ("KP_STAR",         K_KP_STAR,          "#str_07126"),
                ("KP_EQUALS",       K_KP_EQUALS,        "#str_07127"),

                ("PAUSE",           K_PAUSE,            "#str_07128"),

                ("SEMICOLON",       (Key)';',             "#str_07129"),	// because a raw semicolon separates commands
                ("APOSTROPHE",      (Key)'\'',            "#str_07130")	// because a raw apostrophe messes with parsing
        };
        const int MAX_KEYS = 256;

        static bool key_overstrikeMode = false;
        static Key2[] keys = null;

        public static void Init()
        {
            keys = new Key2[MAX_KEYS];

            // register our functions
            cmdSystem.AddCommand("bind", Key_Bind_f, CMD_FL.SYSTEM, "binds a command to a key", ArgCompletion_KeyName);
            cmdSystem.AddCommand("bindunbindtwo", Key_BindUnBindTwo_f, CMD_FL.SYSTEM, "binds a key but unbinds it first if there are more than two binds");
            cmdSystem.AddCommand("unbind", Key_Unbind_f, CMD_FL.SYSTEM, "unbinds any command from a key", ArgCompletion_KeyName);
            cmdSystem.AddCommand("unbindall", Key_Unbindall_f, CMD_FL.SYSTEM, "unbinds any commands from all keys");
            cmdSystem.AddCommand("listBinds", Key_ListBinds_f, CMD_FL.SYSTEM, "lists key bindings");
        }

        public static void Shutdown()
            => keys = null;

        public static void ArgCompletion_KeyName(CmdArgs args, Action<string> callback)
        {
            for (var i = 0; i < unnamedkeys.Length; i++) callback($"{args[0]} {unnamedkeys[i]}");
            foreach (var (n, _, _) in keynames) callback($"{args[0]} {n}");
        }

        /// <summary>
        /// Tracks global key up/down state
        /// Called by the system for both key up and key down events
        /// </summary>
        /// <param name="keyNum">The key number.</param>
        /// <param name="down">if set to <c>true</c> [down].</param>
        public static void PreliminaryKeyEvent(int keyNum, bool down)
        {
            keys[keyNum].down = down;

#if ID_DOOM_LEGACY
            if (down)
            {
                lastKeys[0 + (lastKeyIndex & 15)] = keyNum;
                lastKeys[16 + (lastKeyIndex & 15)] = keyNum;
                lastKeyIndex = (lastKeyIndex + 1) & 15;
                for (var i = 0; cheatCodes[i] != null; i++)
                {
                    var l = cheatCodes[i].Length;
                    assert(l <= 16);
                    if (idStr::Icmpn(lastKeys + 16 + (lastKeyIndex & 15) - l, cheatCodes[i], l) == 0)
                    {
                        Printf("your memory serves you well!\n");
                        break;
                    }
                }
            }
#endif
        }

        public static bool IsDown(Key keyNum)
            => (int)keyNum != -1 && keys[(int)keyNum].down;

        public static int GetUsercmdAction(int keyNum)
            => keys[keyNum].usercmdAction;

        public static bool OverstrikeMode
        {
            get => key_overstrikeMode;
            set => key_overstrikeMode = value;
        }

        public static void ClearStates()
        {
            for (var i = 0; i < MAX_KEYS; i++)
            {
                if (keys[i].down) PreliminaryKeyEvent(i, false);
                keys[i].down = false;
            }

            // clear the usercommand states
            usercmdGen.Clear();
        }

        /// <summary>
        /// Returns a key number to be used to index keys[] by looking at the given string.  Single ascii characters return themselves, while the K_* names are matched up.
        /// 0x11 will be interpreted as raw hex, which will allow new controlers to be configured even if they don't have defined names.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static int StringToKeyNum(string str)
        {
            if (string.IsNullOrEmpty(str)) return -1;
            if (str[1] == 0) return str[0];

            // check for hex code
            if (str[0] == '0' && str[1] == 'x' && str.Length == 4)
            {
                var n1 = (int)str[2];
                if (n1 >= '0' && n1 <= '9') n1 -= '0';
                else if (n1 >= 'a' && n1 <= 'f') n1 = n1 - 'a' + 10;
                else n1 = 0;
                var n2 = (int)str[3];
                if (n2 >= '0' && n2 <= '9') n2 -= '0';
                else if (n2 >= 'a' && n2 <= 'f') n2 = n2 - 'a' + 10;
                else n2 = 0;
                return n1 * 16 + n2;
            }

            // scan for a text match
            foreach (var (n, k, _) in keynames) if (string.Equals(str, n, StringComparison.OrdinalIgnoreCase)) return (int)k;

            return -1;

        }
        /// <summary>
        /// Returns a string (either a single ascii char, a K_* name, or a 0x11 hex string) for the given keynum.
        /// </summary>
        /// <param name="keyNum">The key number.</param>
        /// <param name="localized">if set to <c>true</c> [localized].</param>
        /// <returns></returns>
        static char[] KeyNumToString_tinystr = new char[5];
        public static string KeyNumToString(int keyNum, bool localized = false)
        {
            if (keyNum == -1) return "<KEY NOT FOUND>";
            if (keyNum < 0 || keyNum > 255) return "<OUT OF RANGE>";

            // check for printable ascii (don't use quote)
            if (keyNum > 32 && keyNum < 127 && keyNum != '"' && keyNum != ';' && keyNum != '\'')
            {
                KeyNumToString_tinystr[0] = (char)keyNum;
                KeyNumToString_tinystr[1] = (char)0;
                return new string(KeyNumToString_tinystr);
            }

            // check for a key string
            foreach (var (n, k, si) in keynames)
                if (keyNum == (int)k)
                {
                    if (!localized || si[0] != '#') return n;
                    else
                    {
#if MACOS_X
                        switch (k)
                        {
                            case K_ENTER:
                            case K_BACKSPACE:
                            case K_ALT:
                            case K_INS:
                            case K_PRINT_SCR: return OSX_GetLocalizedString(n);
                            default: return LanguageDictGetString(si);
                        }
#else
                        return LanguageDictGetString(si);
#endif
                    }
                }

            // check for European high-ASCII characters
            if (localized && keyNum >= 161 && keyNum <= 255)
            {
                KeyNumToString_tinystr[0] = (char)keyNum;
                KeyNumToString_tinystr[1] = (char)0;
                return new string(KeyNumToString_tinystr);
            }

            // make a hex string
            var i = keyNum >> 4;
            var j = keyNum & 15;

            KeyNumToString_tinystr[0] = '0';
            KeyNumToString_tinystr[1] = 'x';
            KeyNumToString_tinystr[2] = (char)(i > 9 ? i - 10 + 'a' : i + '0');
            KeyNumToString_tinystr[3] = (char)(j > 9 ? j - 10 + 'a' : j + '0');
            KeyNumToString_tinystr[4] = (char)0;
            return new string(KeyNumToString_tinystr);
        }

        public static void SetBinding(int keyNum, string binding)
        {
            if (keyNum == -1) return;

            // Clear out all button states so we aren't stuck forever thinking this key is held down
            usercmdGen.Clear();

            // allocate memory for new binding
            keys[keyNum].binding = binding;

            // find the action for the async command generation
            keys[keyNum].usercmdAction = usercmdGen.CommandStringUsercmdData(binding);

            // consider this like modifying an archived cvar, so the
            // file write will be triggered at the next oportunity
            cvarSystem.SetModifiedFlags(CVAR.ARCHIVE);
        }

        public static string GetBinding(int keyNum) => keyNum == -1 ? string.Empty : keys[keyNum].binding;

        public static bool UnbindBinding(string binding)
        {
            var unbound = false;
            if (!string.IsNullOrEmpty(binding)) for (var i = 0; i < MAX_KEYS; i++) if (string.Equals(keys[i].binding, binding, StringComparison.OrdinalIgnoreCase)) { SetBinding(i, string.Empty); unbound = true; }
            return unbound;
        }

        public static int NumBinds(string binding)
        {
            var count = 0;
            if (!string.IsNullOrEmpty(binding)) for (var i = 0; i < MAX_KEYS; i++) if (string.Equals(keys[i].binding, binding, StringComparison.OrdinalIgnoreCase)) count++;
            return count;
        }

        public static bool ExecKeyBinding(int keyNum)
        {
            // commands that are used by the async thread don't add text
            if (keys[keyNum].usercmdAction != 0) return false;

            // send the bound action
            if (keys[keyNum].binding.Length != 0)
            {
                cmdSystem.BufferCommandText(CMD_EXEC.APPEND, keys[keyNum].binding);
                cmdSystem.BufferCommandText(CMD_EXEC.APPEND, "\n");
            }
            return true;
        }

        /// <summary>
        /// returns the localized name of the key for the binding
        /// </summary>
        /// <param name="bind">The bind.</param>
        /// <returns></returns>
        public static string KeysFromBinding(string bind)
        {
            var b = new StringBuilder();
            if (!string.IsNullOrEmpty(bind))
                for (var i = 0; i < MAX_KEYS; i++)
                    if (string.Equals(keys[i].binding, bind, StringComparison.OrdinalIgnoreCase))
                    {
                        if (b.Length > 0) b.Append(LanguageDictGetString("#str_07183"));
                        b.Append(KeyNumToString(i, true));
                    }
            return b.Length == 0
                ? LanguageDictGetString("#str_07133").ToLowerInvariant()
                : b.ToString().ToLowerInvariant();
        }

        /// <summary>
        /// returns the binding for the localized name of the key
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string BindingFromKey(string key)
        {
            var keyNum = StringToKeyNum(key);
            return keyNum < 0 || keyNum >= MAX_KEYS ? null : keys[keyNum].binding;
        }

        public static bool KeyIsBoundTo(int keyNum, string binding)
            => keyNum >= 0 && keyNum < MAX_KEYS && string.Equals(keys[keyNum].binding, binding, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Writes lines containing "bind key value"
        /// </summary>
        /// <param name="f">The f.</param>
        public static void WriteBindings(StreamWriter f)
        {
            f.Write("unbindall\n");
            for (var i = 0; i < MAX_KEYS; i++)
                if (keys[i].binding.Length != 0)
                {
                    var name = KeyNumToString(i);

                    // handle the escape character nicely
                    f.Write(!name.Contains('\\')
                        ? $"bind \"\\\" \"{keys[i].binding}\"\n"
                        : $"bind \"{KeyNumToString(i)}\" \"{keys[i].binding}\"\n");
                }
        }

        static void Key_Unbind_f(CmdArgs args)
        {
            if (args.Count != 2) { Printf("unbind <key> : remove commands from a key\n"); return; }

            var b = StringToKeyNum(args[1]);
            if (b == -1)
            {
                // If it wasn't a key, it could be a command
                if (!UnbindBinding(args[1])) Printf($"\"{args[1]}\" isn't a valid key\n");
            }
            else SetBinding(b, string.Empty);
        }

        static void Key_Unbindall_f(CmdArgs args)
        {
            for (var i = 0; i < MAX_KEYS; i++) SetBinding(i, string.Empty);
        }

        static void Key_Bind_f(CmdArgs args)
        {
            var c = args.Count;
            if (c < 2) { Printf("bind <key> [command] : attach a command to a key\n"); return; }
            var b = StringToKeyNum(args[1]);
            if (b == -1) { Printf($"\"{args[1]}\" isn't a valid key\n"); return; }

            if (c == 2)
            {
                if (keys[b].binding.Length != 0) Printf($"\"{args[1]}\" = \"{keys[b].binding}\"\n");
                else Printf($"\"{args[1]}\" is not bound\n");
                return;
            }

            // copy the rest of the command line
            var sb = new StringBuilder(); // start out with a null string
            for (var i = 2; i < c; i++)
            {
                sb.Append(args[i]);
                if (i != (c - 1)) sb.Append(' ');
            }
            SetBinding(b, sb.ToString());
        }

        // binds keynum to bindcommand and unbinds if there are already two binds on the key
        static void Key_BindUnBindTwo_f(CmdArgs args)
        {
            var c = args.Count;
            if (c < 3) { Printf("bindunbindtwo <keynum> [command]\n"); return; }
            var key = int.TryParse(args[1], out var z) ? z : 0;
            var bind = args[2];
            if (NumBinds(bind) >= 2 && !KeyIsBoundTo(key, bind)) UnbindBinding(bind);
            SetBinding(key, bind);
        }

        static void Key_ListBinds_f(CmdArgs args)
        {
            for (var i = 0; i < MAX_KEYS; i++) if (keys[i].binding.Length != 0) Printf($"{KeyNumToString(i)} \"{keys[i].binding}\"\n");
        }
    }
}
