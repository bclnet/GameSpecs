using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class TerrainTex : IHaveMetaInfo
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
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"TexGID: {TexGID:X8}", clickable: true),
                new MetaInfo($"TexTiling: {TexTiling}"),
                new MetaInfo($"MaxVertBrightness: {MaxVertBright}"),
                new MetaInfo($"MinVertBrightness: {MinVertBright}"),
                new MetaInfo($"MaxVertSaturate: {MaxVertSaturate}"),
                new MetaInfo($"MinVertSaturate: {MinVertSaturate}"),
                new MetaInfo($"MaxVertHue: {MaxVertHue}"),
                new MetaInfo($"MinVertHue: {MinVertHue}"),
                new MetaInfo($"DetailTexTiling: {DetailTexTiling}"),
                new MetaInfo($"DetailTexGID: {DetailTexGID:X8}", clickable: true),
            };
            return nodes;
        }
    }
}
