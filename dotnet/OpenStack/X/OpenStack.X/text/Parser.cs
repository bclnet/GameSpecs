#define DEBUG_EVAL
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.NumericsX.OpenStack.Lexer;
using static System.NumericsX.OpenStack.Token;
using static System.NumericsX.Platform;

namespace System.NumericsX.OpenStack
{
    // macro definitions
    public class Define
    {
        public string name;                     // define name
        public int flags;                       // define flags
        public int builtin;                 // > 0 if builtin define
        public int numparms;                    // number of define parameters
        public Token parms;                     // define parameters
        public Token tokens;                        // macro tokens (possibly containing parm tokens)
        public Define next;                     // next defined macro in a list
        public Define hashnext;                 // next define in the hash chain
    }

    // indents used for conditional compilation directives: #if, #else, #elif, #ifdef, #ifndef
    public class Indent
    {
        public int type;                       // indent type
        public int skip;                       // true if skipping current indent
        public Lexer script;                       // script the indent was in
        public Indent next;                        // next indent on the indent stack
    }

    public class Parser
    {
        const int MAX_DEFINEPARMS = 128;
        const int TOKEN_FL_RECURSIVE_DEFINE = 1;

        public const int DEFINE_FIXED = 0x0001;

        public const int BUILTIN_LINE = 1;
        public const int BUILTIN_FILE = 2;
        public const int BUILTIN_DATE = 3;
        public const int BUILTIN_TIME = 4;
        public const int BUILTIN_STDC = 5;

        public const int INDENT_IF = 0x0001;
        public const int INDENT_ELSE = 0x0002;
        public const int INDENT_ELIF = 0x0004;
        public const int INDENT_IFDEF = 0x0008;
        public const int INDENT_IFNDEF = 0x0010;

        bool loaded;                     // set when a source file is loaded from file or memory
        string filename;                 // file name of the script
        string includepath;              // path to include files
        bool osPath;                        // true if the file was loaded from an OS path
        (string p, int n)[] punctuations;          // punctuations to use
        LEXFL flags;                      // flags used for script parsing
        Lexer scriptstack;               // stack with scripts of the source
        Token tokens;                        // tokens to read first
        Dictionary<string, Define> defines;                  // hash chain with defines
        Indent indentstack;              // stack with indents
        int skip;                       // > 0 if skipping conditional code
        int marker_p;

        static Define globaldefines;             // list with global defines added to every source loaded

        // constructor
        public Parser()
        {
            this.loaded = false;
            this.osPath = false;
            this.punctuations = null;
            this.flags = 0;
            this.scriptstack = null;
            this.indentstack = null;
            this.defines = null;
            this.tokens = null;
            this.marker_p = 0;
        }
        public Parser(LEXFL flags)
        {
            this.loaded = false;
            this.osPath = false;
            this.punctuations = null;
            this.flags = flags;
            this.scriptstack = null;
            this.indentstack = null;
            this.defines = null;
            this.tokens = null;
            this.marker_p = 0;
        }
        public Parser(string filename, LEXFL flags = 0, bool osPath = false)
        {
            this.loaded = false;
            this.osPath = true;
            this.punctuations = null;
            this.flags = flags;
            this.scriptstack = null;
            this.indentstack = null;
            this.defines = null;
            this.tokens = null;
            this.marker_p = 0;
            LoadFile(filename, osPath);
        }
        public Parser(string ptr, int length, string name, LEXFL flags = 0)
        {
            this.loaded = false;
            this.osPath = false;
            this.punctuations = null;
            this.flags = flags;
            this.scriptstack = null;
            this.indentstack = null;
            this.defines = null;
            this.tokens = null;
            this.marker_p = 0;
            LoadMemory(ptr, length, name);
        }

        // load a source file
        public bool LoadFile(string filename, bool osPath = false)
        {
            Lexer script;

            if (loaded) { FatalError("Parser::loadFile: another source already loaded"); return false; }
            script = new Lexer(filename, 0, osPath);
            if (!script.IsLoaded) return false;
            script.Flags = flags;
            script.SetPunctuations(punctuations);
            script.next = null;
            this.osPath = osPath;
            this.filename = filename;
            scriptstack = script;
            tokens = null;
            indentstack = null;
            skip = 0;
            loaded = true;

            if (defines == null) { defines = new(); AddGlobalDefinesToSource(); }
            return true;
        }

        // load a source from the given memory with the given length
        public bool LoadMemory(string ptr, int length, string name)
        {
            Lexer script;

            if (loaded) { FatalError("Parser::loadMemory: another source already loaded"); return false; }
            script = new Lexer(ptr, length, name);
            if (!script.IsLoaded) return false;
            script.Flags = flags;
            script.SetPunctuations(punctuations);
            script.next = null;
            filename = name;
            scriptstack = script;
            tokens = null;
            indentstack = null;
            skip = 0;
            loaded = true;

            if (defines == null) { defines = new(); AddGlobalDefinesToSource(); }
            return true;
        }

        // free the current source
        public void FreeSource(bool keepDefines = false)
        {
            if (!keepDefines) defines = null;
            loaded = false;
        }

        // returns true if a source is loaded
        public bool IsLoaded =>
            loaded;

        // read a token from the source
        public bool ReadToken(out Token token)
        {
            while (true)
            {
                if (!ReadSourceToken(out token)) return false;
                // check for precompiler directives
                if (token.type == TT.PUNCTUATION && token[0] == '#' && token[1] == '\0')
                {
                    // read the precompiler directive
                    if (!ReadDirective()) return false;
                    continue;
                }
                // if skipping source because of conditional compilation
                if (skip != 0) continue;
                // recursively concatenate strings that are behind each other still resolving defines
                if (token.type == TT.STRING && (scriptstack.Flags & LEXFL.NOSTRINGCONCAT) == 0)
                    if (ReadToken(out var newtoken))
                    {
                        if (newtoken.type == TT.STRING) token += newtoken;
                        else UnreadSourceToken(newtoken);
                    }
                //
                if ((scriptstack.Flags & LEXFL.NODOLLARPRECOMPILE) == 0)
                {
                    // check for special precompiler directives
                    if (token.type == TT.PUNCTUATION && token[0] == '$' && token[1] == '\0')
                        // read the precompiler directive
                        if (ReadDollarDirective()) continue;
                }
                // if the token is a name
                if (token.type == TT.NAME && (token.flags & TOKEN_FL_RECURSIVE_DEFINE) == 0)
                {
                    // check if the name is a define macro
                    var define = FindHashedDefine(defines, token);
                    // if it is a define macro
                    if (define != null)
                    {
                        // expand the defined macro
                        if (!ExpandDefineIntoSource(token, define)) return false;
                        continue;
                    }
                }
                // found a token
                return true;
            }
        }

        // expect a certain token, reads the token when available
        public bool ExpectTokenString(string s)
        {
            if (!ReadToken(out var token)) { Error($"couldn't find expected '{s}'"); return false; }
            if (token != s) { Error($"expected '{s}' but found '{token}'"); return false; }
            return true;
        }

        // expect a certain token type
        public bool ExpectTokenType(TT type, int subtype, out Token token)
        {
            string str;

            if (!ReadToken(out token)) { Error("couldn't read expected token"); return false; }

            if (token.type != type)
            {
                str = type switch
                {
                    TT.STRING => "string",
                    TT.LITERAL => "literal",
                    TT.NUMBER => "number",
                    TT.NAME => "name",
                    TT.PUNCTUATION => "punctuation",
                    _ => "unknown type",
                };
                Error($"expected a {str} but found '{token}'");
                return false;
            }
            if (token.type == TT.NUMBER)
            {
                if ((token.subtype & subtype) != subtype)
                {
                    str = string.Empty;
                    if ((subtype & TT_DECIMAL) != 0) str = "decimal ";
                    if ((subtype & TT_HEX) != 0) str = "hex ";
                    if ((subtype & TT_OCTAL) != 0) str = "octal ";
                    if ((subtype & TT_BINARY) != 0) str = "binary ";
                    if ((subtype & TT_UNSIGNED) != 0) str += "unsigned ";
                    if ((subtype & TT_LONG) != 0) str += "long ";
                    if ((subtype & TT_FLOAT) != 0) str += "float ";
                    if ((subtype & TT_INTEGER) != 0) str += "integer ";
                    str = str.TrimEnd(' ');
                    Error($"expected {str} but found '{token}'");
                    return false;
                }
            }
            else if (token.type == TT.PUNCTUATION)
            {
                if (subtype < 0) { Error("BUG: wrong punctuation subtype"); return false; }
                if (token.subtype != subtype) { Error($"expected '{scriptstack.GetPunctuationFromId(subtype)}' but found '{token}'"); return false; }
            }
            return true;
        }

