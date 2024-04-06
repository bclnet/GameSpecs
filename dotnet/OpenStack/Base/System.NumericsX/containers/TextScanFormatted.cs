using System.Collections.Generic;
using System.Linq;

namespace System.NumericsX
{
    /// <summary>
    /// Class that provides functionality of the standard C library sscanf() function.
    /// </summary>
    public class TextScanFormatted
    {
        public static int Scan<T1, T2>(string s, string fmt, out T1 t1, out T2 t2)
        {
            var r = Parse(s, fmt, out var values);
            t1 = (T1)values[0];
            t2 = (T2)values[1];
            return r;
        }
        public static int Scan<T1, T2, T3>(string s, string fmt, out T1 t1, out T2 t2, out T3 t3)
        {
            var r = Parse(s, fmt, out var values);
            t1 = (T1)values[0];
            t2 = (T2)values[1];
            t3 = (T3)values[2];
            return r;
        }
        public static int Scan<T1, T2, T3, T4>(string s, string fmt, out T1 t1, out T2 t2, out T3 t3, out T4 t4)
        {
            var r = Parse(s, fmt, out var values);
            t1 = (T1)values[0];
            t2 = (T2)values[1];
            t3 = (T3)values[2];
            t4 = (T4)values[3];
            return r;
        }
        public static int Scan<T1, T2, T3, T4, T5>(string s, string fmt, out T1 t1, out T2 t2, out T3 t3, out T4 t4, out T5 t5)
        {
            var r = Parse(s, fmt, out var values);
            t1 = (T1)values[0];
            t2 = (T2)values[1];
            t3 = (T3)values[2];
            t4 = (T4)values[3];
            t5 = (T5)values[4];
            return r;
        }
        public static int Scan<T1, T2, T3, T4, T5, T6>(string s, string fmt, out T1 t1, out T2 t2, out T3 t3, out T4 t4, out T5 t5, out T6 t6)
        {
            var r = Parse(s, fmt, out var values);
            t1 = (T1)values[0];
            t2 = (T2)values[1];
            t3 = (T3)values[2];
            t4 = (T4)values[3];
            t5 = (T5)values[4];
            t6 = (T6)values[5];
            return r;
        }
        public static int Scan<T1, T2, T3, T4, T5, T6, T7>(string s, string fmt, out T1 t1, out T2 t2, out T3 t3, out T4 t4, out T5 t5, out T6 t6, out T7 t7)
        {
            var r = Parse(s, fmt, out var values);
            t1 = (T1)values[0];
            t2 = (T2)values[1];
            t3 = (T3)values[2];
            t4 = (T4)values[3];
            t5 = (T5)values[4];
            t6 = (T6)values[5];
            t7 = (T7)values[6];
            return r;
        }
        public static int Scan<T1, T2, T3, T4, T5, T6, T7, T8>(string s, string fmt, out T1 t1, out T2 t2, out T3 t3, out T4 t4, out T5 t5, out T6 t6, out T7 t7, out T8 t8)
        {
            var r = Parse(s, fmt, out var values);
            t1 = (T1)values[0];
            t2 = (T2)values[1];
            t3 = (T3)values[2];
            t4 = (T4)values[3];
            t5 = (T5)values[4];
            t6 = (T6)values[5];
            t7 = (T7)values[6];
            t8 = (T8)values[7];
            return r;
        }
        public static int Scan<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string s, string fmt, out T1 t1, out T2 t2, out T3 t3, out T4 t4, out T5 t5, out T6 t6, out T7 t7, out T8 t8, out T9 t9)
        {
            var r = Parse(s, fmt, out var values);
            t1 = (T1)values[0];
            t2 = (T2)values[1];
            t3 = (T3)values[2];
            t4 = (T4)values[3];
            t5 = (T5)values[4];
            t6 = (T6)values[5];
            t7 = (T7)values[6];
            t8 = (T8)values[7];
            t9 = (T9)values[8];
            return r;
        }

        // Format type specifiers
        enum Types
        {
            Character,
            Decimal,
            Float,
            Hexadecimal,
            Octal,
            ScanSet,
            String,
            Unsigned
        }

        // Format modifiers
        enum Modifiers
        {
            None,
            ShortShort,
            Short,
            Long,
            LongLong
        }

