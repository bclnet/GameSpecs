using GameSpec.AC.Formats.Entity;
using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.FileTypes
{
    [PakFileType(PakFileType.SecondaryAttributeTable)]
    public class SecondaryAttributeTable : FileType, IGetExplorerInfo
    {
        public const uint FILE_ID = 0x0E000003;

        public readonly Attribute2ndBase MaxHealth;
        public readonly Attribute2ndBase MaxStamina;
        public readonly Attribute2ndBase MaxMana;

        public SecondaryAttributeTable(BinaryReader r)
        {
            Id = r.ReadUInt32();
            MaxHealth = new Attribute2ndBase(r);
            MaxStamina = new Attribute2ndBase(r);
            MaxMana = new Attribute2ndBase(r);
        }

        //: FileTypes.SecondaryAttributeTable
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"{nameof(SecondaryAttributeTable)}: {Id:X8}", items: new List<ExplorerInfoNode> {
                    new ExplorerInfoNode("Health", items: (MaxHealth.Formula as IGetExplorerInfo).GetInfoNodes(tag: tag)),
                    new ExplorerInfoNode("Stamina", items: (MaxStamina.Formula as IGetExplorerInfo).GetInfoNodes(tag: tag)),
                    new ExplorerInfoNode("Mana", items: (MaxMana.Formula as IGetExplorerInfo).GetInfoNodes(tag: tag)),
                })
            };
            return nodes;
        }
    }
}
