using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.Entity
{
    public class GearCG : IGetMetadataInfo
    {
        public readonly string Name;
        public readonly uint ClothingTable;
        public readonly uint WeenieDefault;

        public GearCG(BinaryReader r)
        {
            Name = r.ReadString();
            ClothingTable = r.ReadUInt32();
            WeenieDefault = r.ReadUInt32();
        }

        //: Entity.GearCG
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"Name: {Name}"),
                new MetadataInfo($"Clothing Table: {ClothingTable:X8}", clickable: true),
                new MetadataInfo($"Weenie Default: {WeenieDefault}"),
            };
            return nodes;
        }
    }
}
