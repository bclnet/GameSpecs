using GameX.Formats;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Origin.Formats.U8
{
    public unsafe class Binary_Pal
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Pal(r));

        #region Headers

        [StructLayout(LayoutKind.Sequential)]
        struct COLOR
        {
            public static (string, int) Struct = ("<3x", sizeof(COLOR));
            public byte R;
            public byte G;
            public byte B;
        }

        #endregion

        COLOR[] Pal;

        public Binary_Pal(BinaryReader r)
        {
            Pal = r.ReadSArray<COLOR>(256);
        }
    }
}
