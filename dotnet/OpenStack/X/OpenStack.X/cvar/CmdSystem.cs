using System.Collections.Generic;
using System.IO;
using System.Text;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.Platform;

namespace System.NumericsX.OpenStack
{
    // command flags
    [Flags]
    public enum CMD_FL
    {
        ALL = -1,
        CHEAT = 1 << 0,    // command is considered a cheat
        SYSTEM = 1 << 1,   // system command
        RENDERER = 1 << 2, // renderer command
        SOUND = 1 << 3,    // sound command
        GAME = 1 << 4, // game command
        TOOL = 1 << 5, // tool command
    }

    // parameters for command buffer stuffing
    public enum CMD_EXEC
    {
        NOW,                        // don't return until completed
        INSERT,                 // insert at current position, but don't run yet
        APPEND                      // add to end of the command buffer (normal case)
    }

    // command function
    public delegate void CmdFunction(CmdArgs args);

    public interface ICmdSystem
    {
        // Registers a command and the function to call for it.
        void AddCommand(string cmdName, CmdFunction function, CMD_FL flags, string description, ArgCompletion argCompletion = null);
        // Removes a command.
        void RemoveCommand(string cmdName);
        // Remove all commands with one of the flags set.
        void RemoveFlaggedCommands(CMD_FL flags);

        // Command and argument completion using callback for each valid string.
        void CommandCompletion(Action<string> callback);
        void ArgCompletion(string cmdString, Action<string> callback);

        // Adds command text to the command buffer, does not add a final \n
        void BufferCommandText(CMD_EXEC exec, string text);
        // Pulls off \n \r or ; terminated lines of text from the command buffer and
        // executes the commands. Stops when the buffer is empty.
        // Normally called once per frame, but may be explicitly invoked.
        void ExecuteCommandBuffer();

        // Base for path/file auto-completion.
        void ArgCompletion_FolderExtension(CmdArgs args, Action<string> callback, string folder, bool stripFolder, params string[] extensions);

        // Adds to the command buffer in tokenized form ( CMD_EXEC_NOW or CMD_EXEC_APPEND only )
        void BufferCommandArgs(CMD_EXEC exec, CmdArgs args);

        // Setup a reloadEngine to happen on next command run, and give a command to execute after reload
        void SetupReloadEngine(CmdArgs args);
        bool PostReloadEngine();
    }

    class CommandDef
    {
        public CommandDef next;
        public string name;
        public CmdFunction function;
        public ArgCompletion argCompletion;
        public CMD_FL flags;
        public string description;
    }

    internal class CmdSystemLocal : ICmdSystem
    {
        //const int MAX_CMD_BUFFER = 0x10000;

        CommandDef commands;

        int wait;
        //int textLength;
        StringBuilder textBuf = new();

        string completionString = "*";
        List<string> completionParms = new();

        // piggybacks on the text buffer, avoids tokenize again and screwing it up
        List<CmdArgs> tokenizedCmds = new();

        // a command stored to be executed after a reloadEngine and all associated commands have been processed
        CmdArgs postReload;

        public CmdSystemLocal()
        {
            AddCommand("listCmds", List_f, CMD_FL.SYSTEM, "lists commands");
            AddCommand("listSystemCmds", SystemList_f, CMD_FL.SYSTEM, "lists system commands");
            AddCommand("listRendererCmds", RendererList_f, CMD_FL.SYSTEM, "lists renderer commands");
            AddCommand("listSoundCmds", SoundList_f, CMD_FL.SYSTEM, "lists sound commands");
            AddCommand("listGameCmds", GameList_f, CMD_FL.SYSTEM, "lists game commands");
            AddCommand("listToolCmds", ToolList_f, CMD_FL.SYSTEM, "lists tool commands");
            AddCommand("exec", Exec_f, CMD_FL.SYSTEM, "executes a config file", CmdArgs.ArgCompletion_ConfigName);
            AddCommand("vstr", Vstr_f, CMD_FL.SYSTEM, "inserts the current value of a cvar as command text");
            AddCommand("echo", Echo_f, CMD_FL.SYSTEM, "prints text");
            AddCommand("parse", Parse_f, CMD_FL.SYSTEM, "prints tokenized string");
            AddCommand("wait", Wait_f, CMD_FL.SYSTEM, "delays remaining buffered commands one or more frames");
        }

        public void Dispose() { }

        public void AddCommand(string cmdName, CmdFunction function, CMD_FL flags, string description, ArgCompletion argCompletion = null)
        {
            CommandDef cmd;

            // fail if the command already exists
            for (cmd = commands; cmd != null; cmd = cmd.next) if (cmdName == cmd.name && function != cmd.function) { Printf($"CmdSystemLocal::AddCommand: {cmdName} already defined\n"); return; }

            cmd = new CommandDef
            {
                name = cmdName,
                function = function,
                argCompletion = argCompletion,
                flags = flags,
                description = description,
                next = commands
            };
            commands = cmd;
        }

