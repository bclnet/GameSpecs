using static System.NumericsX.OpenStack.Key;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.System
{
    public static partial class SysW
    {
        static readonly byte[] s_scantokey = {
        //	 0    1    2    3    4    5    6    7    8    9    A    B    C    D    E    F
	          0,  27,  49,  50,  51,  52,  53,  54,  55,  56,  57,  48,  45,  61, 127,   9,   // 0
	        113, 119, 101, 114, 116, 121, 117, 105, 111, 112,  91,  93,  13, 141,  97, 115,   // 1
	        100, 102, 103, 104, 106, 107, 108,  59,  39,  96, 142,  92, 122, 120,  99, 118,   // 2
	         98, 110, 109,  44,  46,  47, 142, 183, 140,  32, 129, 149, 150, 151, 152, 153,   // 3
	        154, 155, 156, 157, 158, 132, 130, 147, 133, 146, 179, 135, 169, 136, 181, 148,   // 4
	        134, 145, 143, 144,   0,   0,   0, 159, 160,   0,   0, 137, 138, 139,   0,   0,   // 5
	          0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   // 6
	          0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   // 7
        // shifted
	          0,  27,  33,  64,  35,  36,  37,  94,  38,  42,  40,  41,  95,  43, 127,   9,   // 0
	        113, 119, 101, 114, 116, 121, 117, 105, 111, 112,  91,  93,  13, 141,  97, 115,   // 1
	        100, 102, 103, 104, 106, 107, 108,  59,  39, 126, 142,  92, 122, 120,  99, 118,   // 2
	         98, 110, 109,  44,  46,  47, 142, 183, 140,  32, 129, 149, 150, 151, 152, 153,   // 3
	        154, 155, 156, 157, 158, 132, 130, 147, 133, 146, 179, 135, 169, 136, 181, 148,   // 4
	        134, 145, 143, 144,   0,   0,   0, 159, 160,   0,   0, 137, 138, 139,   0,   0,   // 5
	          0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   // 6
	          0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   // 7
        };
        static readonly byte[] s_scantokey_german = {
        //	 0    1    2    3    4    5    6    7    8    9    A    B    C    D    E    F
	          0,  27,  49,  50,  51,  52,  53,  54,  55,  56,  57,  48,  63,  39, 127,   9,   // 0
	        113, 119, 101, 114, 116, 122, 117, 105, 111, 112,  61,  43,  13, 141,  97, 115,   // 1
	        100, 102, 103, 104, 106, 107, 108,  91,  93,  96, 142,  35, 121, 120,  99, 118,   // 2
	         98, 110, 109,  44,  46,  45, 142, 183, 140,  32, 129, 149, 150, 151, 152, 153,   // 3
	        154, 155, 156, 157, 158, 132, 130, 147, 133, 146, 179, 135, 169, 136, 181, 148,   // 4
	        134, 145, 143, 144,   0,   0,  60, 159, 160,   0,   0, 137, 138, 139,   0,   0,   // 5
	          0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   // 6
	          0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   // 7
        // shifted
	          0,  27,  49,  50,  51,  52,  53,  54,  55,  56,  57,  48,  63,  39, 127,   9,   // 0
	        113, 119, 101, 114, 116, 122, 117, 105, 111, 112,  61,  43,  13, 141,  97, 115,   // 1
	        100, 102, 103, 104, 106, 107, 108,  91,  93,  96, 142,  35, 121, 120,  99, 118,   // 2
	         98, 110, 109,  44,  46,  45, 142, 183, 140,  32, 129, 149, 150, 151, 152, 153,   // 3
	        154, 155, 156, 157, 158, 132, 130, 147, 133, 146, 179, 135, 169, 136, 181, 148,   // 4
	        134, 145, 143, 144,   0,   0,  60, 159, 160,   0,   0, 137, 138, 139,   0,   0,   // 5
	          0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   // 6
	          0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   // 7
        };
        static readonly byte[] s_scantokey_french = {
        //	 0    1    2    3    4    5    6    7    8    9    A    B    C    D    E    F
	          0,  27,  49,  50,  51,  52,  53,  54,  55,  56,  57,  48,  41,  61, 127,   9,   // 0
	         97, 122, 101, 114, 116, 121, 117, 105, 111, 112,  94,  36,  13, 141, 113, 115,   // 1
	        100, 102, 103, 104, 106, 107, 108, 109, 249,  96, 142,  42, 119, 120,  99, 118,   // 2
	         98, 110,  44,  59,  58,  33, 142, 183, 140,  32, 129, 149, 150, 151, 152, 153,   // 3
	        154, 155, 156, 157, 158, 132, 130, 147, 133, 146, 179, 135, 169, 136, 181, 148,   // 4
	        134, 145, 143, 144,   0,   0,  60, 159, 160,   0,   0, 137, 138, 139,   0,   0,   // 5
	          0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   // 6
	          0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   // 7
        // shifted
	          0,  27,  38, 233,  34,  39,  40,  45, 232,  95, 231, 224, 176,  43, 127,   9,   // 0
	         97, 122, 101, 114, 116, 121, 117, 105, 111, 112,  94,  36,  13, 141, 113, 115,   // 1
	        100, 102, 103, 104, 106, 107, 108, 109, 249,   0, 142,  42, 119, 120,  99, 118,   // 2
	         98, 110,  44,  59,  58,  33, 142, 183, 140,  32, 129, 149, 150, 151, 152, 153,   // 3
	        154, 155, 156, 157, 158, 132, 130, 147, 133, 146, 179, 135, 169, 136, 181, 148,   // 4
	        134, 145, 143, 144,   0,   0,  60, 159, 160,   0,   0, 137, 138, 139,   0,   0,   // 5
	          0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   // 6
	          0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   // 7
        };
        static readonly byte[] s_scantokey_spanish = {
        //	 0    1    2    3    4    5    6    7    8    9    A    B    C    D    E    F
	          0,  27,  49,  50,  51,  52,  53,  54,  55,  56,  57,  48,  39, 161, 127,   9,   // 0
	        113, 119, 101, 114, 116, 121, 117, 105, 111, 112,  96,  43,  13, 141,  97, 115,   // 1
	        100, 102, 103, 104, 106, 107, 108, 241, 180, 186, 142, 231, 122, 120,  99, 118,   // 2
	         98, 110, 109,  44,  46,  45, 142, 183, 140,  32, 129, 149, 150, 151, 152, 153,   // 3
	        154, 155, 156, 157, 158, 132, 130, 147, 133, 146, 179, 135, 169, 136, 181, 148,   // 4
	        134, 145, 143, 144,   0,   0,  60, 159, 160,   0,   0, 137, 138, 139,   0,   0,   // 5
	          0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   // 6
	          0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   // 7
        // shifted
	          0,  27,  33,  34, 183,  36,  37,  38,  47,  40,  41,  61,  63, 191, 127,   9,   // 0
	        113, 119, 101, 114, 116, 121, 117, 105, 111, 112,  94,  42,  13, 141,  97, 115,   // 1
	        100, 102, 103, 104, 106, 107, 108, 209, 168, 170, 142, 199, 122, 120,  99, 118,   // 2
	         98, 110, 109,  44,  46,  45, 142, 183, 140,  32, 129, 149, 150, 151, 152, 153,   // 3
	        154, 155, 156, 157, 158, 132, 130, 147, 133, 146, 179, 135, 169, 136, 181, 148,   // 4
	        134, 145, 143, 144,   0,   0,  60, 159, 160,   0,   0, 137, 138, 139,   0,   0,   // 5
	          0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   // 6
	          0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   // 7
        };
        static readonly byte[] s_scantokey_italian = {
        //	 0    1    2    3    4    5    6    7    8    9    A    B    C    D    E    F
	          0,  27,  49,  50,  51,  52,  53,  54,  55,  56,  57,  48,  39, 236, 127,   9,   // 0
	        113, 119, 101, 114, 116, 121, 117, 105, 111, 112, 232,  43,  13, 141,  97, 115,   // 1
	        100, 102, 103, 104, 106, 107, 108, 242, 224,  92, 142, 249, 122, 120,  99, 118,   // 2
	         98, 110, 109,  44,  46,  45, 142, 183, 140,  32, 129, 149, 150, 151, 152, 153,   // 3
	        154, 155, 156, 157, 158, 132, 130, 147, 133, 146, 179, 135, 169, 136, 181, 148,   // 4
	        134, 145, 143, 144,   0,   0,  60, 159, 160,   0,   0, 137, 138, 139,   0,   0,   // 5
	          0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   // 6
	          0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   // 7
        // shifted
	          0,  27,  33,  34, 163,  36,  37,  38,  47,  40,  41,  61,  63,  94, 127,   9,   // 0
	        113, 119, 101, 114, 116, 121, 117, 105, 111, 112, 233,  42,  13, 141,  97, 115,   // 1
	        100, 102, 103, 104, 106, 107, 108, 231, 176, 124, 142, 167, 122, 120,  99, 118,   // 2
	         98, 110, 109,  44,  46,  45, 142, 183, 140,  32, 129, 149, 150, 151, 152, 153,   // 3
	        154, 155, 156, 157, 158, 132, 130, 147, 133, 146, 179, 135, 169, 136, 181, 148,   // 4
	        134, 145, 143, 144,   0,   0,  60, 159, 160,   0,   0, 137, 138, 139,   0,   0,   // 5
	          0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   // 6
	          0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   // 7
        };

        static byte[] keyScanTable = s_scantokey;

        // this should be part of the scantables and the scan tables should be 512 bytes (256 scan codes, shifted and unshifted).  Changing everything to use 512 byte
        // scan tables now might introduce bugs in tested code.  Since we only need to fix the right-alt case for non-US keyboards, we're just using a special-case table
        // for it.  Eventually, the tables above should be fixed to handle all possible scan codes instead of just the first 128.
        static Key rightAltKey = K_ALT;

        public static void InitScanTable()
        {
            var lang = cvarSystem.GetCVarString("sys_lang");
            if (lang.Length == 0) lang = "english";
            // the only reason that english right alt binds as K_ALT is so that users who were using right-alt before the patch don't suddenly find that only left-alt is working.
            if (string.Equals(lang, "english", StringComparison.OrdinalIgnoreCase)) { keyScanTable = s_scantokey; rightAltKey = K_ALT; }
            else if (string.Equals(lang, "spanish", StringComparison.OrdinalIgnoreCase)) { keyScanTable = s_scantokey_spanish; rightAltKey = K_RIGHT_ALT; }
            else if (string.Equals(lang, "french", StringComparison.OrdinalIgnoreCase)) { keyScanTable = s_scantokey_french; rightAltKey = K_RIGHT_ALT; }
            else if (string.Equals(lang, "german", StringComparison.OrdinalIgnoreCase)) { keyScanTable = s_scantokey_german; rightAltKey = K_RIGHT_ALT; }
            else if (string.Equals(lang, "italian", StringComparison.OrdinalIgnoreCase)) { keyScanTable = s_scantokey_italian; rightAltKey = K_RIGHT_ALT; }
        }

        public static byte[] Win_GetScanTable()
            => keyScanTable;

        // Map from windows to Doom keynums
        public static Key Win_MapKey(int key)
        {
            var modified = (key >> 16) & 255;
            if (modified > 127) return 0;

            // Check for certain extended character codes. The specific case we are testing is the numpad / is not being translated properly for localized builds.
            var is_extended = ((key & (1 << 24)) != 0);
            if (is_extended)
                switch (modified)
                {
                    case 0x35: return K_KP_SLASH; // Numpad /
                }

            var scanToKey = Win_GetScanTable();
            var result = (Key)scanToKey[modified];

            //Printf("Key: 0x%08x Modified: 0x%02x Extended: %s Result: 0x%02x\n", key, modified, (is_extended ? "Y" : "N"), result);

            if (is_extended)
                switch (result)
                {
                    case K_PAUSE: return K_KP_NUMLOCK;
                    case (Key)0x0D: return K_KP_ENTER;
                    case (Key)0x2F: return K_KP_SLASH;
                    case (Key)0xAF: return K_KP_PLUS;
                    case K_KP_STAR: return K_PRINT_SCR;
                    case K_ALT: return K_RIGHT_ALT;
                }
            else
                switch (result)
                {
                    case K_HOME: return K_KP_HOME;
                    case K_UPARROW: return K_KP_UPARROW;
                    case K_PGUP: return K_KP_PGUP;
                    case K_LEFTARROW: return K_KP_LEFTARROW;
                    case K_RIGHTARROW: return K_KP_RIGHTARROW;
                    case K_END: return K_KP_END;
                    case K_DOWNARROW: return K_KP_DOWNARROW;
                    case K_PGDN: return K_KP_PGDN;
                    case K_INS: return K_KP_INS;
                    case K_DEL: return K_KP_DEL;
                }

            return result;
        }
    }
}