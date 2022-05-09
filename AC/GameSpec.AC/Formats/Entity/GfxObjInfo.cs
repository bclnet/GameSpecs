using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class GfxObjInfo : IGetMetadataInfo
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
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"ID: {Id:X8}", clickable: true),
                new MetadataInfo($"DegradeMode: {DegradeMode}"),
                new MetadataInfo($"MinDist: {MinDist}"),
                new MetadataInfo($"IdealDist: {IdealDist}"),
                new MetadataInfo($"MaxDist: {MaxDist}"),
            };
            return nodes;
        }
    }
}
