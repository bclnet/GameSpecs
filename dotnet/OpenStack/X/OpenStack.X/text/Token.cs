using System.Diagnostics;

namespace System.NumericsX.OpenStack
{
    // token types
    public enum TT
    {
        STRING = 1,         // string
        LITERAL = 2,        // literal
        NUMBER = 3,         // number
        NAME = 4,           // name
        PUNCTUATION = 5,    // punctuation
    }

    public class Token
    {
        internal uint intvalue;                             // integer value
        internal double floatvalue;                         // floating point value
        internal int whiteSpaceStart_p;                     // start of white space before token, only used by Lexer
        internal int whiteSpaceEnd_p;                       // end of white space before token, only used by Lexer
        internal Token next;                                // next token in chain, only used by Parser

        public static implicit operator Token(string s) => new(s);
        public static implicit operator string(Token t) => t.val;
        public override string ToString() => val;

        internal Token() => val = string.Empty;
        public Token(string s) => val = s;
        public char this[int index] => val[index];

        // number sub types
        public const int TT_INTEGER = 0x00001;            // integer
        public const int TT_DECIMAL = 0x00002;            // decimal number
        public const int TT_HEX = 0x00004;                // hexadecimal number
        public const int TT_OCTAL = 0x00008;              // octal number
        public const int TT_BINARY = 0x00010;             // binary number
        public const int TT_LONG = 0x00020;               // long int
        public const int TT_UNSIGNED = 0x00040;           // unsigned int
        public const int TT_FLOAT = 0x00080;              // floating point number
        public const int TT_SINGLE_PRECISION = 0x00100;   // float
        public const int TT_DOUBLE_PRECISION = 0x00200;   // double
        public const int TT_EXTENDED_PRECISION = 0x00400; // long double
        public const int TT_INFINITE = 0x00800;           // infinite 1.#INF
        public const int TT_INDEFINITE = 0x01000;         // indefinite 1.#IND
        public const int TT_NAN = 0x02000;                // NaN
        public const int TT_IPADDRESS = 0x04000;          // ip address
        public const int TT_IPPORT = 0x08000;             // ip port
        public const int TT_VALUESVALID = 0x10000;        // set if intvalue and floatvalue are valid

        public string val;

        // string sub type is the length of the string. literal sub type is the ASCII code
        // punctuation sub type is the punctuation id. name sub type is the length of the name
        /// <summary>
        /// token type
        /// </summary>
        public TT type;

        /// <summary>
        /// token sub type
        /// </summary>
        public int subtype;

        /// <summary>
        /// line in script the token was on
        /// </summary>
        public int line;

        /// <summary>
        /// number of lines crossed in white space before token
        /// </summary>
        public int linesCrossed;

        /// <summary>
        /// token flags, used for recursive defines
        /// </summary>
        public int flags;

        /// <summary>
        /// double value of TT_NUMBER
        /// </summary>
        /// <returns></returns>
        public double DoubleValue
        {
            get
            {
                if (type != TT.NUMBER) return 0.0;
                if ((subtype & TT_VALUESVALID) == 0) NumberValue();
                return floatvalue;
            }
        }

        /// <summary>
        /// float value of TT_NUMBER
        /// </summary>
        /// <returns></returns>
        public float FloatValue
            => (float)DoubleValue;

        /// <summary>
        /// unsigned int value of TT_NUMBER
        /// </summary>
        /// <returns></returns>
        public uint UnsignedIntValue
        {
            get
            {
                if (type != TT.NUMBER) return 0;
                if ((subtype & TT_VALUESVALID) == 0) NumberValue();
                return intvalue;
            }
        }

        /// <summary>
        /// int value of TT_NUMBER
        /// </summary>
        /// <value>
        /// The int value.
        /// </value>
        public int IntValue
            => (int)UnsignedIntValue;

        /// <summary>
        /// returns length of whitespace before token
        /// </summary>
        /// <returns></returns>
        public bool WhiteSpaceBeforeToken
            => whiteSpaceEnd_p > whiteSpaceStart_p;