        public void RemoveCommand(string cmdName)
        {
            CommandDef cmd; ref CommandDef last = ref commands;

            for (cmd = last; cmd != null; cmd = last)
            {
                if (cmdName == cmd.name) { last = cmd.next; return; }
                last = cmd.next;
            }
        }

        public void RemoveFlaggedCommands(CMD_FL flags)
        {
            CommandDef cmd; ref CommandDef last = ref commands;

            for (cmd = last; cmd != null; cmd = last)
            {
                if ((cmd.flags & flags) != 0) { last = cmd.next; continue; }
                last = cmd.next;
            }
        }

        public void CommandCompletion(Action<string> callback)
        {
            CommandDef cmd;

            for (cmd = commands; cmd != null; cmd = cmd.next) callback(cmd.name);
        }

        public void ArgCompletion(string cmdString, Action<string> callback)
        {
            CommandDef cmd; CmdArgs args = new();

            args.TokenizeString(cmdString, false);

            for (cmd = commands; cmd != null; cmd = cmd.next)
            {
                if (cmd.argCompletion == null) continue;
                if (args[0] == cmd.name) { cmd.argCompletion(args, callback); break; }
            }
        }

        public void BufferCommandText(CMD_EXEC exec, string text)
        {
            switch (exec)
            {
                case CMD_EXEC.NOW: ExecuteCommandText(text); break;
                case CMD_EXEC.INSERT: InsertCommandText(text); break;
                case CMD_EXEC.APPEND: AppendCommandText(text); break;
                default: FatalError("CmdSystemLocal::BufferCommandText: bad exec type"); break;
            }
        }

        public void ExecuteCommandBuffer()
        {
            int i; string text; int quotes; CmdArgs args = new();

            while (textBuf.Length != 0)
            {
                // skip out while text still remains in buffer, leaving it for next frame
                if (wait != 0) { wait--; break; }

                // find a \n or ; line break
                quotes = 0;
                for (i = 0; i < textBuf.Length; i++)
                {
                    if (textBuf[i] == '"') quotes++;
                    if ((quotes & 1) == 0 && textBuf[i] == ';') break;  // don't break if inside a quoted string
                    if (textBuf[i] == '\n' || textBuf[i] == '\r') break;
                }

                text = textBuf.ToString(0, i);

                if (text == "_execTokenized") { args = tokenizedCmds[0]; tokenizedCmds.RemoveAt(0); }
                else args.TokenizeString(text, false);

                // delete the text from the command buffer and move remaining commands down this is necessary because commands (exec) can insert data at the beginning of the text buffer

                if (i == textBuf.Length) textBuf.Length = 0;
                else { i++; textBuf.Remove(0, i); }

                // execute the command line that we have already tokenized
                ExecuteTokenizedString(args);
            }
        }

        public void ArgCompletion_FolderExtension(CmdArgs args, Action<string> callback, string folder, bool stripFolder, params string[] extensions)
        {
            int i;

            var s = $"{args[0]} {args[1]}";

            if (!string.Equals(s, completionString, StringComparison.OrdinalIgnoreCase))
            {
                string parm, path; FileList names;

                completionString = s;
                completionParms.Clear();

                parm = args[1];
                path = Path.GetFileName(parm);
                if (stripFolder || path.Length == 0) path = folder + path;
                path = path.TrimEnd('/');

                // list folders
                names = fileSystem.ListFiles(path, "/", true, true);
                for (i = 0; i < names.NumFiles; i++)
                {
                    var name = names.GetFile(i);
                    name = name.Trim(stripFolder ? folder : "/");
                    name = $"{args[0]} {name}/";
                    completionParms.Add(name);
                }
                fileSystem.FreeFileList(names);

                // list files
                foreach (var extension in extensions)
                {
                    names = fileSystem.ListFiles(path, extension, true, true);
                    for (i = 0; i < names.NumFiles; i++)
                    {
                        var name = names.GetFile(i);
                        name = name.Trim(stripFolder ? folder : "/");
                        name = $"{args[0]} {name}";
                        completionParms.Add(name);
                    }
                    fileSystem.FreeFileList(names);
                }
            }
            for (i = 0; i < completionParms.Count; i++) callback(completionParms[i]);
        }

        public void BufferCommandArgs(CMD_EXEC exec, CmdArgs args)
        {
            switch (exec)
            {
                case CMD_EXEC.NOW: ExecuteTokenizedString(args); break;
                case CMD_EXEC.APPEND: AppendCommandText("_execTokenized\n"); tokenizedCmds.Add(args); break;
                default: FatalError("CmdSystemLocal::BufferCommandArgs: bad exec type"); break;
            }
        }