        // expect a token
        public bool ExpectAnyToken(out Token token)
        {
            if (!ReadToken(out token)) { Error("couldn't read expected token"); return false; }
            return true;
        }

        // returns true if the next token equals the given string and removes the token from the source
        public bool CheckTokenString(string s)
        {
            if (!ReadToken(out var tok)) return false;
            // if the token is available
            if (tok == s) return true;

            UnreadSourceToken(tok);
            return false;
        }

        // returns true if the next token equals the given type and removes the token from the source
        public bool CheckTokenType(TT type, int subtype, out Token token)
        {
            if (!ReadToken(out var tok)) { token = default; return false; }
            //if the type matches
            if (tok.type == type && (tok.subtype & subtype) == subtype) { token = tok; return true; }

            UnreadSourceToken(tok);
            token = default;
            return false;
        }

        // returns true if the next token equals the given string but does not remove the token from the source
        public bool PeekTokenString(string s)
        {
            if (!ReadToken(out var tok)) return false;

            UnreadSourceToken(tok);
            // if the token is available
            return tok == s;
        }

        // returns true if the next token equals the given type but does not remove the token from the source
        public bool PeekTokenType(TT type, int subtype, out Token token)
        {
            if (!ReadToken(out var tok)) { token = default; return false; }

            UnreadSourceToken(tok);
            // if the type matches
            if (tok.type == type && (tok.subtype & subtype) == subtype) { token = tok; return true; }
            token = default;
            return false;
        }

        // skip tokens until the given token string is read
        public bool SkipUntilString(string s)
        {
            while (ReadToken(out var token)) if (token == s) return true;
            return false;
        }

        // skip the rest of the current line
        public bool SkipRestOfLine()
        {
            while (ReadToken(out var token)) if (token.linesCrossed != 0) { UnreadSourceToken(token); return true; }
            return false;
        }

        // skip the braced section
        // Skips until a matching close brace is found. Internal brace depths are properly skipped.
        public bool SkipBracedSection(bool parseFirstBrace = true)
        {
            var depth = parseFirstBrace ? 0 : 1;
            do
            {
                if (!ReadToken(out var token)) return false;
                if (token.type == TT.PUNCTUATION)
                {
                    if (token == "{") depth++;
                    else if (token == "}") depth--;
                }
            } while (depth != 0);
            return true;
        }

        // parse a braced section into a string
        // The next token should be an open brace. Parses until a matching close brace is found. Internal brace depths are properly skipped.
        public string ParseBracedSection(out string o, int tabs = -1)
        {
            int i, depth; bool doTabs = false;
            if (tabs >= 0) doTabs = true;

            o = string.Empty;
            if (!ExpectTokenString("{")) return o;
            o = "{";
            depth = 1;
            do
            {
                if (!ReadToken(out var token)) { Error("missing closing brace"); return o; }

                // if the token is on a new line
                for (i = 0; i < token.linesCrossed; i++) o += "\r\n";

                if (doTabs && token.linesCrossed != 0)
                {
                    i = tabs;
                    if (token[0] == '}' && i > 0) i--;
                    while (i-- > 0) o += "\t";
                }
                if (token.type == TT.PUNCTUATION)
                {
                    if (token[0] == '{') { depth++; if (doTabs) tabs++; }
                    else if (token[0] == '}') { depth--; if (doTabs) tabs--; }
                }

                if (token.type == TT.STRING) o += $"\"{token}\"";
                else o += token;
                o += " ";
            } while (depth != 0);
            return o;
        }

        // parse a braced section into a string, maintaining indents and newlines
        // The next token should be an open brace. Parses until a matching close brace is found. Maintains the exact formating of the braced section
        // FIXME: what about precompilation?
        public string ParseBracedSectionExact(out string o, int tabs = -1)
            => scriptstack.ParseBracedSectionExact(out o, tabs);

        // parse the rest of the line
        public string ParseRestOfLine(out string o)
        {
            o = string.Empty;
            while (ReadToken(out var token))
            {
                if (token.linesCrossed != 0) { UnreadSourceToken(token); break; }
                if (o.Length != 0) o += " ";
                o += token;
            }
            return o;
        }

        // unread the given token
        public void UnreadToken(Token token)
            => UnreadSourceToken(token);

        // read a token only if on the current line
        public bool ReadTokenOnLine(out Token token)
        {
            if (!ReadToken(out var tok)) { token = default; return false; }
            // if no lines were crossed before this token
            if (tok.linesCrossed == 0) { token = tok; return true; }
            //
            UnreadSourceToken(tok);
            token = default;
            return false;
        }

        // read a signed integer
        public int ParseInt()
        {
            if (!ReadToken(out var token)) { Error("couldn't read expected integer"); return 0; }
            if (token.type == TT.PUNCTUATION && token == "-") { ExpectTokenType(TT.NUMBER, TT_INTEGER, out token); return -token.IntValue; }
            else if (token.type != TT.NUMBER || token.subtype == TT_FLOAT) Error("expected integer value, found '{token}'");
            return token.IntValue;
        }

        // read a boolean
        public bool ParseBool()
        {
            if (!ExpectTokenType(TT.NUMBER, 0, out var token)) { Error("couldn't read expected boolean"); return false; }
            return token.IntValue != 0;
        }

        // read a floating point number
        public float ParseFloat()
        {
            if (!ReadToken(out var token)) { Error("couldn't read expected floating point number"); return 0f; }
            if (token.type == TT.PUNCTUATION && token == "-") { ExpectTokenType(TT.NUMBER, 0, out token); return -token.FloatValue; }
            else if (token.type != TT.NUMBER) Error("expected float value, found '{token}'");
            return token.FloatValue;
        }

        // parse matrices with floats
        public bool Parse1DMatrix(int x, float[] m, int offset = 0)
        {
            if (!ExpectTokenString("(")) return false;
            for (var i = 0; i < x; i++) m[offset + i] = ParseFloat();
            return ExpectTokenString(")");
        }

        public bool Parse2DMatrix(int y, int x, float[] m, int offset = 0)
        {
            if (!ExpectTokenString("(")) return false;
            for (var i = 0; i < y; i++) if (!Parse1DMatrix(x, m, i * x)) return false;
            return ExpectTokenString(")");
        }

        public bool Parse3DMatrix(int z, int y, int x, float[] m, int offset = 0)
        {
            if (!ExpectTokenString("(")) return false;
            for (var i = 0; i < z; i++) if (!Parse2DMatrix(y, x, m, i * x * y)) return false;
            return ExpectTokenString(")");
        }

        // get the white space before the last read token
        public int GetLastWhiteSpace(out string whiteSpace)
        {
            if (scriptstack != null) scriptstack.GetLastWhiteSpace(out whiteSpace);
            else whiteSpace = string.Empty;
            return whiteSpace.Length;
        }

        // Set a marker in the source file (there is only one marker)
        public void SetMarker()
            => marker_p = 0;

        // Get the string from the marker to the current position
        // FIXME: this is very bad code, the script isn't even garrenteed to still be around
        public void GetStringFromMarker(out string o, bool clean = false)
        {
            var buffer = scriptstack.buffer;
            if (marker_p == 0) marker_p = buffer.Length;
            var p = tokens != null
                ? tokens.whiteSpaceStart_p
                : scriptstack.script_p;

            var marker = buffer[..p];

            // if cleaning then reparse
            if (clean)
            {
                o = string.Empty;
                var temp = new Parser(marker, marker.Length, "temp", flags);
                while (temp.ReadToken(out var token)) o += token;
            }
            else o = marker;
        }

