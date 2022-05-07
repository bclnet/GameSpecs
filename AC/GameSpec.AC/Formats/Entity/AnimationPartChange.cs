using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    //: Entity+AnimationPartChange
    public class AnimationPartChange : IGetExplorerInfo
    {
        public readonly byte PartIndex;
        public readonly uint PartID;

        public AnimationPartChange(BinaryReader r)
        {
            PartIndex = r.ReadByte();
            PartID = r.ReadAsDataIDOfKnownType(0x01000000);
        }
        public AnimationPartChange(BinaryReader r, ushort partIndex)
        {
            PartIndex = (byte)(partIndex & 255);
            PartID = r.ReadAsDataIDOfKnownType(0x01000000);
        }

        //: Entity.AnimPartChange
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"Part Idx: {PartIndex}"),
                new ExplorerInfoNode($"Part ID: {PartID:X8}", clickable: true),
            };
            return nodes;
        }

        //: Entity.AnimPartChange
        public override string ToString() => $"PartIdx: {PartIndex}, PartID: {PartID:X8}";
    }
}
