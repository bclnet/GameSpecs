using GameX.WbB.Formats.Props;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class CellPortal : IHaveMetaInfo
    {
        public readonly PortalFlags Flags;
        public readonly ushort PolygonId;
        public readonly ushort OtherCellId;
        public readonly ushort OtherPortalId;
        public bool ExactMatch => (Flags & PortalFlags.ExactMatch) != 0;
        public bool PortalSide => (Flags & PortalFlags.PortalSide) == 0;

        public CellPortal(BinaryReader r)
        {
            Flags = (PortalFlags)r.ReadUInt16();
            PolygonId = r.ReadUInt16();
            OtherCellId = r.ReadUInt16();
            OtherPortalId = r.ReadUInt16();
        }

        //: Entity.CellPortal
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                Flags != 0 ? new MetaInfo($"Flags: {Flags}") : null,
                new MetaInfo($"Polygon ID: {PolygonId}"),
                OtherCellId != 0 ? new MetaInfo($"OtherCell ID: {OtherCellId:X}") : null,
                OtherPortalId != 0 ? new MetaInfo($"OtherPortal ID: {OtherPortalId:X}") : null,
            };
            return nodes;
        }
    }
}