        // add a define to the source
        public bool AddDefine(string s)
        {
            var define = DefineFromString(s);
            if (define == null) return false;
            AddDefineToHash(define, defines);
            return true;
        }

        // add builtin defines
        static readonly (string s, int i)[] AddBuiltinDefines_builtin = new[] {
            ( "__LINE__", BUILTIN_LINE ),
            ( "__FILE__", BUILTIN_FILE ),
            ( "__DATE__", BUILTIN_DATE ),
            ( "__TIME__", BUILTIN_TIME ),
            ( "__STDC__", BUILTIN_STDC ),
            ( null, 0 )
        };
        public void AddBuiltinDefines()
        {
            for (var i = 0; AddBuiltinDefines_builtin[i].s != null; i++)
                // add the define to the source
                AddDefineToHash(new Define
                {
                    name = AddBuiltinDefines_builtin[i].s,
                    flags = DEFINE_FIXED,
                    builtin = AddBuiltinDefines_builtin[i].i,
                    numparms = 0,
                    parms = null,
                    tokens = null
                }, defines);
        }

        // set the source include path
        public void SetIncludePath(string path)
        {
            includepath = path;
            // add trailing path seperator
            if (includepath[^1] != '\\' && includepath[^1] != '/')
                includepath += Path.DirectorySeparatorChar;
        }

        // set the punctuation set
        public void SetPunctuations((string p, int n)[] p)
            => punctuations = p;

        // returns a pointer to the punctuation with the given id
        public string GetPunctuationFromId(int id)
        {
            if (punctuations == null) return new Lexer().GetPunctuationFromId(id);

            for (var i = 0; punctuations[i].p != null; i++) if (punctuations[i].n == id) return punctuations[i].p;
            return "unknown punctuation";
        }

        // get the id for the given punctuation
        public int GetPunctuationId(string p)
        {
            if (punctuations == null) return new Lexer().GetPunctuationId(p);

            for (var i = 0; punctuations[i].p != null; i++) if (punctuations[i].p == p) return punctuations[i].n;
            return 0;
        }

        // get or set lexer flags
        public LEXFL Flags
        {
            get => flags;
            set { flags = value; for (var s = scriptstack; s != null; s = s.next) s.Flags = flags; }
        }

        // returns the current filename
        public string FileName
            => scriptstack != null ? scriptstack.FileName : string.Empty;

        // get current offset in current script
        public int FileOffset
            => scriptstack != null ? scriptstack.FileOffset : 0;

        // get file time for current script
        public DateTime FileTime
            => scriptstack != null ? scriptstack.FileTime : DateTime.MinValue;

        // returns the current line number
        public int LineNum
            => scriptstack != null ? scriptstack.LineNum : 0;

        // print an error message
        public void Error(string str)
            => scriptstack?.Error(str);

        // print a warning message
        public void Warning(string str)
            => scriptstack?.Warning(str);

        // add a global define that will be added to all opened sources
        public static bool AddGlobalDefine(string s)
        {
            var define = DefineFromString(s);
            if (define == null) return false;
            define.next = globaldefines;
            globaldefines = define;
            return true;
        }

        // remove the given global define
        public static bool RemoveGlobalDefine(string name)
        {
            Define d, prev;
            for (prev = null, d = globaldefines; d != null; prev = d, d = d.next) if (d.name == name) break;
            if (d != null)
            {
                if (prev != null) prev.next = d.next;
                else globaldefines = d.next;
                return true;
            }
            return false;
        }

        // remove all global defines
        public static void RemoveAllGlobalDefines()
        {
            for (var define = globaldefines; define != null; define = globaldefines) globaldefines = globaldefines.next;
        }

        // set the base folder to load files from
        public static void SetBaseFolder(string path)
            => SetBaseFolder(path);

        void PushIndent(int type, int skip)
        {
            var indent = new Indent
            {
                type = type,
                script = scriptstack,
                skip = skip != 0 ? 1 : 0,
            };
            this.skip += indent.skip;
            indent.next = indentstack;
            indentstack = indent;
        }

        void PopIndent(out int type, out int skip)
        {
            type = 0; skip = 0;
            var indent = indentstack;
            if (indent == null) return;

            // must be an indent from the current script
            if (indentstack.script != scriptstack) return;

            type = indent.type;
            skip = indent.skip;
            indentstack = indentstack.next;
            skip -= indent.skip;
        }

        void PushScript(Lexer script)
        {
            for (var s = scriptstack; s != null; s = s.next) if (string.Equals(s.FileName, script.FileName, StringComparison.OrdinalIgnoreCase)) { Warning($"'{script.FileName}' recursively included"); return; }
            // push the script on the script stack
            script.next = scriptstack;
            scriptstack = script;
        }

        bool ReadSourceToken(out Token token)
        {
            Token t; Lexer script; int changedScript;

            if (scriptstack == null) { FatalError("Parser::ReadSourceToken: not loaded"); token = null; return false; }
            changedScript = 0;
            // if there's no token already available
            while (tokens == null)
            {
                // if there's a token to read from the script
                if (scriptstack.ReadToken(out token))
                {
                    token.linesCrossed += changedScript;

                    // set the marker based on the start of the token read in
                    if (marker_p == 0) marker_p = token.whiteSpaceEnd_p;
                    return true;
                }
                // if at the end of the script
                if (scriptstack.EndOfFile())
                {
                    // remove all indents of the script
                    while (indentstack != null && indentstack.script == scriptstack) { Warning("missing #endif"); PopIndent(out var _, out var _); }
                    changedScript = 1;
                }
                // if this was the initial script
                if (scriptstack.next == null) return false;
                // remove the script and return to the previous one
                script = scriptstack;
                scriptstack = scriptstack.next;
            }
            // copy the already available token
            token = tokens;
            // remove the token from the source
            t = tokens;
            tokens = tokens.next;
            return true;
        }

        // reads a token from the current line, continues reading on the next line only if a backslash '\' is found
        bool ReadLine(out Token token)
        {
            var crossline = 0;
            do
            {
                if (!ReadSourceToken(out token)) return false;

                if (token.linesCrossed > crossline) { UnreadSourceToken(token); return false; }
                crossline = 1;
            } while (token == "\\");
            return true;
        }

        bool UnreadSourceToken(Token token)
        {
            tokens = new Token(token) { next = tokens };
            return true;
        }

        bool ReadDefineParms(Define define, Token[] parms, int maxparms)
        {
            Define newdefine; Token t, last; int i, done, lastcomma, numparms, indent;

            if (!ReadSourceToken(out var token)) { Error($"define '{define.name}' missing parameters"); return false; }

            if (define.numparms > maxparms) { Error($"define with more than {maxparms} parameters"); return false; }

            for (i = 0; i < define.numparms; i++) parms[i] = null;
            // if no leading "("
            if (token != "(") { UnreadSourceToken(token); Error($"define '{define.name}' missing parameters"); return false; }
            // read the define parameters
            for (done = 0, numparms = 0, indent = 1; done == 0;)
            {
                if (numparms >= maxparms) { Error($"define '{define.name}' with too many parameters"); return false; }
                parms[numparms] = null;
                lastcomma = 1;
                last = null;
                while (done == 0)
                {
                    if (!ReadSourceToken(out token)) { Error($"define '{define.name}' incomplete"); return false; }

                    if (token == ",")
                    {
                        if (indent <= 1)
                        {
                            if (lastcomma != 0) Warning("too many comma's");
                            if (numparms >= define.numparms) Warning("too many define parameters");
                            lastcomma = 1;
                            break;
                        }
                    }
                    else if (token == "(") indent++;
                    else if (token == ")")
                    {
                        indent--;
                        if (indent <= 0)
                        {
                            if (parms[define.numparms - 1] == null) Warning("too few define parameters");
                            done = 1;
                            break;
                        }
                    }
                    else if (token.type == TT.NAME)
                    {
                        newdefine = FindHashedDefine(defines, token);
                        if (newdefine != null)
                        {
                            if (!ExpandDefineIntoSource(token, newdefine)) return false;
                            continue;
                        }
                    }

                    lastcomma = 0;

                    if (numparms < define.numparms)
                    {
                        t = new Token(token) { next = null };
                        if (last != null) last.next = t;
                        else parms[numparms] = t;
                        last = t;
                    }
                }
                numparms++;
            }
            return true;
        }

