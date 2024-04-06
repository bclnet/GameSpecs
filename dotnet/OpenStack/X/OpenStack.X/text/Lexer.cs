using System.Text;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.Platform;

namespace System.NumericsX.OpenStack
{
    // lexer flags
    public enum LEXFL
    {
        NOERRORS = 1 << 0,  // don't print any errors
        NOWARNINGS = 1 << 1,    // don't print any warnings
        NOFATALERRORS = 1 << 2, // errors aren't fatal
        NOSTRINGCONCAT = 1 << 3,    // multiple strings seperated by whitespaces are not concatenated
        NOSTRINGESCAPECHARS = 1 << 4,   // no escape characters inside strings
        NODOLLARPRECOMPILE = 1 << 5,    // don't use the $ sign for precompilation
        NOBASEINCLUDES = 1 << 6,    // don't include files embraced with < >
        ALLOWPATHNAMES = 1 << 7,    // allow path seperators in names
        ALLOWNUMBERNAMES = 1 << 8,  // allow names to start with a number
        ALLOWIPADDRESSES = 1 << 9,  // allow ip addresses to be parsed as numbers
        ALLOWFLOATEXCEPTIONS = 1 << 10, // allow float exceptions like 1.#INF or 1.#IND to be parsed
        ALLOWMULTICHARLITERALS = 1 << 11,   // allow multi character literals
        ALLOWBACKSLASHSTRINGCONCAT = 1 << 12,   // allow multiple strings seperated by '\' to be concatenated
        ONLYSTRINGS = 1 << 13   // parse as whitespace deliminated strings (quoted strings keep quotes)
    }

    public class Lexer
    {
        // longer punctuations first
        static readonly (string p, int n)[] default_punctuations = new (string, int)[]{
	        // binary operators
	        (">>=",P_RSHIFT_ASSIGN),
            ("<<=",P_LSHIFT_ASSIGN),
	        //
	        ("...",P_PARMS),
	        // define merge operator
	        ("##",P_PRECOMPMERGE),				// pre-compiler
	        // logic operators
	        ("&&",P_LOGIC_AND),					// pre-compiler
	        ("||",P_LOGIC_OR),					// pre-compiler
	        (">=",P_LOGIC_GEQ),					// pre-compiler
	        ("<=",P_LOGIC_LEQ),					// pre-compiler
	        ("==",P_LOGIC_EQ),					// pre-compiler
	        ("!=",P_LOGIC_UNEQ),				// pre-compiler
	        // arithmatic operators
	        ("*=",P_MUL_ASSIGN),
            ("/=",P_DIV_ASSIGN),
            ("%=",P_MOD_ASSIGN),
            ("+=",P_ADD_ASSIGN),
            ("-=",P_SUB_ASSIGN),
            ("++",P_INC),
            ("--",P_DEC),
	        // binary operators
	        ("&=",P_BIN_AND_ASSIGN),
            ("|=",P_BIN_OR_ASSIGN),
            ("^=",P_BIN_XOR_ASSIGN),
            (">>",P_RSHIFT),					// pre-compiler
	        ("<<",P_LSHIFT),					// pre-compiler
	        // reference operators
	        (".",P_POINTERREF),
	        // C++
	        ("::",P_CPP1),
            (".*",P_CPP2),
	        // arithmatic operators
	        ("*",P_MUL),						// pre-compiler
	        ("/",P_DIV),						// pre-compiler
	        ("%",P_MOD),						// pre-compiler
	        ("+",P_ADD),						// pre-compiler
	        ("-",P_SUB),						// pre-compiler
	        ("=",P_ASSIGN),
	        // binary operators
	        ("&",P_BIN_AND),					// pre-compiler
	        ("|",P_BIN_OR),						// pre-compiler
	        ("^",P_BIN_XOR),					// pre-compiler
	        ("~",P_BIN_NOT),					// pre-compiler
	        // logic operators
	        ("!",P_LOGIC_NOT),					// pre-compiler
	        (">",P_LOGIC_GREATER),				// pre-compiler
	        ("<",P_LOGIC_LESS),					// pre-compiler
	        // reference operator
	        (".",P_REF),
	        // seperators
	        (",",P_COMMA),						// pre-compiler
	        (";",P_SEMICOLON),
	        // label indication
	        (":",P_COLON),						// pre-compiler
	        // if statement
	        ("?",P_QUESTIONMARK),				// pre-compiler
	        // embracements
	        ("(",P_PARENTHESESOPEN),			// pre-compiler
	        (")",P_PARENTHESESCLOSE),			// pre-compiler
	        ("{",P_BRACEOPEN),					// pre-compiler
	        ("}",P_BRACECLOSE),					// pre-compiler
	        ("[",P_SQBRACKETOPEN),
            ("]",P_SQBRACKETCLOSE),
	        //
	        ("\\",P_BACKSLASH),
	        // precompiler operator
	        ("#",P_PRECOMP),					// pre-compiler
	        ("$",P_DOLLAR),
        };

        // punctuation ids
        const int P_RSHIFT_ASSIGN = 1;
        const int P_LSHIFT_ASSIGN = 2;
        const int P_PARMS = 3;
        const int P_PRECOMPMERGE = 4;

        internal const int P_LOGIC_AND = 5;
        internal const int P_LOGIC_OR = 6;
        internal const int P_LOGIC_GEQ = 7;
        internal const int P_LOGIC_LEQ = 8;
        internal const int P_LOGIC_EQ = 9;
        internal const int P_LOGIC_UNEQ = 10;

