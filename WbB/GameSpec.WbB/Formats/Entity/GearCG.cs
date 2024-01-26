using GameSpec.Meta;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.Entity
{
    public class GearCG : IHaveMetaInfo
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
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Name: {Name}"),
                new MetaInfo($"Clothing Table: {ClothingTable:X8}", clickable: true),
                new MetaInfo($"Weenie Default: {WeenieDefault}"),
            };
            return nodes;
        }
    }
}