        bool StringizeTokens(Token tokens, out Token token)
        {
            token = new(string.Empty);
            token.type = TT.STRING;
            token.whiteSpaceStart_p = 0;
            token.whiteSpaceEnd_p = 0;
            token = string.Empty;
            for (var t = tokens; t != null; t = t.next) token += t;
            return true;
        }

        bool MergeTokens(Token t1, Token t2)
        {
            // merging of a name with a name or number
            if (t1.type == TT.NAME && (t2.type == TT.NAME || (t2.type == TT.NUMBER && (t2.subtype & TT_FLOAT) == 0))) { t1 += t2; return true; }
            // merging of two strings
            if (t1.type == TT.STRING && t2.type == TT.STRING) { t1 += t2; return true; }
            // merging of two numbers
            if (t1.type == TT.NUMBER && t2.type == TT.NUMBER &&
                (t1.subtype & (TT_HEX | TT_BINARY)) == 0 && (t2.subtype & (TT_HEX | TT_BINARY)) == 0 &&
                ((t1.subtype & TT_FLOAT) == 0 || (t2.subtype & TT_FLOAT) == 0)) { t1 += t2; return true; }

            return false;
        }

        bool ExpandBuiltinDefine(Token deftoken, Define define, out Token firsttoken, out Token lasttoken)
        {
            var token = new Token(deftoken);
            switch (define.builtin)
            {
                case BUILTIN_LINE:
                    token = deftoken.line.ToString();
                    token.intvalue = (uint)deftoken.line;
                    token.floatvalue = deftoken.line;
                    token.type = TT.NUMBER;
                    token.subtype = TT_DECIMAL | TT_INTEGER | TT_VALUESVALID;
                    token.line = deftoken.line;
                    token.linesCrossed = deftoken.linesCrossed;
                    token.flags = 0;
                    firsttoken = token;
                    lasttoken = token;
                    break;
                case BUILTIN_FILE:
                    token = scriptstack.FileName;
                    token.type = TT.NAME;
                    token.subtype = token.val.Length;
                    token.line = deftoken.line;
                    token.linesCrossed = deftoken.linesCrossed;
                    token.flags = 0;
                    firsttoken = token;
                    lasttoken = token;
                    break;
                case BUILTIN_DATE:
                    token = $"\"{DateTime.Now:MMM dd yyyy}\"";
                    token.type = TT.STRING;
                    token.subtype = token.val.Length;
                    token.line = deftoken.line;
                    token.linesCrossed = deftoken.linesCrossed;
                    token.flags = 0;
                    firsttoken = token;
                    lasttoken = token;
                    break;
                case BUILTIN_TIME:
                    token = $"\"{DateTime.Now:HH:mm:ss}\"";
                    token.type = TT.STRING;
                    token.subtype = token.val.Length;
                    token.line = deftoken.line;
                    token.linesCrossed = deftoken.linesCrossed;
                    token.flags = 0;
                    firsttoken = token;
                    lasttoken = token;
                    break;
                case BUILTIN_STDC:
                    Warning("__STDC__ not supported\n");
                    firsttoken = null;
                    lasttoken = null;
                    break;
                default:
                    firsttoken = null;
                    lasttoken = null;
                    break;
            }
            return true;
        }

        bool ExpandDefine(Token deftoken, Define define, out Token firsttoken, out Token lasttoken)
        {
            Token[] parms = new Token[MAX_DEFINEPARMS]; Token dt, pt, t, t1, t2, first, last, nextpt, token; int parmnum, i;

            // if it is a builtin define
            if (define.builtin != 0) return ExpandBuiltinDefine(deftoken, define, out firsttoken, out lasttoken);
            // if the define has parameters
            if (define.numparms != 0)
            {
                if (!ReadDefineParms(define, parms, MAX_DEFINEPARMS)) { firsttoken = lasttoken = default; return false; }
#if DEBUG_EVAL
                for (i = 0; i < define.numparms; i++)
                {
                    Console.Write($"define parms {i}:");
                    for (pt = parms[i]; pt != null; pt = pt.next) Console.Write(pt);
                }
#endif
            }
            // empty list at first
            first = last = null;
            // create a list with tokens of the expanded define
            for (dt = define.tokens; dt != null; dt = dt.next)
            {
                parmnum = -1;
                // if the token is a name, it could be a define parameter
                if (dt.type == TT.NAME) parmnum = FindDefineParm(define, dt);
                // if it is a define parameter
                if (parmnum >= 0)
                {
                    for (pt = parms[parmnum]; pt != null; pt = pt.next)
                    {
                        t = new Token(pt) { next = null };
                        // add the token to the list
                        if (last != null) last.next = t;
                        else first = t;
                        last = t;
                    }
                }
                else
                {
                    // if stringizing operator
                    if (dt == "#")
                    {
                        // the stringizing operator must be followed by a define parameter
                        parmnum = dt.next != null ? FindDefineParm(define, dt.next) : -1;

                        if (parmnum >= 0)
                        {
                            // step over the stringizing operator
                            dt = dt.next;
                            // stringize the define parameter tokens
                            if (!StringizeTokens(parms[parmnum], out token)) { Error("can't stringize tokens"); firsttoken = lasttoken = default; return false; }
                            t = new Token(token) { line = deftoken.line };
                        }
                        else { Warning("stringizing operator without define parameter"); continue; }
                    }
                    else t = new Token(dt) { line = deftoken.line };
                    // add the token to the list
                    t.next = null;
                    // the token being read from the define list should use the line number of the original file, not the header file
                    t.line = deftoken.line;

                    if (last != null) last.next = t;
                    else first = t;
                    last = t;
                }
            }
            // check for the merging operator
            for (t = first; t != null;)
            {
                // if the merging operator
                if (t.next != null && t.next == "##")
                {
                    t1 = t;
                    t2 = t.next.next;
                    if (t2 != null)
                    {
                        if (!MergeTokens(t1, t2)) { Error($"can't merge '{t1}' with '{t2}'"); firsttoken = lasttoken = default; return false; }
                        t1.next = t2.next;
                        if (t2 == last) last = t1;
                        continue;
                    }
                }
                t = t.next;
            }
            // store the first and last token of the list
            firsttoken = first;
            lasttoken = last;
            // free all the parameter tokens
            for (i = 0; i < define.numparms; i++) for (pt = parms[i]; pt != null; pt = nextpt) nextpt = pt.next;

            return true;
        }

        bool ExpandDefineIntoSource(Token deftoken, Define define)
        {
            if (!ExpandDefine(deftoken, define, out var firsttoken, out var lasttoken))
                return false;
            // if the define is not empty
            if (firsttoken != null && lasttoken != null)
            {
                firsttoken.linesCrossed += deftoken.linesCrossed;
                lasttoken.next = tokens;
                tokens = firsttoken;
            }
            return true;
        }

        void AddGlobalDefinesToSource()
        {
            Define define, newdefine;

            for (define = globaldefines; define != null; define = define.next)
            {
                newdefine = CopyDefine(define);
                AddDefineToHash(newdefine, defines);
            }
        }

        Define CopyDefine(Define define)
        {
            Token token, newtoken, lasttoken;

            var newdefine = new Define
            {
                // copy the define name
                name = define.name,
                flags = define.flags,
                builtin = define.builtin,
                numparms = define.numparms,
                // the define is not linked
                next = null,
                hashnext = null,
                // copy the define tokens
                tokens = null
            };
            for (lasttoken = null, token = define.tokens; token != null; token = token.next)
            {
                newtoken = new Token(token) { next = null };
                if (lasttoken != null) lasttoken.next = newtoken;
                else newdefine.tokens = newtoken;
                lasttoken = newtoken;
            }
            // copy the define parameters
            newdefine.parms = null;
            for (lasttoken = null, token = define.parms; token != null; token = token.next)
            {
                newtoken = new Token(token) { next = null };
                if (lasttoken != null) lasttoken.next = newtoken;
                else newdefine.parms = newtoken;
                lasttoken = newtoken;
            }
            return newdefine;
        }

