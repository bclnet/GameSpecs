using GameX.WbB.Formats.Props;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class CBldPortal : IHaveMetaInfo
    {
        public readonly PortalFlags Flags;

        // Not sure what these do. They are both calculated from the flags.
        public bool ExactMatch => Flags.HasFlag(PortalFlags.ExactMatch);
        public bool PortalSide => Flags.HasFlag(PortalFlags.PortalSide);

        // Basically the cells that connect both sides of the portal
        public readonly ushort OtherCellId;
        public readonly ushort OtherPortalId;

        /// <summary>
        /// List of cells used in this structure. (Or possibly just those visible through it.)
        /// </summary>
        public readonly ushort[] StabList;

        public CBldPortal(BinaryReader r)
        {
            Flags = (PortalFlags)r.ReadUInt16();
            OtherCellId = r.ReadUInt16();
            OtherPortalId = r.ReadUInt16();
            StabList = r.ReadL16TArray<ushort>(sizeof(ushort)); r.Align();
        }

        //: Entity.BldPortal
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                Flags != 0 ? new MetaInfo($"Flags: {Flags}") : null,
                OtherCellId != 0 ? new MetaInfo($"OtherCell ID: {OtherCellId:X}") : null,
                OtherPortalId != 0 ? new MetaInfo($"OtherPortal ID: {OtherPortalId:X}") : null,
            };
            return nodes;
        }
    }
}
