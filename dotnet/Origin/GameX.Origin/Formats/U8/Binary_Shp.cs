using GameX.Formats;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Origin.Formats.U8
{
    public unsafe class Binary_Shp
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Shp(r));

        #region Headers

        [StructLayout(LayoutKind.Sequential)]
        struct Header
        {
            public static (string, int) Struct = ("<3H", sizeof(Header));
            public ushort MaximumSizeX;     // In u8fonts.flx and u8mouse.shp, contains the maximum width of the shape's frames. In u8shapes.flx, sometimes contains the ShapeIndex.
            public ushort MaximumSizeY;     // In u8fonts.flx and u8mouse.shp, contains the maximum height of the shape's frames.
            public ushort Count;            // The number of frames in the shape.
        }

        [StructLayout(LayoutKind.Sequential)]
        struct ShapeHeader
        {
            public static (string, int) Struct = ("<3x6i", sizeof(ShapeHeader));
            public uint Offset;        // Offset from the start of the shape to the beginning of the frame in bytes.
            public readonly uint FrameOffset => Offset & 0x0fff;
            public int FrameSize;      // The size in bytes of a frame.
        }

        [StructLayout(LayoutKind.Sequential)]
        struct Shape
        {
            public static (string, int) Struct = ("<2HI5H", sizeof(Shape));
            public ushort ShapeIndex;       // Always either the zero-based index of the shape in its archive or zero. Seems meaningless either way.
            public ushort FrameIndex;       // Always either the zero-based index of the frame in its shape or zero. Seems meaningless either way.
            public uint Reserved;
            public ushort Compression;      // Either 0 or 1; this has relevance to the RLE data below.
            public ushort SizeX;            // Horizontal size of the frame in pixels.
            public ushort SizeY;            // Vertical size of the frame in pixels.
            public ushort OffsetX;          // Horizontal offset of the hot spot of the frame in pixels.
            public ushort OffsetY;          // Vertical offset of the hot spot of the frame in pixels.
        }

        #endregion

        public Binary_Shp(BinaryReader r)
        {
            var header = r.ReadS<Header>();
            var shapes = r.ReadSArray<ShapeHeader>(header.Count)
                .Select(s => r.Seek(s.FrameOffset).ReadS<Shape>())
                .ToArray();
        }
    }
}
