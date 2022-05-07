using GameSpec.AC.Formats.Props;
using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class CBldPortal : IGetExplorerInfo
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
            StabList = r.ReadL16Array<ushort>(sizeof(ushort)); r.AlignBoundary();
        }

        //: Entity.BldPortal
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                Flags != 0 ? new ExplorerInfoNode($"Flags: {Flags}") : null,
                OtherCellId != 0 ? new ExplorerInfoNode($"OtherCell ID: {OtherCellId:X}") : null,
                OtherPortalId != 0 ? new ExplorerInfoNode($"OtherPortal ID: {OtherPortalId:X}") : null,
            };
            return nodes;
        }
    }
}