        const int P_MUL_ASSIGN = 11;
        const int P_DIV_ASSIGN = 12;
        const int P_MOD_ASSIGN = 13;
        const int P_ADD_ASSIGN = 14;
        const int P_SUB_ASSIGN = 15;
        internal const int P_INC = 16;
        internal const int P_DEC = 17;

        const int P_BIN_AND_ASSIGN = 18;
        const int P_BIN_OR_ASSIGN = 19;
        const int P_BIN_XOR_ASSIGN = 20;
        internal const int P_RSHIFT = 21;
        internal const int P_LSHIFT = 22;

        const int P_POINTERREF = 23;
        const int P_CPP1 = 24;
        const int P_CPP2 = 25;
        internal const int P_MUL = 26;
        internal const int P_DIV = 27;
        internal const int P_MOD = 28;
        internal const int P_ADD = 29;
        internal const int P_SUB = 30;
        const int P_ASSIGN = 31;

        internal const int P_BIN_AND = 32;
        internal const int P_BIN_OR = 33;
        internal const int P_BIN_XOR = 34;
        internal const int P_BIN_NOT = 35;

        internal const int P_LOGIC_NOT = 36;
        internal const int P_LOGIC_GREATER = 37;
        internal const int P_LOGIC_LESS = 38;

        const int P_REF = 39;
        const int P_COMMA = 40;
        const int P_SEMICOLON = 41;
        internal const int P_COLON = 42;
        internal const int P_QUESTIONMARK = 43;

        internal const int P_PARENTHESESOPEN = 44;
        internal const int P_PARENTHESESCLOSE = 45;
        const int P_BRACEOPEN = 46;
        const int P_BRACECLOSE = 47;
        const int P_SQBRACKETOPEN = 48;
        const int P_SQBRACKETCLOSE = 49;
        const int P_BACKSLASH = 50;

        const int P_PRECOMP = 51;
        const int P_DOLLAR = 52;

        bool loaded;                 // set when a script file is loaded from file or memory
        string filename;            // file name of the script
        bool allocated;              // true if buffer memory was allocated
        internal string buffer;              // buffer containing the script
        internal int script_p;               // current pointer in the script
        int end_p;                  // pointer to the end of the script
        int lastScript_p;           // script pointer before reading token
        int whiteSpaceStart_p;      // start of last white space
        int whiteSpaceEnd_p;        // end of last white space
        DateTime fileTime;          // file time
        int length;                 // length of the script in bytes
        int line;                   // current line in script
        int lastline;               // line before reading token
        bool tokenavailable;         // set by unreadToken
        LEXFL flags;                  // several script flags
        (string p, int n)[] punctuations; // the punctuations used in the script
        int[] punctuationtable;     // ASCII table with punctuations
        int[] nextpunctuation;      // next punctuation in chain
        Token token;                // available token
        internal Lexer next;                 // next script in a chain
        bool hadError;              // set by Error, even if the error is supressed

        static int[] default_punctuationtable = new int[256];
        static int[] default_nextpunctuation = new int[default_punctuations.Length];
        static bool default_setup;
        static string baseFolder;        // base folder to load files from

        // constructor
        public Lexer()
        {
            loaded = false;
            filename = string.Empty;
            flags = 0;
            SetPunctuations(null);
            allocated = false;
            fileTime = DateTime.MinValue;
            length = 0;
            line = 0;
            lastline = 0;
            tokenavailable = false;
            token = string.Empty;
            next = null;
            hadError = false;
        }
        public Lexer(LEXFL flags)
        {
            loaded = false;
            filename = string.Empty;
            this.flags = flags;
            SetPunctuations(null);
            allocated = false;
            fileTime = DateTime.MinValue;
            length = 0;
            line = 0;
            lastline = 0;
            tokenavailable = false;
            token = new Token(string.Empty);
            next = null;
            hadError = false;
        }
        public Lexer(string filename, LEXFL flags = 0, bool osPath = false)
        {
            loaded = false;
            this.flags = flags;
            SetPunctuations(null);
            allocated = false;
            token = new Token(string.Empty);
            next = null;
            hadError = false;
            LoadFile(filename, osPath);
        }
        public Lexer(string ptr, int length, string name, LEXFL flags = 0)
        {
            loaded = false;
            this.flags = flags;
            SetPunctuations(null);
            allocated = false;
            token = new Token(string.Empty);
            next = null;
            hadError = false;
            LoadMemory(ptr, name);
        }

        // load a script from the given file at the given offset with the given length
        public bool LoadFile(string filename, bool osPath = false)
        {
            if (loaded) { Error("Lexer::LoadFile: another script already loaded"); return false; }

            var pathname = !osPath && (baseFolder[0] != '\0') ? $"{baseFolder}/{filename}" : filename;
            var fp = osPath
                ? fileSystem.OpenExplicitFileRead(pathname)
                : fileSystem.OpenFileRead(pathname);
            if (fp == null) return false;

            var length = fp.Length;
            var buf = new byte[length + 1];
            buf[length] = 0;
            fp.Read(buf, length);
            fileTime = fp.Timestamp;
            this.filename = fp.FullPath;
            fileSystem.CloseFile(fp);

            buffer = Encoding.ASCII.GetString(buf);
            this.length = length;
            // pointer in script buffer
            script_p = 0;
            // pointer in script buffer before reading token
            lastScript_p = 0;
            // pointer to end of script buffer
            end_p = length;

            tokenavailable = false;
            line = 1;
            lastline = 1;
            allocated = true;
            loaded = true;

            return true;
        }
        // load a script from the given memory with the given length and a specified line offset,
        // so source strings extracted from a file can still refer to proper line numbers in the file
        // NOTE: the ptr is expected to point at a valid C string: ptr[length] == '\0'
        public bool LoadMemory(string ptr, string name, int startLine = 1)
        {
            if (loaded) { Error("Lexer::LoadMemory: another script already loaded"); return false; }

            filename = name;
            buffer = ptr;
            fileTime = DateTime.MinValue;
            this.length = ptr.Length;
            // pointer in script buffer
            script_p = 0;
            // pointer in script buffer before reading token
            lastScript_p = 0;
            // pointer to end of script buffer
            end_p = ptr.Length;

            tokenavailable = false;
            line = startLine;
            lastline = startLine;
            allocated = false;
            loaded = true;

            return true;
        }
        // free the script
        public void FreeSource()
        {
            if (allocated) { buffer = null; allocated = false; }
            tokenavailable = false;
            token = string.Empty;
            loaded = false;
        }

