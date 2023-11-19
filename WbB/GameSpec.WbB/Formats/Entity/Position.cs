using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace GameSpec.WbB.Formats.Entity
{
    /// <summary>
    /// Position consists of a CellID + a Frame (Origin + Orientation)
    /// </summary>
    public class Position : IGetMetadataInfo
    {
        public readonly uint ObjCellID;
        public readonly Frame Frame;

        public Position(BinaryReader r)
        {
            ObjCellID = r.ReadUInt32();
            Frame = new Frame(r);
        }

        //: Entity.Position
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                ObjCellID != 0 ? new MetadataInfo($"ObjCell ID: {ObjCellID:X8}", clickable: true) : null,
                !Frame.Origin.IsZeroEpsilon() ? new MetadataInfo($"Origin: {Frame.Origin}") : null,
                !Frame.Orientation.IsIdentity ? new MetadataInfo($"Orientation: {Frame.Orientation}") : null,
            };
            return nodes;
        }
    }
}
