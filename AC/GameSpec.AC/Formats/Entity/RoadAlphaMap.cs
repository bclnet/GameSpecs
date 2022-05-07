using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class RoadAlphaMap : IGetExplorerInfo
    {
        public readonly uint RCode;
        public readonly uint RoadTexGID;

        public RoadAlphaMap(BinaryReader r)
        {
            RCode = r.ReadUInt32();
            RoadTexGID = r.ReadUInt32();
        }
        
        //: Entity.RoadAlphaMap
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"RoadCode: {RCode}"),
                new ExplorerInfoNode($"RoadTexGID: {RoadTexGID:X8}", clickable: true),
            };
            return nodes;
        }

        //: Entity.RoadAlphaMap
        public override string ToString() => $"RoadCode: {RCode}, RoadTexGID: {RoadTexGID:X8}";
    }
}
