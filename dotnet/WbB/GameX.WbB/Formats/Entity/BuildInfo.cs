using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class BuildInfo : IHaveMetaInfo
    {
        /// <summary>
        /// 0x01 or 0x02 model of the building
        /// </summary>
        public readonly uint ModelId;
        /// <summary>
        /// specific @loc of the model
        /// </summary>
        public readonly Frame Frame;
        /// <summary>
        /// unsure what this is used for
        /// </summary>
        public readonly uint NumLeaves;
        /// <summary>
        /// portals are things like doors, windows, etc.
        /// </summary>
        public CBldPortal[] Portals;

        public BuildInfo(BinaryReader r)
        {
            ModelId = r.ReadUInt32();
            Frame = new Frame(r);
            NumLeaves = r.ReadUInt32();
            Portals = r.ReadL32FArray(x => new CBldPortal(x));
        }

        //: Entity.BuildInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Model ID: {ModelId:X8}", clickable: true),
                new MetaInfo($"Frame: {Frame}"),
                new MetaInfo($"NumLeaves: {NumLeaves}"),
                new MetaInfo($"Portals", items: Portals.Select((x, i) => new MetaInfo($"{i}", items: (x as IHaveMetaInfo).GetInfoNodes()))),
            };
            return nodes;
        }
    }
}
