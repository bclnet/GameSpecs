using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    // TODO: refactor to use existing PaletteOverride object
    public class SubPalette : IHaveMetaInfo
    {
        public uint SubID;
        public uint Offset;
        public uint NumColors;

        //: Entity+SubPalette
        public SubPalette() { }
        public SubPalette(BinaryReader r)
        {
            SubID = r.ReadAsDataIDOfKnownType(0x04000000);
            Offset = (uint)(r.ReadByte() * 8);
            NumColors = r.ReadByte();
            if (NumColors == 0) NumColors = 256;
            NumColors *= 8;
        }

        //: Entity.SubPalette
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"SubID: {SubID:X8}", clickable: true),
                new MetaInfo($"Offset: {Offset}"),
                new MetaInfo($"NumColors: {NumColors}"),
            };
            return nodes;
        }
    }
}
