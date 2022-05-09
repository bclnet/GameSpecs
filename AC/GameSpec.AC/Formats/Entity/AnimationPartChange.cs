using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    //: Entity+AnimationPartChange
    public class AnimationPartChange : IGetMetadataInfo
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
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"Part Idx: {PartIndex}"),
                new MetadataInfo($"Part ID: {PartID:X8}", clickable: true),
            };
            return nodes;
        }

        //: Entity.AnimPartChange
        public override string ToString() => $"PartIdx: {PartIndex}, PartID: {PartID:X8}";
    }
}
