using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.Platform;

namespace System.NumericsX.OpenStack
{
    public interface ICVarSystem
    {
        void Init();
        void Shutdown();
        bool IsInitialized();

        // Registers a CVar.
        void Register(CVar cvar);

        // Finds the CVar with the given name.
        // Returns null if there is no CVar with the given name.
        CVar Find(string name);

        // Sets the value of a CVar by name.
        void SetCVarString(string name, string value, CVAR flags = 0);
        void SetCVarBool(string name, bool value, CVAR flags = 0);
        void SetCVarInteger(string name, int value, CVAR flags = 0);
        void SetCVarFloat(string name, float value, CVAR flags = 0);

        // Gets the value of a CVar by name.
        string GetCVarString(string name);
        bool GetCVarBool(string name);
        int GetCVarInteger(string name);
        float GetCVarFloat(string name);

        // Called by the command system when argv(0) doesn't match a known command.
        // Returns true if argv(0) is a variable reference and prints or changes the CVar.
        bool Command(CmdArgs args);

        // Command and argument completion using callback for each valid string.
        void CommandCompletion(Action<string> callback);
        void ArgCompletion(string cmdString, Action<string> callback);

        // Sets/gets/clears modified flags that tell what kind of CVars have changed.
        void SetModifiedFlags(CVAR flags);
        CVAR GetModifiedFlags();
        void ClearModifiedFlags(CVAR flags);

        // Resets variables with one of the given flags set.
        void ResetFlaggedVariables(CVAR flags);

        // Removes auto-completion from the flagged variables.
        void RemoveFlaggedAutoCompletion(CVAR flags);

        // Writes variables with one of the given flags set to the given file.
        void WriteFlaggedVariables(CVAR flags, string setCmd, VFile f);

        // Moves CVars to and from dictionaries.
        Dictionary<string, string> MoveCVarsToDict(CVAR flags);
        void SetCVarsFromDict(Dictionary<string, string> dict);
    }

    internal class CVarSystemLocal : ICVarSystem
    {
        public CVarSystemLocal()
        {
            initialized = false;
            modifiedFlags = 0;
        }

        public void Init()
        {
            modifiedFlags = 0;

            cmdSystem.AddCommand("toggle", Toggle_f, CMD_FL.SYSTEM, "toggles a cvar");
            cmdSystem.AddCommand("set", Set_f, CMD_FL.SYSTEM, "sets a cvar");
            cmdSystem.AddCommand("sets", SetS_f, CMD_FL.SYSTEM, "sets a cvar and flags it as server info");
            cmdSystem.AddCommand("setu", SetU_f, CMD_FL.SYSTEM, "sets a cvar and flags it as user info");
            cmdSystem.AddCommand("sett", SetT_f, CMD_FL.SYSTEM, "sets a cvar and flags it as tool");
            cmdSystem.AddCommand("seta", SetA_f, CMD_FL.SYSTEM, "sets a cvar and flags it as archive");
            cmdSystem.AddCommand("reset", Reset_f, CMD_FL.SYSTEM, "resets a cvar");
            cmdSystem.AddCommand("listCvars", List_f, CMD_FL.SYSTEM, "lists cvars");
            cmdSystem.AddCommand("cvar_restart", Restart_f, CMD_FL.SYSTEM, "restart the cvar system");

            initialized = true;
        }

        public void Shutdown()
        {
            cvars.Clear();
            moveCVarsToDict.Clear();
            initialized = false;
        }

        public bool IsInitialized()
            => initialized;

        public void Register(CVar cvar)
        {
            cvar.InternalVar = cvar;

            var local = FindLocal(cvar.Name);
            if (local != null) local.Update(cvar);
            else { local = new CVarLocal(cvar); cvars[local.nameString] = local; }

            cvar.InternalVar = local;
        }

        public CVar Find(string name)
            => FindLocal(name);

        public void SetCVarString(string name, string value, CVAR flags = 0) => SetInternal(name, value, flags);
        public void SetCVarBool(string name, bool value, CVAR flags = 0) => SetInternal(name, value.ToString(), flags);
        public void SetCVarInteger(string name, int value, CVAR flags = 0) => SetInternal(name, value.ToString(), flags);
        public void SetCVarFloat(string name, float value, CVAR flags = 0) => SetInternal(name, value.ToString(), flags);
        public string GetCVarString(string name) => FindLocal(name)?.String ?? string.Empty;
        public bool GetCVarBool(string name) => FindLocal(name)?.Bool ?? false;
        public int GetCVarInteger(string name) => FindLocal(name)?.Integer ?? 0;
        public float GetCVarFloat(string name) => FindLocal(name)?.Float ?? 0f;

