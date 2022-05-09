using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    // TODO: refactor to use existing PaletteOverride object
    public class SubPalette : IGetMetadataInfo
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
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"SubID: {SubID:X8}", clickable: true),
                new MetadataInfo($"Offset: {Offset}"),
                new MetadataInfo($"NumColors: {NumColors}"),
            };
            return nodes;
        }
    }
}
