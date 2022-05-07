using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class GfxObjInfo : IGetExplorerInfo
    {
        public readonly uint Id;
        public readonly uint DegradeMode;
        public readonly float MinDist;
        public readonly float IdealDist;
        public readonly float MaxDist;

        public GfxObjInfo(BinaryReader r)
        {
            Id = r.ReadUInt32();
            DegradeMode = r.ReadUInt32();
            MinDist = r.ReadSingle();
            IdealDist = r.ReadSingle();
            MaxDist = r.ReadSingle();
        }

        //: Entity.GfxObjInfo
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"ID: {Id:X8}", clickable: true),
                new ExplorerInfoNode($"DegradeMode: {DegradeMode}"),
                new ExplorerInfoNode($"MinDist: {MinDist}"),
                new ExplorerInfoNode($"IdealDist: {IdealDist}"),
                new ExplorerInfoNode($"MaxDist: {MaxDist}"),
            };
            return nodes;
        }
    }
}
