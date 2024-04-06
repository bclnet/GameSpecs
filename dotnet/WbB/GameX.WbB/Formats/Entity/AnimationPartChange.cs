using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    //: Entity+AnimationPartChange
    public class AnimationPartChange : IHaveMetaInfo
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
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Part Idx: {PartIndex}"),
                new MetaInfo($"Part ID: {PartID:X8}", clickable: true),
            };
            return nodes;
        }

        //: Entity.AnimPartChange
        public override string ToString() => $"PartIdx: {PartIndex}, PartID: {PartID:X8}";
    }
}