        public bool Command(CmdArgs args)
        {
            var local = FindLocal(args[0]);
            if (local == null) return false;

            if (args.Count == 1)
            {
                // print the variable
                Printf($"\"{local.nameString}\" is:\"{local.valueString}\"{S_COLOR_WHITE} default:\"{local.resetString}\"\n");
                if (local.Description.Length > 0) Printf($"{S_COLOR_WHITE}{local.Description}\n");
            }
            // set the value
            else local.Set(args.Args(), false, false);
            return true;
        }

        public void CommandCompletion(Action<string> callback)
        {
            foreach (var cvar in cvars.Values)
                callback(cvar.Name);
        }

        public void ArgCompletion(string cmdString, Action<string> callback)
        {
            var args = new CmdArgs();
            args.TokenizeString(cmdString, false);

            foreach (var cvar in cvars.Values)
            {
                if (cvar.valueCompletion == null) continue;
                if (string.Equals(args[0], cvar.nameString, StringComparison.OrdinalIgnoreCase)) { cvar.valueCompletion(args, callback); break; }
            }
        }

        public void SetModifiedFlags(CVAR flags) => modifiedFlags |= flags;
        public CVAR GetModifiedFlags() => modifiedFlags;
        public void ClearModifiedFlags(CVAR flags) => modifiedFlags &= ~flags;

        public void ResetFlaggedVariables(CVAR flags)
        {
            foreach (var cvar in cvars.Values) if ((cvar.Flags & flags) != 0) cvar.Set(null, true, true);
        }

        public void RemoveFlaggedAutoCompletion(CVAR flags)
        {
            foreach (var cvar in cvars.Values) if ((cvar.Flags & flags) != 0) cvar.valueCompletion = null;
        }

        public void WriteFlaggedVariables(CVAR flags, string setCmd, VFile f)
        {
            foreach (var cvar in cvars.Values) if ((cvar.Flags & flags) != 0) Printf($"{setCmd} {cvar.Name} \"{cvar.String}\"\n");
        }

        public Dictionary<string, string> MoveCVarsToDict(CVAR flags)
        {
            moveCVarsToDict.Clear();
            foreach (var cvar in cvars.Values) if ((cvar.Flags & flags) != 0) moveCVarsToDict[cvar.Name] = cvar.String;
            return moveCVarsToDict;
        }

        public void SetCVarsFromDict(Dictionary<string, string> dict)
        {
            foreach (var kv in dict)
            {
                var local = FindLocal(kv.Key);
                if (local != null) local.InternalServerSetString(kv.Value);
            }
        }

        public void RegisterInternal(CVar cvar) { }

        public CVarLocal FindLocal(string name)
            => cvars.TryGetValue(name, out var local) ? local : null;

        public void SetInternal(string name, string value, CVAR flags)
        {
            var local = FindLocal(name);
            if (local != null) { local.InternalSetString(value); local.flags |= flags & ~CVAR.STATIC; local.UpdateCheat(); }
            else { local = new CVarLocal(name, value, flags); cvars[local.nameString] = local; }
        }

        bool initialized;
        Dictionary<string, CVarLocal> cvars = new(StringComparer.OrdinalIgnoreCase);
        CVAR modifiedFlags;
        // use a static dictionary to MoveCVarsToDict can be used from game
        static Dictionary<string, string> moveCVarsToDict = new(StringComparer.OrdinalIgnoreCase);

        static void Toggle_f(CmdArgs args)
        {
            int argc, i; float current, set; string text;

            argc = args.Count;
            if (argc < 2)
            {
                Printf(@"usage:\n"
                    + "   toggle <variable>  - toggles between 0 and 1\n"
                    + "   toggle <variable> <value> - toggles between 0 and <value>\n"
                    + "   toggle <variable> [string 1] [string 2]...[string n] - cycles through all strings\n");
                return;
            }

            var cvar = cvarSystemLocal.FindLocal(args[1]);

            if (cvar == null) { Warning($"Toggle_f: cvar \"{args[1]}\" not found"); return; }

            if (argc > 3)
            {
                // cycle through multiple values
                text = cvar.String;
                for (i = 2; i < argc; i++) if (string.Equals(text, args[i], StringComparison.OrdinalIgnoreCase)) { i++; break; } // point to next value
                if (i >= argc) i = 2;

                Printf($"set {args[1]} = {args[i]}\n");
                cvar.Set(args[i], false, false);
            }
            else
            {
                // toggle between 0 and 1
                current = cvar.Float;
                set = argc == 3 ? float.TryParse(args[2], out var z) ? z : 0f : 1.0f;
                current = current == 0.0f ? set : 0.0f;
                Printf($"set {args[1]} = {current}\n");
                cvar.Set(current.ToString(), false, false);
            }
        }

        static void Set_f(CmdArgs args)
        {
            var str = args.Args(2, args.Count - 1);
            cvarSystemLocal.SetCVarString(args[1], str);
        }