        /// <summary>
        /// forget whitespace before token
        /// </summary>
        public void ClearTokenWhiteSpace()
        {
            whiteSpaceStart_p = 0;
            whiteSpaceEnd_p = 0;
            linesCrossed = 0;
        }

        /// <summary>
        /// calculate values for a TT_NUMBER
        /// </summary>
        public unsafe void NumberValue()
        {
            int i, pow, c, p; bool div; double m;
            Debug.Assert(type == TT.NUMBER);

            p = 0;
            floatvalue = 0;
            intvalue = 0;
            // floating point number
            if ((subtype & TT_FLOAT) != 0)
            {
                if ((subtype & (TT_INFINITE | TT_INDEFINITE | TT_NAN)) != 0)
                {
                    if ((subtype & TT_INFINITE) != 0) { uint inf = 0x7f800000; floatvalue = *(float*)&inf; } // 1.#INF
                    else if ((subtype & TT_INDEFINITE) != 0) { uint ind = 0xffc00000; floatvalue = *(float*)&ind; } // 1.#IND
                    else if ((subtype & TT_NAN) != 0) { uint nan = 0x7fc00000; floatvalue = *(float*)&nan; } // 1.#QNAN
                }
                else
                {
                    while (p < val.Length && val[p] != '.' && val[p] != 'e') { floatvalue = floatvalue * 10.0 + (val[p] - '0'); p++; }
                    if (val[p] == '.')
                    {
                        p++;
                        for (m = 0.1; p < val.Length && val[p] != 'e'; p++) { floatvalue += (val[p] - '0') * m; m *= 0.1; }
                    }
                    if (val[p] == 'e')
                    {
                        p++;
                        if (val[p] == '-') { div = true; p++; }
                        else if (val[p] == '+') { div = false; p++; }
                        else div = false;
                        for (pow = 0; p < val.Length; p++) pow = pow * 10 + (val[p] - '0');
                        for (m = 1.0, i = 0; i < pow; i++) m *= 10.0;
                        if (div) floatvalue /= m;
                        else floatvalue *= m;
                    }
                }
                intvalue = MathX.Ftol(floatvalue);
            }
            else if ((subtype & TT_DECIMAL) != 0)
            {
                while (p < val.Length) { intvalue = intvalue * 10 + ((uint)val[p] - '0'); p++; }
                floatvalue = intvalue;
            }
            else if ((subtype & TT_IPADDRESS) != 0)
            {
                c = 0;
                while (p < val.Length && val[p] != ':')
                {
                    if (val[p] == '.')
                    {
                        while (c != 3) { intvalue *= 10; c++; }
                        c = 0;
                    }
                    else
                    {
                        intvalue = intvalue * 10 + ((uint)val[p] - '0');
                        c++;
                    }
                    p++;
                }
                while (c != 3) { intvalue *= 10; c++; }
                floatvalue = intvalue;
            }
            else if ((subtype & TT_OCTAL) != 0)
            {
                // step over the first zero
                p += 1;
                while (p < val.Length) { intvalue = (intvalue << 3) + ((uint)val[p] - '0'); p++; }
                floatvalue = intvalue;
            }
            else if ((subtype & TT_HEX) != 0)
            {
                // step over the leading 0x or 0X
                p += 2;
                while (p < val.Length)
                {
                    intvalue <<= 4;
                    if (val[p] >= 'a' && val[p] <= 'f') intvalue += (uint)val[p] - 'a' + 10;
                    else if (val[p] >= 'A' && val[p] <= 'F') intvalue += (uint)val[p] - 'A' + 10;
                    else intvalue += (uint)val[p] - '0';
                    p++;
                }
                floatvalue = intvalue;
            }
            else if ((subtype & TT_BINARY) != 0)
            {
                // step over the leading 0b or 0B
                p += 2;
                while (p < val.Length) { intvalue = (intvalue << 1) + ((uint)val[p] - '0'); p++; }
                floatvalue = intvalue;
            }
            subtype |= TT_VALUESVALID;
        }

        /// <summary>
        /// append character without adding trailing zero
        /// </summary>
        /// <param name="a">a.</param>
        internal void AppendDirty(char a)
            => val += a;
    }
}