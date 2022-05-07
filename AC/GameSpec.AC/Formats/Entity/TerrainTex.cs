using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class TerrainTex : IGetExplorerInfo
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
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"TexGID: {TexGID:X8}", clickable: true),
                new ExplorerInfoNode($"TexTiling: {TexTiling}"),
                new ExplorerInfoNode($"MaxVertBrightness: {MaxVertBright}"),
                new ExplorerInfoNode($"MinVertBrightness: {MinVertBright}"),
                new ExplorerInfoNode($"MaxVertSaturate: {MaxVertSaturate}"),
                new ExplorerInfoNode($"MinVertSaturate: {MinVertSaturate}"),
                new ExplorerInfoNode($"MaxVertHue: {MaxVertHue}"),
                new ExplorerInfoNode($"MinVertHue: {MinVertHue}"),
                new ExplorerInfoNode($"DetailTexTiling: {DetailTexTiling}"),
                new ExplorerInfoNode($"DetailTexGID: {DetailTexGID:X8}", clickable: true),
            };
            return nodes;
        }
    }
}
