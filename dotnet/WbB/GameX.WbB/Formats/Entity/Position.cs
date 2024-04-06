using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace GameX.WbB.Formats.Entity
{
    /// <summary>
    /// Position consists of a CellID + a Frame (Origin + Orientation)
    /// </summary>
    public class Position : IHaveMetaInfo
    {
        public readonly uint ObjCellID;
        public readonly Frame Frame;

        public Position(BinaryReader r)
        {
            ObjCellID = r.ReadUInt32();
            Frame = new Frame(r);
        }

        //: Entity.Position
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                ObjCellID != 0 ? new MetaInfo($"ObjCell ID: {ObjCellID:X8}", clickable: true) : null,
                !Frame.Origin.IsZeroEpsilon() ? new MetaInfo($"Origin: {Frame.Origin}") : null,
                !Frame.Orientation.IsIdentity ? new MetaInfo($"Orientation: {Frame.Orientation}") : null,
            };
            return nodes;
        }
    }
}
