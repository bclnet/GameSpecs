using GameX.WbB.Formats.Entity;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.FileTypes
{
    [PakFileType(PakFileType.SecondaryAttributeTable)]
    public class SecondaryAttributeTable : FileType, IHaveMetaInfo
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
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(SecondaryAttributeTable)}: {Id:X8}", items: new List<MetaInfo> {
                    new MetaInfo("Health", items: (MaxHealth.Formula as IHaveMetaInfo).GetInfoNodes(tag: tag)),
                    new MetaInfo("Stamina", items: (MaxStamina.Formula as IHaveMetaInfo).GetInfoNodes(tag: tag)),
                    new MetaInfo("Mana", items: (MaxMana.Formula as IHaveMetaInfo).GetInfoNodes(tag: tag)),
                })
            };
            return nodes;
        }
    }
}