        static void SetS_f(CmdArgs args)
        {
            Set_f(args);
            var cvar = cvarSystemLocal.FindLocal(args[1]);
            if (cvar == null) return;
            cvar.flags |= CVAR.SERVERINFO | CVAR.ARCHIVE;
        }

        static void SetU_f(CmdArgs args)
        {
            Set_f(args);
            var cvar = cvarSystemLocal.FindLocal(args[1]);
            if (cvar == null) return;
            cvar.flags |= CVAR.USERINFO | CVAR.ARCHIVE;
        }

        static void SetT_f(CmdArgs args)
        {
            Set_f(args);
            var cvar = cvarSystemLocal.FindLocal(args[1]);
            if (cvar == null) return;
            cvar.flags |= CVAR.TOOL;
        }

        static void SetA_f(CmdArgs args)
        {
            Set_f(args);
            var cvar = cvarSystemLocal.FindLocal(args[1]);
            if (cvar == null) return;

            // FIXME: enable this for ship, so mods can store extra data but during development we don't want obsolete cvars to continue to be saved
            //	cvar.flags |= CVAR.ARCHIVE;
        }

        static void Reset_f(CmdArgs args)
        {
            if (args.Count != 2) { Printf("usage: reset <variable>\n"); return; }
            var cvar = cvarSystemLocal.FindLocal(args[1]);
            if (cvar == null) return;

            cvar.Reset();
        }

        enum SHOW
        {
            VALUE,
            DESCRIPTION,
            TYPE,
            FLAGS
        }

