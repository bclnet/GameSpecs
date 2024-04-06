using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class RoadAlphaMap : IHaveMetaInfo
    {
        public readonly uint RCode;
        public readonly uint RoadTexGID;

        public RoadAlphaMap(BinaryReader r)
        {
            RCode = r.ReadUInt32();
            RoadTexGID = r.ReadUInt32();
        }
        
        //: Entity.RoadAlphaMap
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"RoadCode: {RCode}"),
                new MetaInfo($"RoadTexGID: {RoadTexGID:X8}", clickable: true),
            };
            return nodes;
        }

        //: Entity.RoadAlphaMap
        public override string ToString() => $"RoadCode: {RCode}, RoadTexGID: {RoadTexGID:X8}";
    }
}
