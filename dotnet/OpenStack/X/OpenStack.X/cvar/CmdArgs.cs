using System.Text;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack
{
    // argument completion function
    public delegate void ArgCompletion(CmdArgs args, Action<string> callback);

    public class CmdArgs
    {
        public static readonly CmdArgs Empty = new();
        const int MAX_COMMAND_ARGS = 64;

        int argc; // number of arguments
        string[] argv = new string[MAX_COMMAND_ARGS]; // points into tokenized

        public CmdArgs(CmdArgs args)
        {
            argc = args.argc;
            argv = (string[])argv.Clone();
        }
        public CmdArgs()
            => argc = 0;
        public CmdArgs(string text, bool keepAsStrings)
            => TokenizeString(text, keepAsStrings);

        /// <summary>
        /// The functions that execute commands get their parameters with these functions.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count
            => argc;

        /// <summary>
        /// Argv() will return an empty string, not NULL if arg >= argc.
        /// </summary>
        /// <value>
        /// The <see cref="System.String"/>.
        /// </value>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        public string this[int arg]
            => arg >= 0 && arg < argc ? argv[arg] : string.Empty;

        /// <summary>
        /// Returns a single string containing argv(start) to argv(end) escapeArgs is a fugly way to put the string back into a state ready to tokenize again
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="escapeArgs">if set to <c>true</c> [escape arguments].</param>
        /// <returns></returns>
        public string Args(int start = 1, int end = -1, bool escapeArgs = false)
        {
            if (end < 0) end = argc - 1;
            else if (end >= argc) end = argc - 1;

            var b = new StringBuilder();
            if (escapeArgs) b.Append('"');
            for (var i = start; i <= end; i++)
            {
                if (i > start) b.Append(escapeArgs ? "\" \"" : " ");
                var buf = argv[i];
                if (escapeArgs && buf.Contains('\\'))
                {
                    var p = 0;
                    while (buf[p] != '\0') { b.Append(buf[p] != '\\' ? buf[p] : "\\\\"); p++; }
                }
                else b.Append(buf);
            }
            if (escapeArgs) b.Append('"');
            return b.ToString();
        }

        /// <summary>
        /// Takes a null terminated string and breaks the string up into arg tokens.
        /// Does not need to be /n terminated.
        /// Set keepAsStrings to true to only seperate tokens from whitespace and comments, ignoring punctuation
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="keepAsStrings">if set to <c>true</c> [keep as strings].</param>
        public void TokenizeString(string text, bool keepAsStrings)
        {
            Lexer lex = new();

            // clear previous args
            argc = 0;

            if (text == null) return;

            lex.LoadMemory(text, "CmdSystemLocal::TokenizeString");
            lex.Flags = LEXFL.NOERRORS
                        | LEXFL.NOWARNINGS
                        | LEXFL.NOSTRINGCONCAT
                        | LEXFL.ALLOWPATHNAMES
                        | LEXFL.NOSTRINGESCAPECHARS
                        | LEXFL.ALLOWIPADDRESSES
                        | (keepAsStrings ? LEXFL.ONLYSTRINGS : 0);

            while (true)
            {
                if (argc == MAX_COMMAND_ARGS) return; // this is usually something malicious

                if (!lex.ReadToken(out var token)) return;

                // check for negative numbers
                if (!keepAsStrings && token == "-" &&
                    lex.CheckTokenType(TT.NUMBER, 0, out var number))
                    token = "-" + number;

                // check for cvar expansion
                if (token == "$")
                {
                    if (!lex.ReadToken(out token)) return;
                    token = cvarSystem != null
                        ? cvarSystem.GetCVarString(token)
                        : "<unknown>";
                }

                // add token
                argv[argc] = token;
                argc++;
            }
        }

        public void AppendArg(string text) => argv[argc++] = text;

        public void Clear() => argc = 0;

        public string[] GetArgs(out int argc)
        {
            argc = this.argc;
            return argv;
        }

        // Default argument completion functions.
        public static void ArgCompletion_Boolean(CmdArgs args, Action<string> callback)
        {
            callback($"{args[0]} 0");
            callback($"{args[0]} 1");
        }

        public static ArgCompletion ArgCompletion_Integer(int min, int max) => (CmdArgs args, Action<string> callback) =>
        {
            for (var i = min; i <= max; i++) callback($"{args[0]} {i}");
        };

        public static ArgCompletion ArgCompletion_String(string[] strings) => (CmdArgs args, Action<string> callback) =>
        {
            for (var i = 0; i < strings.Length; i++) callback($"{args[0]} {strings[i]}");
        };

        public static void ArgCompletion_FileName(CmdArgs args, Action<string> callback) =>
            cmdSystem.ArgCompletion_FolderExtension(args, callback, "/", true, "", null);

        public static void ArgCompletion_MapName(CmdArgs args, Action<string> callback) =>
            cmdSystem.ArgCompletion_FolderExtension(args, callback, "maps/", true, ".map", null);
        public static void ArgCompletion_ModelName(CmdArgs args, Action<string> callback) =>
            cmdSystem.ArgCompletion_FolderExtension(args, callback, "models/", false, ".lwo", ".ase", ".md5mesh", ".ma", null);
        public static void ArgCompletion_SoundName(CmdArgs args, Action<string> callback) =>
            cmdSystem.ArgCompletion_FolderExtension(args, callback, "sound/", false, ".wav", ".ogg", null);
        public static void ArgCompletion_ImageName(CmdArgs args, Action<string> callback) =>
            cmdSystem.ArgCompletion_FolderExtension(args, callback, "/", false, ".tga", ".dds", ".jpg", ".pcx", null);
        public static void ArgCompletion_VideoName(CmdArgs args, Action<string> callback) =>
            cmdSystem.ArgCompletion_FolderExtension(args, callback, "video/", false, ".roq", null);
        public static void ArgCompletion_ConfigName(CmdArgs args, Action<string> callback) =>
            cmdSystem.ArgCompletion_FolderExtension(args, callback, "/", true, ".cfg", null);
        public static void ArgCompletion_SaveGame(CmdArgs args, Action<string> callback) =>
            cmdSystem.ArgCompletion_FolderExtension(args, callback, "SaveGames/", true, ".save", null);
        public static void ArgCompletion_DemoName(CmdArgs args, Action<string> callback) =>
            cmdSystem.ArgCompletion_FolderExtension(args, callback, "demos/", true, ".demo", null);
    }
}