using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.WbB.Formats.Entity
{
    public class BuildInfo : IGetMetadataInfo
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
            Portals = r.ReadL32Array(x => new CBldPortal(x));
        }

        //: Entity.BuildInfo
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"Model ID: {ModelId:X8}", clickable: true),
                new MetadataInfo($"Frame: {Frame}"),
                new MetadataInfo($"NumLeaves: {NumLeaves}"),
                new MetadataInfo($"Portals", items: Portals.Select((x, i) => new MetadataInfo($"{i}", items: (x as IGetMetadataInfo).GetInfoNodes()))),
            };
            return nodes;
        }
    }
}