        public void SetupReloadEngine(CmdArgs args)
        {
            BufferCommandText(CMD_EXEC.APPEND, "reloadEngine\n");
            postReload = args;
        }

        public bool PostReloadEngine()
        {
            if (postReload.Count == 0) return false;
            BufferCommandArgs(CMD_EXEC.APPEND, postReload);
            postReload.Clear();
            return true;
        }

        public void SetWait(int numFrames)
            => wait = numFrames;

        public CommandDef Commands
            => commands;

        void ExecuteTokenizedString(CmdArgs args)
        {
            CommandDef cmd; ref CommandDef prev = ref commands;

            // execute the command line
            if (args.Count == 0) return;     // no tokens

            // check registered command functions
            for (; prev != null; prev = cmd.next)
            {
                cmd = prev;
                if (string.Equals(args[0], cmd.name, StringComparison.OrdinalIgnoreCase))
                {
                    // rearrange the links so that the command will be near the head of the list next time it is used
                    prev = cmd.next;
                    cmd.next = commands;
                    commands = cmd;

                    if ((cmd.flags & (CMD_FL.CHEAT | CMD_FL.TOOL)) != 0 && Session_IsMultiplayer != null && Session_IsMultiplayer() && !cvarSystem.GetCVarBool("net_allowCheats")) { Printf($"Command '{cmd.name}' not valid in multiplayer mode.\n"); return; }
                    // perform the action
                    if (cmd.function == null) break;
                    cmd.function(args);
                    return;
                }
            }

            // check cvars
            if (cvarSystem.Command(args)) return;

            Printf($"Unknown command '{args[0]}'\n");
        }

        void ExecuteCommandText(string text) => ExecuteTokenizedString(new CmdArgs(text, false));

        void InsertCommandText(string text)
            => textBuf.Insert(0, $"{text}\n");

        void AppendCommandText(string text)
            => textBuf.Append(text);

        static void ListByFlags(CmdArgs args, CMD_FL flags)
        {
            var match = args.Count > 1 ? args.Args(1, -1).Replace(" ", "") : string.Empty;

            CommandDef cmd;
            var cmdList = new List<CommandDef>();
            for (cmd = cmdSystemLocal.Commands; cmd != null; cmd = cmd.next)
            {
                if ((cmd.flags & flags) == 0) continue;
                if (match.Length != 0 && cmd.name.Filter(match, false)) continue;

                cmdList.Add(cmd);
            }

            cmdList.Sort();

            for (var i = 0; i < cmdList.Count; i++) { cmd = cmdList[i]; Printf($"  {cmd.name:-21} {cmd.description}\n"); }
            Printf($"{cmdList.Count} commands\n");
        }

        static void List_f(CmdArgs args) => ListByFlags(args, CMD_FL.ALL);
        static void SystemList_f(CmdArgs args) => ListByFlags(args, CMD_FL.SYSTEM);
        static void RendererList_f(CmdArgs args) => ListByFlags(args, CMD_FL.RENDERER);
        static void SoundList_f(CmdArgs args) => ListByFlags(args, CMD_FL.SOUND);
        static void GameList_f(CmdArgs args) => ListByFlags(args, CMD_FL.GAME);
        static void ToolList_f(CmdArgs args) => ListByFlags(args, CMD_FL.TOOL);

        static void Exec_f(CmdArgs args)
        {
            if (args.Count != 2) { Printf("exec <filename> : execute a script file\n"); return; }

            var filename = args[1];
            filename = Path.GetExtension(filename).Length != 0 ? filename : $"{filename}.cfg";
            fileSystem.ReadFile(filename, out var f, out var _);
            if (f == null) { Printf($"couldn't exec {args[1]}\n"); return; }
            Printf($"execing {args[1]}\n");

            cmdSystemLocal.BufferCommandText(CMD_EXEC.INSERT, Encoding.ASCII.GetString(f));

            fileSystem.FreeFile(f);
        }

        static void Vstr_f(CmdArgs args)
        {
            if (args.Count != 2) { Printf("vstr <variablename> : execute a variable command\n"); return; }

            var v = cvarSystem.GetCVarString(args[1]);

            cmdSystemLocal.BufferCommandText(CMD_EXEC.APPEND, $"{v}\n");
        }
        static void Echo_f(CmdArgs args)
        {
            for (var i = 1; i < args.Count; i++) Printf($"{args[i]} ");
            Printf("\n");
        }

        static void Parse_f(CmdArgs args)
        {
            for (var i = 0; i < args.Count; i++) Printf($"{i}: {args[i]}\n");
        }

        static void Wait_f(CmdArgs args)
            => cmdSystemLocal.SetWait(args.Count == 2 ? int.TryParse(args[1], out var z) ? z : 1 : 1);

        static void PrintMemInfo_f(CmdArgs args) { }
    }
}