        static void ListByFlags(CmdArgs args, CVAR flags)
        {
            var cvarList = new List<CVarLocal>();

            var argNum = 1;
            var show = SHOW.VALUE;

            if (string.Equals(args[argNum], "-", StringComparison.OrdinalIgnoreCase) || string.Equals(args[argNum], "/", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(args[argNum + 1], "help", StringComparison.OrdinalIgnoreCase) || string.Equals(args[argNum + 1], "?", StringComparison.OrdinalIgnoreCase)) { argNum = 3; show = SHOW.DESCRIPTION; }
                else if (string.Equals(args[argNum + 1], "type", StringComparison.OrdinalIgnoreCase) || string.Equals(args[argNum + 1], "range", StringComparison.OrdinalIgnoreCase)) { argNum = 3; show = SHOW.TYPE; }
                else if (string.Equals(args[argNum + 1], "flags", StringComparison.OrdinalIgnoreCase)) { argNum = 3; show = SHOW.FLAGS; }
            }

            string match;
            if (args.Count > argNum) { match = args.Args(argNum, -1); match = match.Replace(" ", ""); }
            else match = string.Empty;

            foreach (var cvar in cvarSystemLocal.cvars.Values)
            {
                if ((cvar.Flags & flags) == 0) continue;
                if (match.Length != 0 && !cvar.nameString.Filter(match, false)) continue;
                cvarList.Add(cvar);
            }

            cvarList.Sort();

            const int NUM_COLUMNS = 77; // 78 - 1
            const int NUM_NAME_CHARS = 33;
            const int NUM_DESCRIPTION_CHARS = NUM_COLUMNS - NUM_NAME_CHARS;

            switch (show)
            {
                case SHOW.VALUE:
                    {
                        foreach (var cvar in cvarList) Printf($"{cvar.nameString:-32}{S_COLOR_WHITE} \"{cvar.valueString}\"\n");
                        break;
                    }
                case SHOW.DESCRIPTION:
                    {
                        var indent = "\n" + new string(' ', NUM_NAME_CHARS);
                        var b = new StringBuilder();
                        foreach (var cvar in cvarList) Printf($"{cvar.nameString:-32}{S_COLOR_WHITE}{CreateColumn(cvar.Description, NUM_DESCRIPTION_CHARS, indent, b)}\n");
                        break;
                    }
                case SHOW.TYPE:
                    {
                        foreach (var cvar in cvarList)
                        {
                            if ((cvar.Flags & CVAR.BOOL) != 0) Printf($"{cvar.Name:-32}{S_COLOR_CYAN}bool\n");
                            else if ((cvar.Flags & CVAR.INTEGER) != 0)
                            {
                                if (cvar.MinValue < cvar.MaxValue) Printf($"{cvar.Name:-32}{S_COLOR_GREEN}int {S_COLOR_WHITE}[{(int)cvar.MinValue}, {(int)cvar.MaxValue}]\n");
                                else Printf($"{cvar.Name:-32}{S_COLOR_GREEN}int\n");
                            }
                            else if ((cvar.Flags & CVAR.FLOAT) != 0)
                            {
                                if (cvar.MinValue < cvar.MaxValue) Printf($"{cvar.Name:-32}{S_COLOR_RED}float {S_COLOR_WHITE}[{cvar.MinValue}, {cvar.MaxValue}]\n");
                                else Printf($"{cvar.Name:-32}{S_COLOR_RED}float\n");
                            }
                            else if (cvar.ValueStrings != null)
                            {
                                Printf($"{cvar.Name:-32}{S_COLOR_WHITE}string {S_COLOR_WHITE}[");
                                for (var j = 0; cvar.ValueStrings[j] != null; j++)
                                    if (j != 0) Printf($"{S_COLOR_WHITE}, {cvar.ValueStrings[j]}");
                                    else Printf($"{S_COLOR_WHITE}{cvar.ValueStrings[j]}");
                                Printf($"{S_COLOR_WHITE}]\n");
                            }
                            else Printf($"{cvar.Name:-32}{S_COLOR_WHITE}string\n");
                        }
                        break;
                    }
                case SHOW.FLAGS:
                    {
                        foreach (var cvar in cvarList)
                        {
                            Printf($"{cvar.Name:-32}");
                            var s = string.Empty;
                            if ((cvar.Flags & CVAR.BOOL) != 0) s += $"{S_COLOR_CYAN}B ";
                            else if ((cvar.Flags & CVAR.INTEGER) != 0) s += $"{S_COLOR_GREEN}I ";
                            else if ((cvar.Flags & CVAR.FLOAT) != 0) s += $"{S_COLOR_RED}F ";
                            else s += $"{S_COLOR_WHITE}S ";
                            if ((cvar.Flags & CVAR.SYSTEM) != 0) s += $"{S_COLOR_WHITE}SYS  ";
                            else if ((cvar.Flags & CVAR.RENDERER) != 0) s += $"{S_COLOR_WHITE}RNDR ";
                            else if ((cvar.Flags & CVAR.SOUND) != 0) s += $"{S_COLOR_WHITE}SND  ";
                            else if ((cvar.Flags & CVAR.GUI) != 0) s += $"{S_COLOR_WHITE}GUI  ";
                            else if ((cvar.Flags & CVAR.GAME) != 0) s += $"{S_COLOR_WHITE}GAME ";
                            else if ((cvar.Flags & CVAR.TOOL) != 0) s += $"{S_COLOR_WHITE}TOOL ";
                            else s += $"{S_COLOR_WHITE}     ";
                            s += ((cvar.Flags & CVAR.USERINFO) != 0) ? "UI " : "   ";
                            s += ((cvar.Flags & CVAR.SERVERINFO) != 0) ? "SI " : "   ";
                            s += ((cvar.Flags & CVAR.STATIC) != 0) ? "ST " : "   ";
                            s += ((cvar.Flags & CVAR.CHEAT) != 0) ? "CH " : "   ";
                            s += ((cvar.Flags & CVAR.INIT) != 0) ? "IN " : "   ";
                            s += ((cvar.Flags & CVAR.ROM) != 0) ? "RO " : "   ";
                            s += ((cvar.Flags & CVAR.ARCHIVE) != 0) ? "AR " : "   ";
                            s += ((cvar.Flags & CVAR.MODIFIED) != 0) ? "MO " : "   ";
                            s += "\n";
                            Printf(s);
                        }
                        break;
                    }
            }

            Printf($"\n{cvarList.Count} cvars listed\n\n");
            Printf("listCvar [search string]          = list cvar values\n"
                + "listCvar -help [search string]    = list cvar descriptions\n"
                + "listCvar -type [search string]    = list cvar types\n"
                + "listCvar -flags [search string]   = list cvar flags\n");
        }

        static void List_f(CmdArgs args)
            => ListByFlags(args, CVAR.ALL);

        static void Restart_f(CmdArgs args)
        {
            for (var i = 0; i < cvarSystemLocal.cvars.Count; i++)
            {
                var cvar = cvarSystemLocal.cvars.Values.ElementAt(i);

                // don't mess with rom values
                if ((cvar.flags & (CVAR.ROM | CVAR.INIT)) != 0) continue;

                // throw out any variables the user created
                if ((cvar.flags & CVAR.STATIC) == 0) { cvarSystemLocal.cvars.Remove(cvar.nameString); i--; continue; }

                cvar.Reset();
            }
        }

        static string CreateColumn(string text, int columnWidth, string indent, StringBuilder b)
        {
            int i, lastLine;
            b.Clear();
            for (lastLine = i = 0; text[i] != '\0'; i++)
                if (i - lastLine >= columnWidth || text[i] == '\n')
                {
                    while (i > 0 && text[i] > ' ' && text[i] != '/' && text[i] != ',' && text[i] != '\\') i--;
                    while (lastLine < i) b.Append(text[lastLine++]);
                    b.Append(indent);
                    lastLine++;
                }
            while (lastLine < i) b.Append(text[lastLine++]);
            return b.ToString();
        }
    }
}