using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class GfxObjInfo : IHaveMetaInfo
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
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"ID: {Id:X8}", clickable: true),
                new MetaInfo($"DegradeMode: {DegradeMode}"),
                new MetaInfo($"MinDist: {MinDist}"),
                new MetaInfo($"IdealDist: {IdealDist}"),
                new MetaInfo($"MaxDist: {MaxDist}"),
            };
            return nodes;
        }
    }
}