        // returns true if a script is loaded
        public bool IsLoaded => loaded;

        /// <summary>
        /// read a token
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public bool ReadToken(out Token token)
        {
            token = new Token(string.Empty);
            if (!loaded) { Error("Lexer::ReadToken: no file loaded"); return false; }

            // if there is a token available (from unreadToken)
            if (tokenavailable) { tokenavailable = false; token = this.token; return true; }
            // save script pointer
            lastScript_p = script_p;
            // save line counter
            lastline = line;
            // clear the token stuff
            token.val = string.Empty;
            // start of the white space
            whiteSpaceStart_p = script_p;
            token.whiteSpaceStart_p = script_p;
            // read white space before token
            if (!ReadWhiteSpace()) return false;
            // end of the white space
            this.whiteSpaceEnd_p = script_p;
            token.whiteSpaceEnd_p = script_p;
            // line the token is on
            token.line = line;
            // number of lines crossed before token
            token.linesCrossed = line - lastline;
            // clear token flags
            token.flags = 0;

            var c = buffer[script_p];

            // if we're keeping everything as whitespace deliminated strings
            if ((flags & LEXFL.ONLYSTRINGS) != 0)
            {
                // if there is a leading quote
                if (c == '\"' || c == '\'') { if (!ReadString(token, c)) return false; }
                else if (!ReadName(token)) return false;
            }
            // if there is a number
            else if ((c >= '0' && c <= '9') || (c == '.' && buffer[script_p + 1] >= '0' && buffer[script_p + 1] <= '9'))
            {
                if (!ReadNumber(token)) return false;
                // if names are allowed to start with a number
                if ((flags & LEXFL.ALLOWNUMBERNAMES) != 0)
                {
                    c = buffer[script_p];
                    if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_')
                        if (!ReadName(token)) return false;
                }
            }
            // if there is a leading quote
            else if (c == '\"' || c == '\'') { if (!ReadString(token, c)) return false; }
            // if there is a name
            else if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_') { if (!ReadName(token)) return false; }
            // names may also start with a slash when pathnames are allowed
            else if ((flags & LEXFL.ALLOWPATHNAMES) != 0 && (c == '/' || c == '\\' || c == '.')) { if (!ReadName(token)) return false; }
            // check for punctuations
            else if (!ReadPunctuation(token)) { Error($"unknown punctuation {c}"); return false; }
            // succesfully read a token
            return true;
        }

        /// <summary>
        /// expect a certain token, reads the token when available
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        public bool ExpectTokenString(string s)
        {
            if (!ReadToken(out var token)) { Error($"couldn't find expected '{s}'"); return false; }
            if (token != s) { Error($"expected '{s}' but found '{token}'"); return false; }
            return true;
        }

