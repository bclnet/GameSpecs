using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class GearCG : IGetExplorerInfo
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
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"Name: {Name}"),
                new ExplorerInfoNode($"Clothing Table: {ClothingTable:X8}", clickable: true),
                new ExplorerInfoNode($"Weenie Default: {WeenieDefault}"),
            };
            return nodes;
        }
    }
}
