using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class FontCharDesc
    {
        public readonly ushort Unicode;
        public readonly ushort OffsetX;
        public readonly ushort OffsetY;
        public readonly byte Width;
        public readonly byte Height;
        public readonly byte HorizontalOffsetBefore;
        public readonly byte HorizontalOffsetAfter;
        public readonly byte VerticalOffsetBefore;

        public FontCharDesc(BinaryReader r)
        {
            Unicode = r.ReadUInt16();
            OffsetX = r.ReadUInt16();
            OffsetY = r.ReadUInt16();
            Width = r.ReadByte();
            Height = r.ReadByte();
            HorizontalOffsetBefore = r.ReadByte();
            HorizontalOffsetAfter = r.ReadByte();
            VerticalOffsetBefore = r.ReadByte();
        }
    }
}