        // Delegate to parse a type
        delegate bool ParseValue(Action<object> add, TextVisiter input, FormatSpecifier spec);

        // Class to associate format type with type parser
        class TypeParser
        {
            public Types Type { get; set; }
            public ParseValue Parser { get; set; }
        }

        // Class to hold format specifier information
        class FormatSpecifier
        {
            public Types Type { get; set; }
            public Modifiers Modifier { get; set; }
            public int Width { get; set; }
            public bool NoResult { get; set; }
            public string ScanSet { get; set; }
            public bool ScanSetExclude { get; set; }
        }

        // Lookup table to find parser by parser type
        static readonly TypeParser[] _typeParsers = new[] {
            new TypeParser { Type = Types.Character, Parser = ParseCharacter },
            new TypeParser { Type = Types.Decimal, Parser = ParseDecimal },
            new TypeParser { Type = Types.Float, Parser = ParseFloat },
            new TypeParser { Type = Types.Hexadecimal, Parser = ParseHexadecimal },
            new TypeParser { Type = Types.Octal, Parser = ParseOctal },
            new TypeParser { Type = Types.ScanSet, Parser = ParseScanSet },
            new TypeParser { Type = Types.String, Parser = ParseString },
            new TypeParser { Type = Types.Unsigned, Parser = ParseDecimal }
        };

        /// <summary>
        /// Parses the input string according to the rules in the
        /// format string. Similar to the standard C library's
        /// sscanf() function. Parsed fields are placed in the
        /// class' Results member.
        /// </summary>
        /// <param name="input">String to parse</param>
        /// <param name="format">Specifies rules for parsing input</param>
        public static int Parse(string input, string format, out IList<object> values)
        {
            var inp = new TextVisiter(input);
            var fmt = new TextVisiter(format);
            var results = new List<object>();
            var spec = new FormatSpecifier();
            var count = 0;

            // Process input string as indicated in format string
            while (!fmt.EndOfText && !inp.EndOfText)
                if (ParseFormatSpecifier(fmt, spec))
                {
                    // Found a format specifier
                    var parser = _typeParsers.First(tp => tp.Type == spec.Type);
                    if (parser.Parser(results.Add, inp, spec)) count++;
                    else break;
                }
                else if (char.IsWhiteSpace(fmt.Peek()))
                {
                    // Whitespace
                    inp.MovePastWhitespace();
                    fmt.MoveAhead();
                }
                else if (fmt.Peek() == inp.Peek())
                {
                    // Matching character
                    inp.MoveAhead();
                    fmt.MoveAhead();
                }
                else break;    // Break at mismatch

            // Return number of fields successfully parsed
            values = results;
            return count;
        }

        /// <summary>
        /// Attempts to parse a field format specifier from the format string.
        /// </summary>
        static bool ParseFormatSpecifier(TextVisiter format, FormatSpecifier spec)
        {
            // Return if not a field format specifier
            if (format.Peek() != '%') return false;
            format.MoveAhead();

            // Return if "%%" (treat as '%' literal)
            if (format.Peek() == '%') return false;

            // Test for asterisk, which indicates result is not stored
            if (format.Peek() == '*') { spec.NoResult = true; format.MoveAhead(); }
            else spec.NoResult = false;

            // Parse width
            var start = format.Position;
            while (char.IsDigit(format.Peek())) format.MoveAhead();
            if (format.Position > start) spec.Width = int.Parse(format.Extract(start, format.Position));
            else spec.Width = 0;

            // Parse modifier
            if (format.Peek() == 'h')
            {
                format.MoveAhead();
                if (format.Peek() == 'h') { format.MoveAhead(); spec.Modifier = Modifiers.ShortShort; }
                else spec.Modifier = Modifiers.Short;
            }
            else if (char.ToLower(format.Peek()) == 'l')
            {
                format.MoveAhead();
                if (format.Peek() == 'l') { format.MoveAhead(); spec.Modifier = Modifiers.LongLong; }
                else spec.Modifier = Modifiers.Long;
            }
            else spec.Modifier = Modifiers.None;

            // Parse type
            switch (format.Peek())
            {
                case 'c': spec.Type = Types.Character; break;
                case 'd': case 'i': spec.Type = Types.Decimal; break;
                case 'a': case 'A': case 'e': case 'E': case 'f': case 'F': case 'g': case 'G': spec.Type = Types.Float; break;
                case 'o': spec.Type = Types.Octal; break;
                case 's': spec.Type = Types.String; break;
                case 'u': spec.Type = Types.Unsigned; break;
                case 'x': case 'X': spec.Type = Types.Hexadecimal; break;
                case '[':
                    spec.Type = Types.ScanSet;
                    format.MoveAhead();
                    // Parse scan set characters
                    if (format.Peek() == '^') { spec.ScanSetExclude = true; format.MoveAhead(); }
                    else spec.ScanSetExclude = false;
                    start = format.Position;
                    // Treat immediate ']' as literal
                    if (format.Peek() == ']') format.MoveAhead();
                    format.MoveTo(']');
                    if (format.EndOfText)
                        throw new Exception("Type specifier expected character : ']'");
                    spec.ScanSet = format.Extract(start, format.Position);
                    break;
                default: throw new Exception($"Unknown format type specified : '{format.Peek()}'");
            }
            format.MoveAhead();
            return true;
        }