        Define FindHashedDefine(Dictionary<string, Define> defines, string name)
            => defines.TryGetValue(name, out var z) ? z : null;

        int FindDefineParm(Define define, string name)
        {
            var i = 0;
            for (var p = define.parms; p != null; p = p.next) { if (p == name) return i; i++; }
            return -1;
        }

        void AddDefineToHash(Define define, Dictionary<string, Define> defines)
            => defines[define.name] = define;

        static void PrintDefine(Define define)
        {
            Printf($"define.name = {define.name}\n");
            Printf($"define.flags = {define.flags}\n");
            Printf($"define.builtin = {define.builtin}\n");
            Printf($"define.numparms = {define.numparms}\n");
        }

        static Define FindDefine(Define defines, string name)
        {
            for (var d = defines; d != null; d = d.next) if (d.name == name) return d;
            return null;
        }

        static Define DefineFromString(string s)
        {
            Parser src = new();

            if (!src.LoadMemory(s, s.Length, "*defineString")) return null;
            // create a define from the source
            if (!src.Directive_define()) { src.FreeSource(); return null; }
            var def = src.CopyFirstDefine();
            src.FreeSource();
            // if the define was created succesfully
            return def;
        }

        Define CopyFirstDefine()
        {
            var define = defines.Values.FirstOrDefault();
            return define != null ? CopyDefine(define) : null;
        }

        bool Directive_include()
        {
            Lexer script; string path;

            if (!ReadSourceToken(out var token)) { Error("#include without file name"); return false; }
            if (token.linesCrossed > 0) { Error("#include without file name"); return false; }
            if (token.type == TT.STRING)
            {
                script = new();
                // try relative to the current file
                path = $"{Path.GetDirectoryName(scriptstack.FileName)}/{token}";
                if (!script.LoadFile(path, osPath))
                {
                    // try absolute path
                    path = token;
                    if (!script.LoadFile(path, osPath))
                    {
                        // try from the include path
                        path = includepath + token;
                        if (!script.LoadFile(path, osPath)) script = null;
                    }
                }
            }
            else if (token.type == TT.PUNCTUATION && token == "<")
            {
                path = includepath;
                while (ReadSourceToken(out token))
                {
                    if (token.linesCrossed > 0) { UnreadSourceToken(token); break; }
                    if (token.type == TT.PUNCTUATION && token == ">") break;
                    path += token;
                }
                if (token != ">") Warning("#include missing trailing >");
                if (path.Length == 0) { Error("#include without file name between < >"); return false; }
                if ((flags & LEXFL.NOBASEINCLUDES) != 0) return true;
                script = new Lexer();
                if (!script.LoadFile(includepath + path, osPath)) script = null;
            }
            else { Error("#include without file name"); return false; }
            if (script == null) { Error($"file '{path}' not found"); return false; }
            script.Flags = flags;
            script.SetPunctuations(punctuations);
            PushScript(script);
            return true;
        }

        bool Directive_undef()
        {
            if (!ReadLine(out var token)) { Error("undef without name"); return false; }
            if (token.type != TT.NAME) { UnreadSourceToken(token); Error($"expected name but found '{token}'"); return false; }
            if (defines.TryGetValue(token, out var define))
            {
                if ((define.flags & DEFINE_FIXED) != 0) Warning($"can't undef '{token}'");
                else defines.Remove(token);
            }
            return true;
        }

        bool Directive_if_def(int type)
        {
            if (!ReadLine(out var token)) { Error("#ifdef without name"); return false; }
            if (token.type != TT.NAME) { UnreadSourceToken(token); Error($"expected name after #ifdef, found '{token}'"); return false; }
            var d = FindHashedDefine(defines, token);
            var skip = (type == INDENT_IFDEF) == (d == null) ? 1 : 0;
            PushIndent(type, skip);
            return true;
        }

        bool Directive_ifdef()
            => Directive_if_def(INDENT_IFDEF);

        bool Directive_ifndef()
            => Directive_if_def(INDENT_IFNDEF);

        bool Directive_else()
        {
            PopIndent(out var type, out var skip);
            if (type == 0) { Error("misplaced #else"); return false; }
            if (type == INDENT_ELSE) { Error("#else after #else"); return false; }
            PushIndent(INDENT_ELSE, skip == 0 ? 1 : 0);
            return true;
        }

        bool Directive_endif()
        {
            PopIndent(out var type, out var skip);
            if (type == 0) { Error("misplaced #endif"); return false; }
            return true;
        }

        public class Operator
        {
            public int op;
            public int priority;
            public int parentheses;
            public Operator prev, next;
        }

        public class Value
        {
            public int intvalue;
            public double floatvalue;
            public int parentheses;
            public Value prev, next;
        }

        int PC_OperatorPriority(int op)
            => op switch
            {
                P_MUL => 15,
                P_DIV => 15,
                P_MOD => 15,
                P_ADD => 14,
                P_SUB => 14,
                P_LOGIC_AND => 7,
                P_LOGIC_OR => 6,
                P_LOGIC_GEQ => 12,
                P_LOGIC_LEQ => 12,
                P_LOGIC_EQ => 11,
                P_LOGIC_UNEQ => 11,
                P_LOGIC_NOT => 16,
                P_LOGIC_GREATER => 12,
                P_LOGIC_LESS => 12,
                P_RSHIFT => 13,
                P_LSHIFT => 13,
                P_BIN_AND => 10,
                P_BIN_OR => 8,
                P_BIN_XOR => 9,
                P_BIN_NOT => 16,
                P_COLON => 5,
                P_QUESTIONMARK => 5,
                _ => 0,
            };

        const int MAX_VALUES = 64;
        const int MAX_OPERATORS = 64;

