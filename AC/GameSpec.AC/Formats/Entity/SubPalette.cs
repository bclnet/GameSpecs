using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    // TODO: refactor to use existing PaletteOverride object
    public class SubPalette : IGetExplorerInfo
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
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"SubID: {SubID:X8}", clickable: true),
                new ExplorerInfoNode($"Offset: {Offset}"),
                new ExplorerInfoNode($"NumColors: {NumColors}"),
            };
            return nodes;
        }
    }
}
