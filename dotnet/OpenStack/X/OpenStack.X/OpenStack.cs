#define FRUSTUM_DEBUG
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("OpenStack.XTests")]

namespace System.NumericsX.OpenStack
{
    public static class OpenStack
    {
        public static string GAME_NAME = "GAME_NAME";
        public static int com_frameTime;            //:sky (attach) time for the current frame in milliseconds
        public static volatile int com_ticNumber;   //:sky (attach)		// 60 hz tics, incremented by async function
        public static Func<string, DateTime> Sys_FileTimeStamp => (path) => DateTime.MinValue;
        public static Func<string> Sys_GetClipboardData => () => null;
        public static Func<bool> Session_IsMultiplayer => () => false;

        public static ISystem system;
        public static ICommon common;
        public static IConsole console;
        internal static CVarSystemLocal cvarSystemLocal = new(); public static ICVarSystem cvarSystem = cvarSystemLocal;
        internal static CmdSystemLocal cmdSystemLocal = new(); public static ICmdSystem cmdSystem = cmdSystemLocal;
        public static IVFileSystem fileSystem;
        public static IUsercmd usercmdGen; //internal static CmdSystemLocal usercmdGenLocal = new(); public static IUsercmdGen usercmdGen = cmdSystemLocal;

#if FRUSTUM_DEBUG
        static readonly CVar r_showInteractionScissors_0 = new("r_showInteractionScissors", "0", CVAR.RENDERER | CVAR.INTEGER, "", 0, 2, CmdArgs.ArgCompletion_Integer(0, 2));
        static readonly CVar r_showInteractionScissors_1 = new("r_showInteractionScissors", "0", CVAR.RENDERER | CVAR.INTEGER, "", 0, 2, CmdArgs.ArgCompletion_Integer(0, 2));
#endif

        #region Colors

        // color escape character
        public const int C_COLOR_ESCAPE = '^';
        public const int C_COLOR_DEFAULT = '0';
        public const int C_COLOR_RED = '1';
        public const int C_COLOR_GREEN = '2';
        public const int C_COLOR_YELLOW = '3';
        public const int C_COLOR_BLUE = '4';
        public const int C_COLOR_CYAN = '5';
        public const int C_COLOR_MAGENTA = '6';
        public const int C_COLOR_WHITE = '7';
        public const int C_COLOR_GRAY = '8';
        public const int C_COLOR_BLACK = '9';

        // color escape string
        public const string S_COLOR_DEFAULT = "^0";
        public const string S_COLOR_RED = "^1";
        public const string S_COLOR_GREEN = "^2";
        public const string S_COLOR_YELLOW = "^3";
        public const string S_COLOR_BLUE = "^4";
        public const string S_COLOR_CYAN = "^5";
        public const string S_COLOR_MAGENTA = "^6";
        public const string S_COLOR_WHITE = "^7";
        public const string S_COLOR_GRAY = "^8";
        public const string S_COLOR_BLACK = "^9";

        // basic colors
        public static readonly Vector4 colorBlack = new(0.00f, 0.00f, 0.00f, 1.00f);
        public static readonly Vector4 colorWhite = new(1.00f, 1.00f, 1.00f, 1.00f);
        public static readonly Vector4 colorRed = new(1.00f, 0.00f, 0.00f, 1.00f);
        public static readonly Vector4 colorGreen = new(0.00f, 1.00f, 0.00f, 1.00f);
        public static readonly Vector4 colorBlue = new(0.00f, 0.00f, 1.00f, 1.00f);
        public static readonly Vector4 colorYellow = new(1.00f, 1.00f, 0.00f, 1.00f);
        public static readonly Vector4 colorMagenta = new(1.00f, 0.00f, 1.00f, 1.00f);
        public static readonly Vector4 colorCyan = new(0.00f, 1.00f, 1.00f, 1.00f);
        public static readonly Vector4 colorOrange = new(1.00f, 0.50f, 0.00f, 1.00f);
        public static readonly Vector4 colorPurple = new(0.60f, 0.00f, 0.60f, 1.00f);
        public static readonly Vector4 colorPink = new(0.73f, 0.40f, 0.48f, 1.00f);
        public static readonly Vector4 colorBrown = new(0.40f, 0.35f, 0.08f, 1.00f);
        public static readonly Vector4 colorLtGrey = new(0.75f, 0.75f, 0.75f, 1.00f);
        public static readonly Vector4 colorMdGrey = new(0.50f, 0.50f, 0.50f, 1.00f);
        public static readonly Vector4 colorDkGrey = new(0.25f, 0.25f, 0.25f, 1.00f);
        public static readonly uint[] colorMask = new[] { 255U, 0U };

        static byte ColorFloatToByte(float c)
            => (byte)(((uint)(c * 255.0f)) & colorMask[MathX.FLOATSIGNBITSET(c) ? 1 : 0]);

        // packs color floats in the range [0,1] into an integer
        public static uint PackColor(ref Vector3 color)
        {
            uint dx, dy, dz;

            dx = ColorFloatToByte(color.x);
            dy = ColorFloatToByte(color.y);
            dz = ColorFloatToByte(color.z);
#if !BIG_ENDIAN
            return (dx << 0) | (dy << 8) | (dz << 16);
#else
            return (dy << 16) | (dz << 8) | (dx << 0);
#endif
        }
        public static uint PackColor(ref Vector4 color)
        {
            uint dw, dx, dy, dz;

            dx = ColorFloatToByte(color.x);
            dy = ColorFloatToByte(color.y);
            dz = ColorFloatToByte(color.z);
            dw = ColorFloatToByte(color.w);
#if !BIG_ENDIAN
            return (dx << 0) | (dy << 8) | (dz << 16) | (dw << 24);
#else
            return (dx << 24) | (dy << 16) | (dz << 8) | (dw << 0);
#endif
        }

        public static void UnpackColor(uint color, ref Vector3 unpackedColor)
        {
#if !BIG_ENDIAN
            unpackedColor.Set(((color >> 0) & 255) * (1.0f / 255.0f),
                                ((color >> 8) & 255) * (1.0f / 255.0f),
                                ((color >> 16) & 255) * (1.0f / 255.0f));
#else
            unpackedColor.Set(((color >> 16) & 255) * (1.0f / 255.0f),
                                ((color >> 8) & 255) * (1.0f / 255.0f),
                                ((color >> 0) & 255) * (1.0f / 255.0f));
#endif
        }

        public static void UnpackColor(uint color, ref Vector4 unpackedColor)
        {
#if !BIG_ENDIAN
            unpackedColor.Set(((color >> 0) & 255) * (1.0f / 255.0f),
                                ((color >> 8) & 255) * (1.0f / 255.0f),
                                ((color >> 16) & 255) * (1.0f / 255.0f),
                                ((color >> 24) & 255) * (1.0f / 255.0f));
#else
            unpackedColor.Set(((color >> 24) & 255) * (1.0f / 255.0f),
                                ((color >> 16) & 255) * (1.0f / 255.0f),
                                ((color >> 8) & 255) * (1.0f / 255.0f),
                                ((color >> 0) & 255) * (1.0f / 255.0f));
#endif
        }

        #endregion
    }
}