        /// <summary>
        /// expect a certain token type
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="subtype">The subtype.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public bool ExpectTokenType(TT type, int subtype, out Token token)
        {
            if (!ReadToken(out token)) { Error("couldn't read expected token"); return false; }

            string str;
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
                    if ((subtype & Token.TT_DECIMAL) != 0) str = "decimal ";
                    if ((subtype & Token.TT_HEX) != 0) str = "hex ";
                    if ((subtype & Token.TT_OCTAL) != 0) str = "octal ";
                    if ((subtype & Token.TT_BINARY) != 0) str = "binary ";
                    if ((subtype & Token.TT_UNSIGNED) != 0) str += "unsigned ";
                    if ((subtype & Token.TT_LONG) != 0) str += "long ";
                    if ((subtype & Token.TT_FLOAT) != 0) str += "float ";
                    if ((subtype & Token.TT_INTEGER) != 0) str += "integer ";
                    str = str.TrimEnd(' ');
                    Error($"expected {str} but found '{token}'");
                    return false;
                }
            }
            else if (token.type == TT.PUNCTUATION)
            {
                if (subtype < 0) { Error("BUG: wrong punctuation subtype"); return false; }
                if (token.subtype != subtype) { Error($"expected '{GetPunctuationFromId(subtype)}' but found '{token}'"); return false; }
            }
            return true;
        }

        /// <summary>
        /// expect a token
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public bool ExpectAnyToken(out Token token)
        {
            if (!ReadToken(out token)) { Error("couldn't read expected token"); return false; }
            return true;
        }

        /// <summary>
        /// returns true when the token is available
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        public bool CheckTokenString(string s)
        {
            if (!ReadToken(out var tok)) return false;
            // if the given string is available
            if (tok == s) return true;
            // unread token
            script_p = lastScript_p;
            line = lastline;
            return false;
        }

        /// <summary>
        /// returns true an reads the token when a token with the given type is available
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="subtype">The subtype.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public bool CheckTokenType(TT type, int subtype, out Token token)
        {
            if (!ReadToken(out token)) return false;
            // if the type matches
            if (token.type == type && (token.subtype & subtype) == subtype) return true;
            // unread token
            script_p = lastScript_p;
            line = lastline;
            return false;
        }

        /// <summary>
        /// returns true if the next token equals the given string but does not remove the token from the source
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        public bool PeekTokenString(string s)
        {
            if (!ReadToken(out var tok)) return false;

            // unread token
            script_p = lastScript_p;
            line = lastline;

            // if the given string is available
            return tok == s;
        }

        /// <summary>
        /// returns true if the next token equals the given type but does not remove the token from the source
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="subtype">The subtype.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public bool PeekTokenType(TT type, int subtype, ref Token token)
        {
            if (!ReadToken(out var tok)) return false;

            // unread token
            script_p = lastScript_p;
            line = lastline;

            // if the type matches
            if (tok.type == type && (tok.subtype & subtype) == subtype) { token = tok; return true; }
            return false;
        }

        /// <summary>
        /// skip tokens until the given token string is read
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        public bool SkipUntilString(string s)
        {
            while (ReadToken(out var token)) if (token == s) return true;
            return false;
        }

        /// <summary>
        /// skip the rest of the current line
        /// </summary>
        /// <returns></returns>
        public bool SkipRestOfLine()
        {
            while (ReadToken(out var token))
                if (token.linesCrossed != 0)
                {
                    script_p = lastScript_p;
                    line = lastline;
                    return true;
                }
            return false;
        }

        /// <summary>
        /// skip the braced section
        /// Skips until a matching close brace is found.
        /// Internal brace depths are properly skipped.
        /// </summary>
        /// <param name="parseFirstBrace">if set to <c>true</c> [parse first brace].</param>
        /// <returns></returns>
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

        // 
        /// <summary>
        /// unread the given token
        /// </summary>
        /// <param name="token">The token.</param>
        public void UnreadToken(Token token)
        {
            if (tokenavailable) FatalError("Lexer::unreadToken, unread token twice\n");
            this.token = token;
            tokenavailable = true;
        }

        /// <summary>
        /// read a token only if on the same line
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public bool ReadTokenOnLine(out Token token)
        {
            if (!ReadToken(out token)) { script_p = lastScript_p; line = lastline; return false; }
            // if no lines were crossed before this token
            if (token.linesCrossed == 0) return true;
            // restore our position
            script_p = lastScript_p;
            line = lastline;
            token.val = string.Empty;
            return false;
        }

        /// <summary>
        /// Returns the rest of the current line
        /// </summary>
        /// <param name="o">The out.</param>
        /// <returns></returns>
        public string ReadRestOfLine(ref string o)
        {
            while (true)
            {
                if (buffer[script_p] == '\n') { line++; break; }
                if (buffer[script_p] != 0) break;
                if (buffer[script_p] <= ' ') o += ' ';
                else o += buffer[script_p];
                script_p++;
            }
            return o.Trim(' ');
        }

        /// <summary>
        /// read a signed integer
        /// </summary>
        /// <returns></returns>
        public int ParseInt()
        {
            if (!ReadToken(out var token)) { Error("couldn't read expected integer"); return 0; }
            if (token.type == TT.PUNCTUATION && token == "-") { ExpectTokenType(TT.NUMBER, Token.TT_INTEGER, out token); return -token.IntValue; }
            else if (token.type != TT.NUMBER || token.subtype == Token.TT_FLOAT) Error($"expected integer value, found '{token}'");
            return token.IntValue;
        }

        /// <summary>
        /// read a boolean
        /// </summary>
        /// <returns></returns>
        public bool ParseBool()
        {
            if (!ExpectTokenType(TT.NUMBER, 0, out var token)) { Error("couldn't read expected boolean"); return false; }
            return token.IntValue != 0;
        }

        /// <summary>
        /// read a floating point number.  If errorFlag is NULL, a non-numeric token will issue an Error().  If it isn't NULL, it will issue a Warning() and set *errorFlag = true
        /// </summary>
        /// <param name="errorFlag">if set to <c>true</c> [error flag].</param>
        /// <returns></returns>
        public float ParseFloat(Action<bool> errorFlag = null)
        {
            var error = false;
            if (!ReadToken(out var token))
            {
                if (error) { Warning("couldn't read expected floating point number"); error = true; }
                else Error("couldn't read expected floating point number");
                return 0f;
            }
            if (token.type == TT.PUNCTUATION && token == "-")
            {
                ExpectTokenType(TT.NUMBER, 0, out token);
                return -token.FloatValue;
            }
            else if (token.type != TT.NUMBER)
            {
                if (error) { Warning($"expected float value, found '{token}'"); error = true; }
                else Error($"expected float value, found '{token}'");
            }
            errorFlag?.Invoke(error);
            return token.FloatValue;
        }

        // parse matrices with floats
        public unsafe bool Parse1DMatrix(int x, float* m)
        {
            if (!ExpectTokenString("(")) return false;
            for (var i = 0; i < x; i++) m[i] = ParseFloat();
            if (!ExpectTokenString(")")) return false;
            return true;
        }
        public bool Parse1DMatrix(int x, float[] m, int offset = 0)
        {
            if (!ExpectTokenString("(")) return false;
            for (var i = 0; i < x; i++) m[i] = ParseFloat();
            if (!ExpectTokenString(")")) return false;
            return true;
        }
        public unsafe bool Parse2DMatrix(int y, int x, float* m)
        {
            if (!ExpectTokenString("(")) return false;
            for (var i = 0; i < y; i++) if (!Parse1DMatrix(x, m + i * x)) return false;
            if (!ExpectTokenString(")")) return false;
            return true;
        }
        public bool Parse2DMatrix(int y, int x, float[] m, int offset = 0)
        {
            if (!ExpectTokenString("(")) return false;
            for (var i = 0; i < y; i++) if (!Parse1DMatrix(x, m, i * x)) return false;
            if (!ExpectTokenString(")")) return false;
            return true;
        }
        public unsafe bool Parse3DMatrix(int z, int y, int x, float* m)
        {
            if (!ExpectTokenString("(")) return false;
            for (var i = 0; i < z; i++) if (!Parse2DMatrix(y, x, m + i * x * y)) return false;
            if (!ExpectTokenString(")")) return false;
            return true;
        }
        public bool Parse3DMatrix(int z, int y, int x, float[] m, int offset = 0)
        {
            if (!ExpectTokenString("(")) return false;
            for (var i = 0; i < z; i++) if (!Parse2DMatrix(y, x, m, i * x * y)) return false;
            if (!ExpectTokenString(")")) return false;
            return true;
        }

        /// <summary>
        /// parse a braced section into a string
        /// The next token should be an open brace. Parses until a matching close brace is found.
        /// Internal brace depths are properly skipped.
        /// </summary>
        /// <param name="o">The out.</param>
        /// <returns></returns>
        public string ParseBracedSection(out string o)
        {
            o = string.Empty;
            if (!ExpectTokenString("{")) return o;

            o = "{";
            var depth = 1;
            do
            {
                if (!ReadToken(out var token)) { Error("missing closing brace"); return o; }

                // if the token is on a new line
                for (var i = 0; i < token.linesCrossed; i++) o += "\r\n";

                if (token.type == TT.PUNCTUATION)
                {
                    if (token[0] == '{') depth++;
                    else if (token[0] == '}') depth--;
                }

                o += token.type == TT.STRING ? "\"" + token + "\"" : " ";
                o += " ";
            } while (depth != 0);

            return o;
        }

        /// <summary>
        /// parse a braced section into a string, maintaining indents and newlines
        /// The next token should be an open brace. Parses until a matching close brace is found.
        /// Maintains exact characters between braces.
        /// </summary>
        /// <param name="o">The out.</param>
        /// <param name="tabs">The tabs.</param>
        /// <returns></returns>
        public string ParseBracedSectionExact(out string o, int tabs = -1)
        {
            o = string.Empty;
            if (!ExpectTokenString("{")) return o;

            o = "{";
            var depth = 1;
            var skipWhite = false;
            var doTabs = tabs >= 0;

            while (depth != 0 && buffer[script_p] != 0)
            {
                var c = buffer[script_p++];
                switch (c)
                {
                    case '\t': case ' ': if (skipWhite) continue; break;
                    case '\n': if (doTabs) { skipWhite = true; o += c; continue; } break;
                    case '{': depth++; tabs++; break;
                    case '}': depth--; tabs--; break;
                }

                if (skipWhite)
                {
                    var i = tabs;
                    if (c == '{') i--;
                    skipWhite = false;
                    for (; i > 0; i--) o += '\t';
                }
                o += c;
            }
            return o;
        }

        // 
        /// <summary>
        /// parse the rest of the line
        /// </summary>
        /// <param name="o">The out.</param>
        /// <returns></returns>
        public string ParseRestOfLine(out string o)
        {
            o = string.Empty;
            while (ReadToken(out var token))
            {
                if (token.linesCrossed != 0)
                {
                    script_p = lastScript_p;
                    line = lastline;
                    break;
                }
                if (o.Length != 0) o += " ";
                o += token;
            }
            return o;
        }

        /// <summary>
        /// retrieves the white space characters before the last read token
        /// </summary>
        /// <param name="whiteSpace">The white space.</param>
        /// <returns></returns>
        public int GetLastWhiteSpace(out string whiteSpace)
        {
            whiteSpace = string.Empty;
            for (var p = whiteSpaceStart_p; p < whiteSpaceEnd_p; p++) whiteSpace += buffer[p];
            return whiteSpace.Length;
        }

        /// <summary>
        /// returns start index into text buffer of last white space
        /// </summary>
        /// <returns></returns>
        public int LastWhiteSpaceStart => whiteSpaceStart_p;

        /// <summary>
        /// returns end index into text buffer of last white space
        /// </summary>
        /// <returns></returns>
        public int LastWhiteSpaceEnd => whiteSpaceEnd_p;

        /// <summary>
        /// set an array with punctuations, NULL restores default C/C++ set, see default_punctuations for an example
        /// </summary>
        /// <param name="p">The p.</param>
        public void SetPunctuations((string p, int n)[] p)
        {
            CreatePunctuationTable(p ?? default_punctuations);
            punctuations = p ?? default_punctuations;
        }

        /// <summary>
        /// returns a pointer to the punctuation with the given id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public string GetPunctuationFromId(int id)
        {
            for (var i = 0; i < punctuations.Length; i++) if (punctuations[i].n == id) return punctuations[i].p;
            return "unknown punctuation";
        }

        /// <summary>
        /// get the id for the given punctuation
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        public int GetPunctuationId(string p)
        {
            for (var i = 0; i < punctuations.Length; i++) if (punctuations[i].p == p) return punctuations[i].n;
            return 0;
        }

        /// <summary>
        /// gets or sets the lexer flags
        /// </summary>
        /// <value>
        /// The flags.
        /// </value>
        public LEXFL Flags
        {
            get => flags;
            set => flags = value;
        }

        /// <summary>
        /// reset the lexer
        /// </summary>
        public void Reset()
        {
            // pointer in script buffer
            script_p = 0;
            // pointer in script buffer before reading token
            lastScript_p = 0;
            // begin of white space
            whiteSpaceStart_p = 0;
            // end of white space
            whiteSpaceEnd_p = 0;
            // set if there's a token available in token
            tokenavailable = false;

            line = 1;
            lastline = 1;
            // clear the saved token
            token = string.Empty;
        }

        /// <summary>
        /// returns true if at the end of the file
        /// </summary>
        /// <returns></returns>
        public bool EndOfFile() => script_p >= end_p;

        /// <summary>
        /// returns the current filename
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName => filename;

        /// <summary>
        /// get offset in script
        /// </summary>
        /// <value>
        /// The file offset.
        /// </value>
        public int FileOffset => script_p;

        /// <summary>
        /// get file time
        /// </summary>
        /// <value>
        /// The file time.
        /// </value>
        public DateTime FileTime => fileTime;

        /// <summary>
        /// returns the current line number
        /// </summary>
        /// <value>
        /// The line number.
        /// </value>
        public int LineNum => line;

        /// <summary>
        /// print an error message
        /// </summary>
        /// <param name="str">The string.</param>
        public void Error(string str)
        {
            hadError = true;
            if ((flags & LEXFL.NOERRORS) != 0) return;

            var text = str;
            if ((flags & LEXFL.NOFATALERRORS) != 0) Warning($"file {filename}, line {line}: {text}");
            else Error($"file {filename}, line {line}: {text}");
        }

        /// <summary>
        /// print a warning message
        /// </summary>
        /// <param name="str">The string.</param>
        public void Warning(string str)
        {
            if ((flags & LEXFL.NOWARNINGS) != 0) return;

            var text = str;
            Warning($"file {filename}, line {line}: {text}");
        }

        /// <summary>
        /// returns true if Error() was called with LEXFL_NOFATALERRORS or LEXFL_NOERRORS set
        /// </summary>
        /// <returns></returns>
        public bool HadError => hadError;

        /// <summary>
        /// set the base folder to load files from
        /// </summary>
        /// <param name="path">The path.</param>
        public static void SetBaseFolder(string path) => baseFolder = path;

        void CreatePunctuationTable((string p, int n)[] punctuations)
        {
            // get memory for the table
            int i;
            if (punctuations == default_punctuations)
            {
                punctuationtable = default_punctuationtable;
                nextpunctuation = default_nextpunctuation;
                if (default_setup) return;
                default_setup = true;
                //i = default_punctuations.Length; //: opt
            }
            else
            {
                if (punctuationtable == null || punctuationtable == default_punctuationtable) punctuationtable = new int[256];
                i = punctuations.Length;
                nextpunctuation = new int[i];
            }

            Array.Fill(punctuationtable, int.MaxValue);
            Array.Fill(nextpunctuation, int.MaxValue);

            // add the punctuations in the list to the punctuation table
            int n, lastp;
            for (i = 0; i < punctuations.Length; i++)
            {
                ref (string p, int n) newp = ref punctuations[i];
                lastp = -1;
                // sort the punctuations in this table entry on length (longer punctuations first)
                for (n = punctuationtable[newp.p[0]]; n >= 0; n = nextpunctuation[n])
                {
                    ref (string p, int n) p = ref punctuations[n];
                    if (p.p.Length < newp.p.Length)
                    {
                        nextpunctuation[i] = n;
                        if (lastp >= 0) nextpunctuation[lastp] = i;
                        else punctuationtable[newp.p[0]] = i;
                        break;
                    }
                    lastp = n;
                }
                if (n < 0)
                {
                    nextpunctuation[i] = -1;
                    if (lastp >= 0) nextpunctuation[lastp] = i;
                    else punctuationtable[newp.p[0]] = i;
                }
            }
        }

        /// <summary>
        /// Reads spaces, tabs, C-like comments etc.
        /// When a newline character is found the scripts line counter is increased.
        /// </summary>
        /// <returns></returns>
        bool ReadWhiteSpace()
        {
            while (true)
            {
                // skip white space
                while (buffer[script_p] <= ' ')
                {
                    if (buffer[script_p] == 0) return false;
                    if (buffer[script_p] == '\n') line++;
                    script_p++;
                }
                // skip comments
                if (buffer[script_p] == '/')
                {
                    // comments //
                    if (buffer[script_p + 1] == '/')
                    {
                        script_p++;
                        do
                        {
                            script_p++;
                            if (buffer[script_p] == 0) return false;
                        }
                        while (buffer[script_p] != '\n');
                        line++;
                        script_p++;
                        if (buffer[script_p] == 0) return false;
                        continue;
                    }
                    // comments /* */
                    else if (buffer[script_p] == '*')
                    {
                        script_p++;
                        while (true)
                        {
                            script_p++;
                            if (buffer[script_p] == 0) return false;
                            if (buffer[script_p] == '\n') line++;
                            else if (buffer[script_p] == '/')
                            {
                                if (buffer[script_p] == '*') break;
                                if (buffer[script_p] == '*') Warning("nested comment");
                            }
                        }
                        script_p++;
                        if (buffer[script_p] == 0) return false;
                        script_p++;
                        if (buffer[script_p] == 0) return false;
                        continue;
                    }
                }
                break;
            }
            return false;
        }

        bool ReadEscapeCharacter(out char ch)
        {
            int c, val, i;
            // step over the leading '\\'
            script_p++;
            // determine the escape character
            switch (buffer[script_p])
            {
                case '\\': c = '\\'; break;
                case 'n': c = '\n'; break;
                case 'r': c = '\r'; break;
                case 't': c = '\t'; break;
                case 'v': c = '\v'; break;
                case 'b': c = '\b'; break;
                case 'f': c = '\f'; break;
                case 'a': c = '\a'; break;
                case '\'': c = '\''; break;
                case '\"': c = '\"'; break;
                case '?': c = '?'; break;
                case 'x':
                    script_p++;
                    for (i = 0, val = 0; ; i++, script_p++)
                    {
                        c = buffer[script_p];
                        if (c >= '0' && c <= '9') c = c - '0';
                        else if (c >= 'A' && c <= 'Z') c = c - 'A' + 10;
                        else if (c >= 'a' && c <= 'z') c = c - 'a' + 10;
                        else break;
                        val = (val << 4) + c;
                    }
                    script_p--;
                    if (val > 0xFF) { Warning("too large value in escape character"); val = 0xFF; }
                    c = val;
                    break;
                // NOTE: decimal ASCII code, NOT octal
                default:
                    if (buffer[script_p] < '0' || buffer[script_p] > '9') Error("unknown escape char");
                    for (i = 0, val = 0; ; i++, script_p++)
                    {
                        c = buffer[script_p];
                        if (c >= '0' && c <= '9') c = c - '0';
                        else break;
                        val = val * 10 + c;
                    }
                    script_p--;
                    if (val > 0xFF) { Warning("too large value in escape character"); val = 0xFF; }
                    c = val;
                    break;
            }
            // step over the escape character or the last digit of the number
            script_p++;
            // store the escape character
            ch = (char)c;
            // succesfully read escape character
            return true;
        }

        /// <summary>
        /// Escape characters are interpretted.
        /// Reads two strings with only a white space between them as one string.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="quote">The quote.</param>
        /// <returns></returns>
        bool ReadString(Token token, int quote)
        {
            int tmpline, tmpscript_p;

            token.type = quote == '\"' ? TT.STRING : TT.LITERAL;

            // leading quote
            script_p++;

            while (true)
            {
                // if there is an escape character and escape characters are allowed
                if (buffer[script_p] == '\\' && (flags & LEXFL.NOSTRINGESCAPECHARS) == 0)
                {
                    if (!ReadEscapeCharacter(out var ch)) return false;
                    token.AppendDirty(ch);
                }
                // if a trailing quote
                else if (buffer[script_p] == quote)
                {
                    // step over the quote
                    script_p++;
                    // if consecutive strings should not be concatenated
                    if ((flags & LEXFL.NOSTRINGCONCAT) != 0 && ((flags & LEXFL.ALLOWBACKSLASHSTRINGCONCAT) == 0 || quote != '\"')) break;

                    tmpscript_p = script_p;
                    tmpline = line;
                    // read white space between possible two consecutive strings
                    if (!ReadWhiteSpace()) { script_p = tmpscript_p; line = tmpline; break; }

                    if ((flags & LEXFL.NOSTRINGCONCAT) != 0)
                    {
                        if (buffer[script_p] != '\\') { script_p = tmpscript_p; line = tmpline; break; }
                        // step over the '\\'
                        script_p++;
                        if (!ReadWhiteSpace() || (buffer[script_p] != quote)) { Error("expecting string after '\' terminated line"); return false; }
                    }

                    // if there's no leading qoute
                    if (buffer[script_p] != quote) { script_p = tmpscript_p; line = tmpline; break; }
                    // step over the new leading quote
                    script_p++;
                }
                else
                {
                    if (buffer[script_p] == '\0') { Error("missing trailing quote"); return false; }
                    if (buffer[script_p] == '\n') { Error("newline inside string"); return false; }
                    token.AppendDirty(buffer[script_p++]);
                }
            }
            //token.data[token.len] = '\0';

            if (token.type == TT.LITERAL)
            {
                if ((flags & LEXFL.ALLOWMULTICHARLITERALS) == 0 && token.val.Length != 1) Warning("literal is not one character long");
                token.subtype = token.val[0];
            }
            // the sub type is the length of the string
            else token.subtype = token.val.Length;
            return true;
        }

        bool ReadName(Token token)
        {
            char c;
            token.type = TT.NAME;
            do
            {
                token.AppendDirty(buffer[script_p++]);
                c = buffer[script_p];
            } while (
                (c >= 'a' && c <= 'z') ||
                (c >= 'A' && c <= 'Z') ||
                (c >= '0' && c <= '9') ||
                c == '_' ||
                // if treating all tokens as strings, don't parse '-' as a seperate token
                ((flags & LEXFL.ONLYSTRINGS) != 0 && (c == '-')) ||
                // if special path name characters are allowed
                ((flags & LEXFL.ALLOWPATHNAMES) != 0 && (c == '/' || c == '\\' || c == ':' || c == '.')));
            //token.data[token.len] = '\0';
            //the sub type is the length of the name
            token.subtype = token.val.Length;
            return true;
        }

        bool ReadNumber(Token token)
        {
            int i;
            int dot;
            char c, c2;

            token.type = TT.NUMBER;
            token.subtype = 0;
            token.intvalue = 0;
            token.floatvalue = 0;

            c = buffer[script_p];
            c2 = buffer[script_p + 1];

            if (c == '0' && c2 != '.')
            {
                // check for a hexadecimal number
                if (c2 == 'x' || c2 == 'X')
                {
                    token.AppendDirty(buffer[script_p++]);
                    token.AppendDirty(buffer[script_p++]);
                    c = buffer[script_p];
                    while ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')) { token.AppendDirty(c); c = buffer[script_p]; }
                    token.subtype = Token.TT_HEX | Token.TT_INTEGER;
                }
                // check for a binary number
                else if (c2 == 'b' || c2 == 'B')
                {
                    token.AppendDirty(buffer[script_p++]);
                    token.AppendDirty(buffer[script_p++]);
                    c = buffer[script_p];
                    while (c == '0' || c == '1') { token.AppendDirty(c); c = buffer[++script_p]; }
                    token.subtype = Token.TT_BINARY | Token.TT_INTEGER;
                }
                // its an octal number
                else
                {
                    token.AppendDirty(buffer[script_p++]);
                    c = buffer[script_p];
                    while (c >= '0' && c <= '7') { token.AppendDirty(c); c = buffer[++script_p]; }
                    token.subtype = Token.TT_OCTAL | Token.TT_INTEGER;
                }
            }
            else
            {
                // decimal integer or floating point number or ip address
                dot = 0;
                while (true)
                {
                    if (c >= '0' && c <= '9') { }
                    else if (c == '.') dot++;
                    else break;
                    token.AppendDirty(c);
                    c = buffer[++script_p];
                }
                if (c == 'e' && dot == 0) dot++; // We have scientific notation without a decimal point
                // if a floating point number
                if (dot == 1)
                {
                    token.subtype = Token.TT_DECIMAL | Token.TT_FLOAT;
                    // check for floating point exponent
                    if (c == 'e')
                    {
                        // Append the e so that GetFloatValue code works
                        token.AppendDirty(c);
                        c = buffer[++script_p];
                        if (c == '-') { token.AppendDirty(c); c = buffer[++script_p]; }
                        else if (c == '+') { token.AppendDirty(c); c = buffer[++script_p]; }
                        while (c >= '0' && c <= '9') { token.AppendDirty(c); c = buffer[++script_p]; }
                    }
                    // check for floating point exception infinite 1.#INF or indefinite 1.#IND or NaN
                    else if (c == '#')
                    {
                        c2 = '\x4';
                        if (CheckString("INF")) token.subtype |= Token.TT_INFINITE;
                        else if (CheckString("IND")) token.subtype |= Token.TT_INDEFINITE;
                        else if (CheckString("NAN")) token.subtype |= Token.TT_NAN;
                        else if (CheckString("QNAN")) { token.subtype |= Token.TT_NAN; c2++; }
                        else if (CheckString("SNAN")) { token.subtype |= Token.TT_NAN; c2++; }
                        for (i = 0; i < c2; i++) { token.AppendDirty(c); c = buffer[++script_p]; }
                        while (c >= '0' && c <= '9') { token.AppendDirty(c); c = buffer[++script_p]; }
                        if ((flags & LEXFL.ALLOWFLOATEXCEPTIONS) == 0) { token.AppendDirty('\0'); Error($"parsed {token}"); } // zero terminate for c_str
                    }
                }
                else if (dot > 1)
                {
                    if ((flags & LEXFL.ALLOWIPADDRESSES) == 0) { Error("more than one dot in number"); return false; }
                    if (dot != 3) { Error("ip address should have three dots"); return false; }
                    token.subtype = Token.TT_IPADDRESS;
                }
                else token.subtype = Token.TT_DECIMAL | Token.TT_INTEGER;
            }

            if ((token.subtype & Token.TT_FLOAT) != 0)
            {
                if (c > ' ')
                {
                    // single-precision: float
                    if (c == 'f' || c == 'F') { token.subtype |= Token.TT_SINGLE_PRECISION; script_p++; }
                    // extended-precision: long double
                    else if (c == 'l' || c == 'L') { token.subtype |= Token.TT_EXTENDED_PRECISION; script_p++; }
                    // default is double-precision: double
                    else token.subtype |= Token.TT_DOUBLE_PRECISION;
                }
                else token.subtype |= Token.TT_DOUBLE_PRECISION;
            }
            else if ((token.subtype & Token.TT_INTEGER) != 0)
            {
                if (c > ' ')
                    // default: signed long
                    for (i = 0; i < 2; i++)
                    {
                        // long integer
                        if (c == 'l' || c == 'L') token.subtype |= Token.TT_LONG;
                        // unsigned integer
                        else if (c == 'u' || c == 'U') token.subtype |= Token.TT_UNSIGNED;
                        else break;
                        c = buffer[++script_p];
                    }
            }
            else if ((token.subtype & Token.TT_IPADDRESS) != 0)
            {
                if (c == ':')
                {
                    token.AppendDirty(c);
                    c = buffer[++script_p];
                    while (c >= '0' && c <= '9') { token.AppendDirty(c); c = buffer[++script_p]; }
                    token.subtype |= Token.TT_IPPORT;
                }
            }
            //token.data[token.len] = '\0';
            return true;
        }

        bool ReadPunctuation(Token token)
        {
            int l;
            for (var n = punctuationtable[buffer[script_p]]; n >= 0; n = nextpunctuation[n])
            {
                ref (string p, int n) punc = ref punctuations[n];
                var p = punc.p;
                // check for this punctuation in the script
                for (l = 0; p[l] != 0 && buffer[script_p + l] != 0; l++)
                    if (buffer[script_p + l] != p[l])
                        break;
                if (p[l] == 0)
                {
                    for (var i = 0; i <= l; i++)
                        token.val += p[i];
                    script_p += l;
                    token.type = TT.PUNCTUATION;
                    // sub type is the punctuation id
                    token.subtype = punc.n;
                    return true;
                }
            }
            return false;
        }

        //bool ReadPrimitive(Token token) { }

        bool CheckString(string str)
        {
            for (var i = 0; i < str.Length; i++) if (buffer[script_p + i] != str[i]) return false;
            return true;
        }

        int NumLinesCrossed => line - lastline;
    }
}
