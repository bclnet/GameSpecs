using GameX.Bethesda.Formats.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GameX.Bethesda.Formats
{
    partial class RecordGroup
    {
        internal HashSet<byte[]> _ensureCELLsByLabel;
        internal Dictionary<Int3, CELLRecord> CELLsById;
        internal Dictionary<Int3, LANDRecord> LANDsById;

        public RecordGroup[] EnsureWrldAndCell(Int3 cellId)
        {
            var cellBlockX = (short)(cellId.X >> 5);
            var cellBlockY = (short)(cellId.Y >> 5);
            var cellBlockId = new byte[4];
            Buffer.BlockCopy(BitConverter.GetBytes(cellBlockY), 0, cellBlockId, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(cellBlockX), 0, cellBlockId, 2, 2);
            Load();
            if (GroupsByLabel.TryGetValue(cellBlockId, out var cellBlocks))
                return cellBlocks.Select(x => x.EnsureCell(cellId)).ToArray();
            return null;
        }

        //  = nxn[nbits] + 4x4[2bits] + 8x8[3bit]
        public RecordGroup EnsureCell(Int3 cellId)
        {
            if (_ensureCELLsByLabel == null)
                _ensureCELLsByLabel = new HashSet<byte[]>(ByteArrayComparer.Default);
            var cellBlockX = (short)(cellId.X >> 5);
            var cellBlockY = (short)(cellId.Y >> 5);
            var cellSubBlockX = (short)(cellId.X >> 3);
            var cellSubBlockY = (short)(cellId.Y >> 3);
            var cellSubBlockId = new byte[4];
            Buffer.BlockCopy(BitConverter.GetBytes(cellSubBlockY), 0, cellSubBlockId, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(cellSubBlockX), 0, cellSubBlockId, 2, 2);
            if (_ensureCELLsByLabel.Contains(cellSubBlockId))
                return this;
            Load();
            if (CELLsById == null)
                CELLsById = new Dictionary<Int3, CELLRecord>();
            if (LANDsById == null && cellId.Z >= 0)
                LANDsById = new Dictionary<Int3, LANDRecord>();
            if (GroupsByLabel.TryGetValue(cellSubBlockId, out var cellSubBlocks))
            {
                // find cell
                var cellSubBlock = cellSubBlocks.Single();
                cellSubBlock.Load(true);
                foreach (var cell in cellSubBlock.Records.Cast<CELLRecord>())
                {
                    cell.GridId = new Int3(cell.XCLC.Value.GridX, cell.XCLC.Value.GridY, !cell.IsInterior ? cellId.Z : -1);
                    CELLsById.Add(cell.GridId, cell);
                    // find children
                    if (cellSubBlock.GroupsByLabel.TryGetValue(BitConverter.GetBytes(cell.Id), out var cellChildren))
                    {
                        var cellChild = cellChildren.Single();
                        var cellTemporaryChildren = cellChild.Groups.Single(x => x.Headers.First().GroupType == Header.HeaderGroupType.CellTemporaryChildren);
                        foreach (var land in cellTemporaryChildren.Records.Cast<LANDRecord>())
                        {
                            land.GridId = new Int3(cell.XCLC.Value.GridX, cell.XCLC.Value.GridY, !cell.IsInterior ? cellId.Z : -1);
                            LANDsById.Add(land.GridId, land);
                        }
                    }
                }
                _ensureCELLsByLabel.Add(cellSubBlockId);
                return this;
            }
            return null;
        }
    }
}
