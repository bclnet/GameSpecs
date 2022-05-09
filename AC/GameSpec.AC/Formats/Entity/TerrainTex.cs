using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class TerrainTex : IGetMetadataInfo
    {
        public readonly uint TexGID;
        public readonly uint TexTiling;
        public readonly uint MaxVertBright;
        public readonly uint MinVertBright;
        public readonly uint MaxVertSaturate;
        public readonly uint MinVertSaturate;
        public readonly uint MaxVertHue;
        public readonly uint MinVertHue;
        public readonly uint DetailTexTiling;
        public readonly uint DetailTexGID;

        public TerrainTex(BinaryReader r)
        {
            TexGID = r.ReadUInt32();
            TexTiling = r.ReadUInt32();
            MaxVertBright = r.ReadUInt32();
            MinVertBright = r.ReadUInt32();
            MaxVertSaturate = r.ReadUInt32();
            MinVertSaturate = r.ReadUInt32();
            MaxVertHue = r.ReadUInt32();
            MinVertHue = r.ReadUInt32();
            DetailTexTiling = r.ReadUInt32();
            DetailTexGID = r.ReadUInt32();
        }

        //: Entity.TerrainTex
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"TexGID: {TexGID:X8}", clickable: true),
                new MetadataInfo($"TexTiling: {TexTiling}"),
                new MetadataInfo($"MaxVertBrightness: {MaxVertBright}"),
                new MetadataInfo($"MinVertBrightness: {MinVertBright}"),
                new MetadataInfo($"MaxVertSaturate: {MaxVertSaturate}"),
                new MetadataInfo($"MinVertSaturate: {MinVertSaturate}"),
                new MetadataInfo($"MaxVertHue: {MaxVertHue}"),
                new MetadataInfo($"MinVertHue: {MinVertHue}"),
                new MetadataInfo($"DetailTexTiling: {DetailTexTiling}"),
                new MetadataInfo($"DetailTexGID: {DetailTexGID:X8}", clickable: true),
            };
            return nodes;
        }
    }
}
