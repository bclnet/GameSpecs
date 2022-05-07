using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class BuildInfo : IGetExplorerInfo
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
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"Model ID: {ModelId:X8}", clickable: true),
                new ExplorerInfoNode($"Frame: {Frame}"),
                new ExplorerInfoNode($"NumLeaves: {NumLeaves}"),
                new ExplorerInfoNode($"Portals", items: Portals.Select((x, i) => new ExplorerInfoNode($"{i}", items: (x as IGetExplorerInfo).GetInfoNodes()))),
            };
            return nodes;
        }
    }
}