        bool EvaluateTokens(Token tokens, out int intvalue, out double floatvalue, bool integer)
        {
            Operator o, firstoperator, lastoperator;
            Value v, firstvalue, lastvalue, v1, v2;
            Token t;
            bool brace = false, error = false, lastwasvalue = false, negativevalue = false; int parentheses = 0;
            int questmarkintvalue = 0; double questmarkfloatvalue = 0; bool gotquestmarkvalue = false;
            //
            var operator_heap = new Operator[MAX_OPERATORS]; int numoperators = 0;
            var value_heap = new Value[MAX_VALUES]; int numvalues = 0;

            bool AllocValue(out Value val)
            {
                if (numvalues >= MAX_VALUES) { Error("out of value space\n"); val = default; return false; }
                val = value_heap[numvalues++];
                return true;
            }

            static void FreeValue(Value val) { }

            bool AllocOperator(out Operator op)
            {
                if (numoperators >= MAX_OPERATORS) { Error("out of operator space\n"); op = default; return false; }
                op = operator_heap[numoperators++];
                return true;
            }

            static void FreeOperator(Operator op) { }

            firstoperator = lastoperator = null;
            firstvalue = lastvalue = null;
            intvalue = 0;
            floatvalue = 0;

            for (t = tokens; t != null; t = t.next)
            {
                switch (t.type)
                {
                    case TT.NAME:
                        {
                            if (lastwasvalue || negativevalue) { Error("syntax error in #if/#elif"); error = true; break; }
                            if (t != "defined") { Error($"undefined name '{t}' in #if/#elif"); error = true; break; }
                            t = t.next;
                            if (t == "(") { brace = true; t = t.next; }
                            if (t == null || t.type != TT.NAME) { Error("defined() without name in #if/#elif"); error = true; break; }
                            if (AllocValue(out v)) { error = true; break; }
                            if (defines.ContainsKey(t)) { v.intvalue = 1; v.floatvalue = 1; }
                            else { v.intvalue = 0; v.floatvalue = 0; }
                            v.parentheses = parentheses;
                            v.next = null;
                            v.prev = lastvalue;
                            if (lastvalue != null) lastvalue.next = v;
                            else firstvalue = v;
                            lastvalue = v;
                            if (brace)
                            {
                                t = t.next;
                                if (t == null || t != ")") { Error("defined missing ) in #if/#elif"); error = true; break; }
                            }
                            brace = false;
                            // defined() creates a value
                            lastwasvalue = true;
                            break;
                        }
                    case TT.NUMBER:
                        {
                            if (lastwasvalue) { Error("syntax error in #if/#elif"); error = true; break; }
                            if (AllocValue(out v)) { error = true; break; }
                            if (negativevalue) { v.intvalue = -t.IntValue; v.floatvalue = -t.FloatValue; }
                            else { v.intvalue = t.IntValue; v.floatvalue = t.FloatValue; }
                            v.parentheses = parentheses;
                            v.next = null;
                            v.prev = lastvalue;
                            if (lastvalue != null) lastvalue.next = v;
                            else firstvalue = v;
                            lastvalue = v;
                            // last token was a value
                            lastwasvalue = true;
                            negativevalue = false;
                            break;
                        }
                    case TT.PUNCTUATION:
                        {
                            if (negativevalue) { Error("misplaced minus sign in #if/#elif"); error = true; break; }
                            if (t.subtype == P_PARENTHESESOPEN) { parentheses++; break; }
                            else if (t.subtype == P_PARENTHESESCLOSE)
                            {
                                parentheses--;
                                if (parentheses < 0) { Error("too many ) in #if/#elsif"); error = true; }
                                break;
                            }
                            // check for invalid operators on floating point values
                            if (!integer)
                            {
                                if (t.subtype == P_BIN_NOT || t.subtype == P_MOD ||
                                    t.subtype == P_RSHIFT || t.subtype == P_LSHIFT ||
                                    t.subtype == P_BIN_AND || t.subtype == P_BIN_OR ||
                                    t.subtype == P_BIN_XOR) { Error($"illigal operator '{t}' on floating point operands\n"); error = true; break; }
                            }
                            switch (t.subtype)
                            {
                                case P_LOGIC_NOT:
                                case P_BIN_NOT: if (lastwasvalue) { Error("! or ~ after value in #if/#elif"); error = true; break; } break;
                                case P_INC:
                                case P_DEC: Error("++ or -- used in #if/#elif"); break;
                                case P_SUB: if (!lastwasvalue) { negativevalue = true; break; } goto case P_MUL;

                                case P_MUL:
                                case P_DIV:
                                case P_MOD:
                                case P_ADD:

                                case P_LOGIC_AND:
                                case P_LOGIC_OR:
                                case P_LOGIC_GEQ:
                                case P_LOGIC_LEQ:
                                case P_LOGIC_EQ:
                                case P_LOGIC_UNEQ:

                                case P_LOGIC_GREATER:
                                case P_LOGIC_LESS:

                                case P_RSHIFT:
                                case P_LSHIFT:

                                case P_BIN_AND:
                                case P_BIN_OR:
                                case P_BIN_XOR:

                                case P_COLON:
                                case P_QUESTIONMARK: if (!lastwasvalue) { Error($"operator '{t}' after operator in #if/#elif"); error = true; break; } break;
                                default: Error("invalid operator '{t}' in #if/#elif"); error = true; break;
                            }
                            if (!error && !negativevalue)
                            {
                                if (AllocOperator(out o)) { error = true; break; }
                                o.op = t.subtype;
                                o.priority = PC_OperatorPriority(t.subtype);
                                o.parentheses = parentheses;
                                o.next = null;
                                o.prev = lastoperator;
                                if (lastoperator != null) lastoperator.next = o;
                                else firstoperator = o;
                                lastoperator = o;
                                lastwasvalue = false;
                            }
                            break;
                        }
                    default: Error($"unknown '{t}' in #if/#elif"); error = true; break;
                }
                if (error) break;
            }
            if (!error)
            {
                if (!lastwasvalue) { Error("trailing operator in #if/#elif"); error = true; }
                else if (parentheses != 0) { Error("too many ( in #if/#elif"); error = true; }
            }
            //
            gotquestmarkvalue = false; questmarkintvalue = 0; questmarkfloatvalue = 0;
            // while there are operators
            while (!error && firstoperator != null)
            {
                v = firstvalue;
                for (o = firstoperator; o.next != null; o = o.next)
                {
                    // if the current operator is nested deeper in parentheses than the next operator
                    if (o.parentheses > o.next.parentheses) break;
                    // if the current and next operator are nested equally deep in parentheses
                    if (o.parentheses == o.next.parentheses)
                        // if the priority of the current operator is equal or higher than the priority of the next operator
                        if (o.priority >= o.next.priority) break;
                    // if the arity of the operator isn't equal to 1
                    if (o.op != P_LOGIC_NOT && o.op != P_BIN_NOT) v = v.next;
                    // if there's no value or no next value
                    if (v == null) { Error("mising values in #if/#elif"); error = true; break; }
                }
                if (error) break;
                v1 = v;
                v2 = v.next;
#if DEBUG_EVAL
                if (integer)
                {
                    Console.Write($"operator {scriptstack.GetPunctuationFromId(o.op)}, value1 = {v1.intvalue}");
                    if (v2 != null) Console.Write($"value2 = {v2.intvalue}");
                }
                else
                {
                    Console.Write($"operator {scriptstack.GetPunctuationFromId(o.op)}, value1 = {v1.floatvalue}");
                    if (v2 != null) Console.Write($"value2 = {v2.floatvalue}");
                }
#endif
                switch (o.op)
                {
                    case P_LOGIC_NOT:
                        v1.intvalue = v1.intvalue == 0 ? 1 : 0;
                        v1.floatvalue = v1.floatvalue == 0 ? 1d : 0d; break;
                    case P_BIN_NOT:
                        v1.intvalue = ~v1.intvalue;
                        break;
                    case P_MUL:
                        v1.intvalue *= v2.intvalue;
                        v1.floatvalue *= v2.floatvalue; break;
                    case P_DIV:
                        if (v2.intvalue == 0 || v2.floatvalue == 0) { Error("divide by zero in #if/#elif\n"); error = true; break; }
                        v1.intvalue /= v2.intvalue;
                        v1.floatvalue /= v2.floatvalue; break;
                    case P_MOD:
                        if (v2.intvalue == 0) { Error("divide by zero in #if/#elif\n"); error = true; break; }
                        v1.intvalue %= v2.intvalue; break;
                    case P_ADD:
                        v1.intvalue += v2.intvalue;
                        v1.floatvalue += v2.floatvalue; break;
                    case P_SUB:
                        v1.intvalue -= v2.intvalue;
                        v1.floatvalue -= v2.floatvalue; break;
                    case P_LOGIC_AND:
                        v1.intvalue = v1.intvalue != 0 && v2.intvalue != 0 ? 1 : 0;
                        v1.floatvalue = v1.floatvalue != 0d && v2.floatvalue != 0d ? 1d : 0d; break;
                    case P_LOGIC_OR:
                        v1.intvalue = v1.intvalue != 0 || v2.intvalue != 0 ? 1 : 0;
                        v1.floatvalue = v1.floatvalue != 0d || v2.floatvalue != 0d ? 1d : 0d; break;
                    case P_LOGIC_GEQ:
                        v1.intvalue = v1.intvalue >= v2.intvalue ? 1 : 0;
                        v1.floatvalue = v1.floatvalue >= v2.floatvalue ? 1d : 0d; break;
                    case P_LOGIC_LEQ:
                        v1.intvalue = v1.intvalue <= v2.intvalue ? 1 : 0;
                        v1.floatvalue = v1.floatvalue <= v2.floatvalue ? 1d : 0d; break;
                    case P_LOGIC_EQ:
                        v1.intvalue = v1.intvalue == v2.intvalue ? 1 : 0;
                        v1.floatvalue = v1.floatvalue == v2.floatvalue ? 1d : 0d; break;
                    case P_LOGIC_UNEQ:
                        v1.intvalue = v1.intvalue != v2.intvalue ? 1 : 0;
                        v1.floatvalue = v1.floatvalue != v2.floatvalue ? 1d : 0d; break;
                    case P_LOGIC_GREATER:
                        v1.intvalue = v1.intvalue > v2.intvalue ? 1 : 0;
                        v1.floatvalue = v1.floatvalue > v2.floatvalue ? 1d : 0d; break;
                    case P_LOGIC_LESS:
                        v1.intvalue = v1.intvalue < v2.intvalue ? 1 : 0;
                        v1.floatvalue = v1.floatvalue < v2.floatvalue ? 1d : 0d; break;
                    case P_RSHIFT:
                        v1.intvalue >>= v2.intvalue;
                        break;
                    case P_LSHIFT:
                        v1.intvalue <<= v2.intvalue;
                        break;
                    case P_BIN_AND:
                        v1.intvalue &= v2.intvalue;
                        break;
                    case P_BIN_OR:
                        v1.intvalue |= v2.intvalue;
                        break;
                    case P_BIN_XOR:
                        v1.intvalue ^= v2.intvalue;
                        break;
                    case P_COLON:
                        if (!gotquestmarkvalue) { Error(": without ? in #if/#elif"); error = true; break; }
                        if (integer)
                        {
                            if (questmarkintvalue == 0)
                                v1.intvalue = v2.intvalue;
                        }
                        else
                        {
                            if (questmarkfloatvalue == 0d)
                                v1.floatvalue = v2.floatvalue;
                        }
                        gotquestmarkvalue = false;
                        break;
                    case P_QUESTIONMARK:
                        if (gotquestmarkvalue) { Error("? after ? in #if/#elif"); error = true; break; }
                        questmarkintvalue = v1.intvalue;
                        questmarkfloatvalue = v1.floatvalue;
                        gotquestmarkvalue = true;
                        break;
                }
#if DEBUG_EVAL
                if (integer) Console.Write($"result value = {v1.intvalue}");
                else Console.Write($"result value = {v1.floatvalue}");
#endif
                if (error) break;
                // if not an operator with arity 1
                if (o.op != P_LOGIC_NOT && o.op != P_BIN_NOT)
                {
                    // remove the second value if not question mark operator
                    if (o.op != P_QUESTIONMARK) v = v.next;
                    //
                    if (v.prev != null) v.prev.next = v.next;
                    else firstvalue = v.next;
                    if (v.next != null) v.next.prev = v.prev;
                    else lastvalue = v.prev;
                    FreeValue(v);
                }
                // remove the operator
                if (o.prev != null) o.prev.next = o.next;
                else firstoperator = o.next;
                if (o.next != null) o.next.prev = o.prev;
                else lastoperator = o.prev;
                FreeOperator(o);
            }
            if (firstvalue != null) { intvalue = firstvalue.intvalue; floatvalue = firstvalue.floatvalue; }
            for (o = firstoperator; o != null; o = lastoperator) { lastoperator = o.next; FreeOperator(o); }
            for (v = firstvalue; v != null; v = lastvalue) { lastvalue = v.next; FreeValue(v); }
            if (!error) return true;
            intvalue = 0;
            floatvalue = 0;
            return false;
        }

