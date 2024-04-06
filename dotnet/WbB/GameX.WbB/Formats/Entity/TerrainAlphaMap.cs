using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class TerrainAlphaMap : IHaveMetaInfo
    {
        public readonly uint TCode;
        public readonly uint TexGID;

        public TerrainAlphaMap(BinaryReader r)
        {
            TCode = r.ReadUInt32();
            TexGID = r.ReadUInt32();
        }

        //: Entity.TerrainAlphaMap
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"TerrainCode: {TCode}"),
                new MetaInfo($"TextureGID: {TexGID:X8}", clickable: true),
            };
            return nodes;
        }

        //: Entity.TerrainAlphaMap
        public override string ToString() => $"TerrainCode: {TCode}, TextureGID: {TexGID:X8}";
    }
}
