using GameSpec.WbB.Formats.Props;
using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.Entity
{
    public class CellPortal : IGetMetadataInfo
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
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                Flags != 0 ? new MetadataInfo($"Flags: {Flags}") : null,
                new MetadataInfo($"Polygon ID: {PolygonId}"),
                OtherCellId != 0 ? new MetadataInfo($"OtherCell ID: {OtherCellId:X}") : null,
                OtherPortalId != 0 ? new MetadataInfo($"OtherPortal ID: {OtherPortalId:X}") : null,
            };
            return nodes;
        }
    }
}