        bool Evaluate(out int intvalue, out double floatvalue, bool integer)
        {
            Token firsttoken, lasttoken, t, nexttoken; Define define; bool defined = false;

            intvalue = 0;
            floatvalue = 0;
            //
            if (!ReadLine(out var token)) { Error("no value after #if/#elif"); return false; }
            firsttoken = null;
            lasttoken = null;
            do
            {
                // if the token is a name
                if (token.type == TT.NAME)
                {
                    if (defined)
                    {
                        defined = false;
                        t = new Token(token) { next = null };
                        if (lasttoken != null) lasttoken.next = t;
                        else firsttoken = t;
                        lasttoken = t;
                    }
                    else if (token == "defined")
                    {
                        defined = true;
                        t = new Token(token) { next = null };
                        if (lasttoken != null) lasttoken.next = t;
                        else firsttoken = t;
                        lasttoken = t;
                    }
                    else
                    {
                        // then it must be a define
                        define = FindHashedDefine(defines, token);
                        if (define == null) { Error($"can't Evaluate '{token}', not defined"); return false; }
                        if (!ExpandDefineIntoSource(token, define)) return false;
                    }
                }
                // if the token is a number or a punctuation
                else if (token.type == TT.NUMBER || token.type == TT.PUNCTUATION)
                {
                    t = new Token(token) { next = null };
                    if (lasttoken != null) lasttoken.next = t;
                    else firsttoken = t;
                    lasttoken = t;
                }
                else { Error($"can't Evaluate '{token}'"); return false; }
            } while (ReadLine(out token));
            //
            if (!EvaluateTokens(firsttoken, out intvalue, out floatvalue, integer)) return false;

#if DEBUG_EVAL
            Console.Write("eval:");
#endif
            for (t = firsttoken; t != null; t = nexttoken)
            {
#if DEBUG_EVAL
                Console.Write($" {t}");
#endif
                nexttoken = t.next;
            }
#if DEBUG_EVAL
            if (integer) Console.Write($"eval result: {intvalue}");
            else Console.Write($"eval result: {floatvalue}");
#endif
            return true;
        }

        bool DollarEvaluate(out int intvalue, out double floatvalue, bool integer)
        {
            int indent; Token firsttoken, lasttoken, t, nexttoken; Define define; bool defined = false;

            intvalue = 0; floatvalue = 0d;

            if (!ReadSourceToken(out var token)) { Error("no leading ( after $evalint/$evalfloat"); return false; }
            if (!ReadSourceToken(out token)) { Error("nothing to Evaluate"); return false; }
            indent = 1;
            firsttoken = null;
            lasttoken = null;
            do
            {
                //if the token is a name
                if (token.type == TT.NAME)
                {
                    if (defined)
                    {
                        defined = false;
                        t = new Token(token) { next = null };
                        if (lasttoken != null) lasttoken.next = t;
                        else firsttoken = t;
                        lasttoken = t;
                    }
                    else if (token == "defined")
                    {
                        defined = true;
                        t = new Token(token) { next = null };
                        if (lasttoken != null) lasttoken.next = t;
                        else firsttoken = t;
                        lasttoken = t;
                    }
                    else
                    {
                        //then it must be a define
                        define = FindHashedDefine(defines, token);
                        if (define == null) { Warning($"can't Evaluate '{token}', not defined"); return false; }
                        if (!ExpandDefineIntoSource(token, define)) return false;
                    }
                }
                //if the token is a number or a punctuation
                else if (token.type == TT.NUMBER || token.type == TT.PUNCTUATION)
                {
                    if (token[0] == '(') indent++;
                    else if (token[0] == ')') indent--;
                    if (indent <= 0) break;
                    t = new Token(token) { next = null };
                    if (lasttoken != null) lasttoken.next = t;
                    else firsttoken = t;
                    lasttoken = t;
                }
                else { Error($"can't Evaluate '{token}'"); return false; }
            } while (ReadSourceToken(out token));
            //
            if (!EvaluateTokens(firsttoken, out intvalue, out floatvalue, integer)) return false;

#if DEBUG_EVAL
            Console.Write("$eval:");
#endif
            for (t = firsttoken; t != null; t = nexttoken)
            {
#if DEBUG_EVAL
                Console.Write(t);
#endif
                nexttoken = t.next;
            }
#if DEBUG_EVAL
            if (integer) Console.Write($"$eval result: {intvalue}");
            else Console.Write($"$eval result: {floatvalue}");
#endif
            return true;
        }