        // Determines if the given digit is valid for the given radix
        static bool IsValidDigit(char c, int radix)
        {
            var i = "0123456789abcdef".IndexOf(char.ToLower(c));
            return i >= 0 && i < radix;
        }

        // Parse signed token and add to results
        static object Signed(string token, Modifiers mod, int radix)
            => mod == Modifiers.ShortShort ? Convert.ToSByte(token, radix)
            : mod == Modifiers.Short ? Convert.ToInt16(token, radix)
            : mod == Modifiers.Long || mod == Modifiers.LongLong ? Convert.ToInt64(token, radix)
            : Convert.ToInt32(token, radix);

        // Parse unsigned token and add to results
        static object Unsigned(string token, Modifiers mod, int radix)
            => mod == Modifiers.ShortShort ? Convert.ToByte(token, radix)
            : mod == Modifiers.Short ? Convert.ToUInt16(token, radix)
            : mod == Modifiers.Long || mod == Modifiers.LongLong ? Convert.ToUInt64(token, radix)
            : Convert.ToUInt32(token, radix);

        #region Parsers

        /// <summary>
        /// Parse a character field
        /// </summary>
        static bool ParseCharacter(Action<object> add, TextVisiter input, FormatSpecifier spec)
        {
            // Parse character(s)
            var start = input.Position;
            var count = (spec.Width > 1) ? spec.Width : 1;
            while (!input.EndOfText && count-- > 0) input.MoveAhead();

            // Extract token
            if (count <= 0 && input.Position > start)
            {
                if (!spec.NoResult)
                {
                    var token = input.Extract(start, input.Position);
                    add(token.Length > 1 ? token.ToCharArray() : token[0]);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Parse integer field
        /// </summary>
        static bool ParseDecimal(Action<object> add, TextVisiter input, FormatSpecifier spec)
        {
            // Skip any whitespace
            input.MovePastWhitespace();

            // Parse leading sign
            var radix = 10;
            var start = input.Position;
            if (input.Peek() == '+' || input.Peek() == '-') input.MoveAhead();
            else if (input.Peek() == '0')
                if (char.ToLower(input.Peek(1)) == 'x') { radix = 16; input.MoveAhead(2); }
                else { radix = 8; input.MoveAhead(); }

            // Parse digits
            while (IsValidDigit(input.Peek(), radix)) input.MoveAhead();

            // Don't exceed field width
            if (spec.Width > 0)
            {
                var count = input.Position - start;
                if (spec.Width < count) input.MoveAhead(spec.Width - count);
            }

            // Extract token
            if (input.Position > start)
            {
                if (!spec.NoResult)
                    add(spec.Type == Types.Decimal
                        ? Signed(input.Extract(start, input.Position), spec.Modifier, radix)
                        : Unsigned(input.Extract(start, input.Position), spec.Modifier, radix));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Parse a floating-point field
        /// </summary>
        static bool ParseFloat(Action<object> add, TextVisiter input, FormatSpecifier spec)
        {
            // Skip any whitespace
            input.MovePastWhitespace();

            // Parse leading sign
            var start = input.Position;
            if (input.Peek() == '+' || input.Peek() == '-') input.MoveAhead();

            // Parse digits
            var hasPoint = false;
            while (char.IsDigit(input.Peek()) || input.Peek() == '.')
            {
                if (input.Peek() == '.') { if (hasPoint) break; hasPoint = true; }
                input.MoveAhead();
            }

            // Parse exponential notation
            if (char.ToLower(input.Peek()) == 'e')
            {
                input.MoveAhead();
                if (input.Peek() == '+' || input.Peek() == '-') input.MoveAhead();
                while (char.IsDigit(input.Peek())) input.MoveAhead();
            }

            // Don't exceed field width
            if (spec.Width > 0)
            {
                var count = input.Position - start;
                if (spec.Width < count) input.MoveAhead(spec.Width - count);
            }

            // Because we parse the exponential notation before we apply any field-width constraint, it becomes awkward to verify
            // we have a valid floating point token. To prevent an exception, we use TryParse() here instead of Parse().

            // Extract token
            if (input.Position > start && double.TryParse(input.Extract(start, input.Position), out var result))
            {
                if (!spec.NoResult)
                    add(spec.Modifier == Modifiers.Long || spec.Modifier == Modifiers.LongLong ? result : result);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Parse hexadecimal field
        /// </summary>
        static bool ParseHexadecimal(Action<object> add, TextVisiter input, FormatSpecifier spec)
        {
            // Skip any whitespace
            input.MovePastWhitespace();

            // Parse 0x prefix
            var start = input.Position;
            if (input.Peek() == '0' && input.Peek(1) == 'x') input.MoveAhead(2);

            // Parse digits
            while (IsValidDigit(input.Peek(), 16)) input.MoveAhead();

            // Don't exceed field width
            if (spec.Width > 0)
            {
                var count = input.Position - start;
                if (spec.Width < count) input.MoveAhead(spec.Width - count);
            }

            // Extract token
            if (input.Position > start)
            {
                if (!spec.NoResult) add(Unsigned(input.Extract(start, input.Position), spec.Modifier, 16));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Parse an octal field
        /// </summary>
        static bool ParseOctal(Action<object> add, TextVisiter input, FormatSpecifier spec)
        {
            // Skip any whitespace
            input.MovePastWhitespace();

            // Parse digits
            var start = input.Position;
            while (IsValidDigit(input.Peek(), 8)) input.MoveAhead();

            // Don't exceed field width
            if (spec.Width > 0)
            {
                var count = input.Position - start;
                if (spec.Width < count) input.MoveAhead(spec.Width - count);
            }

            // Extract token
            if (input.Position > start)
            {
                if (!spec.NoResult) add(Unsigned(input.Extract(start, input.Position), spec.Modifier, 8));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Parse a scan-set field
        /// </summary>
        static bool ParseScanSet(Action<object> add, TextVisiter input, FormatSpecifier spec)
        {

            // Parse characters
            var start = input.Position;
            if (!spec.ScanSetExclude) while (spec.ScanSet.Contains(input.Peek())) input.MoveAhead();
            else while (!input.EndOfText && !spec.ScanSet.Contains(input.Peek())) input.MoveAhead();

            // Don't exceed field width
            if (spec.Width > 0)
            {
                var count = input.Position - start;
                if (spec.Width < count) input.MoveAhead(spec.Width - count);
            }

            // Extract token
            if (input.Position > start)
            {
                if (!spec.NoResult) add(input.Extract(start, input.Position));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Parse a string field
        /// </summary>
        static bool ParseString(Action<object> add, TextVisiter input, FormatSpecifier spec)
        {
            // Skip any whitespace
            input.MovePastWhitespace();

            // Parse string characters
            var start = input.Position;
            while (!input.EndOfText && !char.IsWhiteSpace(input.Peek())) input.MoveAhead();

            // Don't exceed field width
            if (spec.Width > 0)
            {
                var count = input.Position - start;
                if (spec.Width < count) input.MoveAhead(spec.Width - count);
            }

            // Extract token
            if (input.Position > start)
            {
                if (!spec.NoResult) add(input.Extract(start, input.Position));
                return true;
            }
            return false;
        }

        #endregion
    }
}