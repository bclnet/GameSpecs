using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class TerrainAlphaMap : IGetExplorerInfo
    {
        public readonly uint TCode;
        public readonly uint TexGID;

        public TerrainAlphaMap(BinaryReader r)
        {
            TCode = r.ReadUInt32();
            TexGID = r.ReadUInt32();
        }

        //: Entity.TerrainAlphaMap
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"TerrainCode: {TCode}"),
                new ExplorerInfoNode($"TextureGID: {TexGID:X8}", clickable: true),
            };
            return nodes;
        }

        //: Entity.TerrainAlphaMap
        public override string ToString() => $"TerrainCode: {TCode}, TextureGID: {TexGID:X8}";
    }
}