        bool Directive_define()
        {
            Token last, t;

            if (!ReadLine(out var token)) { Error("#define without name"); return false; }
            if (token.type != TT.NAME) { UnreadSourceToken(token); Error($"expected name after #define, found '{token}'"); return false; }
            // check if the define already exists
            var define = FindHashedDefine(defines, token);
            if (define != null)
            {
                if ((define.flags & DEFINE_FIXED) != 0) { Error($"can't redefine '{token}'"); return false; }
                Warning($"redefinition of '{token}'");
                // unread the define name before executing the #undef directive
                UnreadSourceToken(token);
                if (!Directive_undef()) return false;
                // if the define was not removed (define.flags & DEFINE_FIXED)
                define = FindHashedDefine(defines, token);
            }
            // allocate define
            define = new Define { name = token };
            // add the define to the source
            AddDefineToHash(define, defines);
            // if nothing is defined, just return
            if (!ReadLine(out token)) return true;
            // if it is a define with parameters
            if (!token.WhiteSpaceBeforeToken && token == "(")
            {
                // read the define parameters
                last = null;
                if (!CheckTokenString(")"))
                    while (true)
                    {
                        if (!ReadLine(out token)) { Error("expected define parameter"); return false; }
                        // if it isn't a name
                        if (token.type != TT.NAME) { Error("invalid define parameter"); return false; }

                        if (FindDefineParm(define, token) >= 0) { Error("two the same define parameters"); return false; }
                        // add the define parm
                        t = new Token(token);
                        t.ClearTokenWhiteSpace();
                        t.next = null;
                        if (last != null) last.next = t;
                        else define.parms = t;
                        last = t;
                        define.numparms++;
                        // read next token
                        if (!ReadLine(out token)) { Error("define parameters not terminated"); return false; }

                        if (token == ")") break;
                        // then it must be a comma
                        if (token != ",") { Error("define not terminated"); return false; }
                    }
                if (!ReadLine(out token)) return true;
            }
            // read the defined stuff
            last = null;
            do
            {
                t = new Token(token);
                if (t.type == TT.NAME && t == define.name) { t.flags |= TOKEN_FL_RECURSIVE_DEFINE; Warning("recursive define (removed recursion)"); }
                t.ClearTokenWhiteSpace();
                t.next = null;
                if (last != null) last.next = t;
                else define.tokens = t;
                last = t;
            } while (ReadLine(out token));

            if (last != null)
                // check for merge operators at the beginning or end
                if (define.tokens == "##" || last == "##") { Error("define with misplaced ##"); return false; }
            return true;
        }

        bool Directive_elif()
        {
            PopIndent(out var type, out var skip);
            if (type == 0 || type == INDENT_ELSE) { Error("misplaced #elif"); return false; }
            if (!Evaluate(out var value, out var _, true)) return false;
            skip = value == 0 ? 1 : 0;
            PushIndent(INDENT_ELIF, skip);
            return true;
        }

        bool Directive_if()
        {
            if (!Evaluate(out var value, out var _, true)) return false;
            var skip = value == 0 ? 1 : 0;
            PushIndent(INDENT_IF, skip);
            return true;
        }

        bool Directive_line()
        {
            Error("#line directive not supported");
            while (ReadLine(out var _)) { }
            return true;
        }

        bool Directive_error()
        {
            if (!ReadLine(out var token) || token.type != TT.STRING) { Error("#error without string"); return false; }
            Error($"#error: {token}");
            return true;
        }

        bool Directive_warning()
        {
            if (!ReadLine(out var token) || token.type != TT.STRING) { Warning("#warning without string"); return false; }
            Warning($"#warning: {token}");
            return true;
        }

        bool Directive_pragma()
        {
            Warning("#pragma directive not supported");
            while (ReadLine(out var _)) { }
            return true;
        }

        void UnreadSignToken()
        {
            var token = new Token
            {
                line = scriptstack.LineNum,
                whiteSpaceStart_p = 0,
                whiteSpaceEnd_p = 0,
                linesCrossed = 0,
                flags = 0,
                val = "-",
                type = TT.PUNCTUATION,
                subtype = P_SUB
            };
            UnreadSourceToken(token);
        }

        bool Directive_eval()
        {
            if (!Evaluate(out var value, out var _, true)) return false;

            Token token = new();
            token.line = scriptstack.LineNum;
            token.whiteSpaceStart_p = 0;
            token.whiteSpaceEnd_p = 0;
            token.linesCrossed = 0;
            token.flags = 0;
            token = Math.Abs(value).ToString();
            token.type = TT.NUMBER;
            token.subtype = TT_INTEGER | TT_LONG | TT_DECIMAL;
            UnreadSourceToken(token);
            if (value < 0) UnreadSignToken();
            return true;
        }

        bool Directive_evalfloat()
        {
            if (!Evaluate(out var _, out var value, false)) return false;

            Token token = new();
            token.line = scriptstack.LineNum;
            token.whiteSpaceStart_p = 0;
            token.whiteSpaceEnd_p = 0;
            token.linesCrossed = 0;
            token.flags = 0;
            token = $"{MathX.Fabs(value):1.2}";
            token.type = TT.NUMBER;
            token.subtype = TT_FLOAT | TT_LONG | TT_DECIMAL;
            UnreadSourceToken(token);
            if (value < 0) UnreadSignToken();
            return true;
        }

        bool ReadDirective()
        {
            //read the directive name
            if (!ReadSourceToken(out var token)) { Error("found '#' without name"); return false; }
            // directive name must be on the same line
            if (token.linesCrossed > 0) { UnreadSourceToken(token); Error("found '#' at end of line"); return false; }
            // if if is a name
            if (token.type == TT.NAME)
            {
                if (token == "if") return Directive_if();
                else if (token == "ifdef") return Directive_ifdef();
                else if (token == "ifndef") return Directive_ifndef();
                else if (token == "elif") return Directive_elif();
                else if (token == "else") return Directive_else();
                else if (token == "endif") return Directive_endif();
                else if (skip > 0)
                {
                    // skip the rest of the line
                    while (ReadLine(out token)) { }
                    return true;
                }
                else
                {
                    if (token == "include") return Directive_include();
                    else if (token == "define") return Directive_define();
                    else if (token == "undef") return Directive_undef();
                    else if (token == "line") return Directive_line();
                    else if (token == "error") return Directive_error();
                    else if (token == "warning") return Directive_warning();
                    else if (token == "pragma") return Directive_pragma();
                    else if (token == "eval") return Directive_eval();
                    else if (token == "evalfloat") return Directive_evalfloat();
                }
            }
            Error($"unknown precompiler directive '{token}'");
            return false;
        }

        bool DollarDirective_evalint()
        {
            if (!DollarEvaluate(out var value, out var _, true)) return false;

            Token token = new();
            token.line = scriptstack.LineNum;
            token.whiteSpaceStart_p = 0;
            token.whiteSpaceEnd_p = 0;
            token.linesCrossed = 0;
            token.flags = 0;
            token = Math.Abs(value).ToString();
            token.type = TT.NUMBER;
            token.subtype = TT_INTEGER | TT_LONG | TT_DECIMAL | TT_VALUESVALID;
            token.intvalue = (uint)Math.Abs(value);
            token.floatvalue = Math.Abs(value);
            UnreadSourceToken(token);
            if (value < 0) UnreadSignToken();
            return true;
        }

        bool DollarDirective_evalfloat()
        {
            if (!DollarEvaluate(out var _, out var value, false)) return false;

            Token token = new();
            token.line = scriptstack.LineNum;
            token.whiteSpaceStart_p = 0;
            token.whiteSpaceEnd_p = 0;
            token.linesCrossed = 0;
            token.flags = 0;
            token = $"{MathX.Fabs(value):1.2}";
            token.type = TT.NUMBER;
            token.subtype = TT_FLOAT | TT_LONG | TT_DECIMAL | TT_VALUESVALID;
            token.intvalue = (uint)MathX.Fabs(value);
            token.floatvalue = MathX.Fabs(value);
            UnreadSourceToken(token);
            if (value < 0) UnreadSignToken();
            return true;
        }

        bool ReadDollarDirective()
        {
            // read the directive name
            if (!ReadSourceToken(out var token)) { Error("found '$' without name"); return false; }
            // directive name must be on the same line
            if (token.linesCrossed > 0) { UnreadSourceToken(token); Error("found '$' at end of line"); return false; }
            // if if is a name
            if (token.type == TT.NAME)
            {
                if (token == "evalint") return DollarDirective_evalint();
                else if (token == "evalfloat") return DollarDirective_evalfloat();
            }
            UnreadSourceToken(token);
            return false;
        }
    }
}