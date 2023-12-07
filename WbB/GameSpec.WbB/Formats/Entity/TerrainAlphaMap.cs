using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.Entity
{
    public class TerrainAlphaMap : IGetMetadataInfo
    {
        public readonly uint TCode;
        public readonly uint TexGID;

        public TerrainAlphaMap(BinaryReader r)
        {
            TCode = r.ReadUInt32();
            TexGID = r.ReadUInt32();
        }

        //: Entity.TerrainAlphaMap
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"TerrainCode: {TCode}"),
                new MetadataInfo($"TextureGID: {TexGID:X8}", clickable: true),
            };
            return nodes;
        }

        //: Entity.TerrainAlphaMap
        public override string ToString() => $"TerrainCode: {TCode}, TextureGID: {TexGID:X8}";
    }
}
