using GameSpec.WbB.Formats.Entity;
using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.FileTypes
{
    [PakFileType(PakFileType.SecondaryAttributeTable)]
    public class SecondaryAttributeTable : FileType, IGetMetadataInfo
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
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"{nameof(SecondaryAttributeTable)}: {Id:X8}", items: new List<MetadataInfo> {
                    new MetadataInfo("Health", items: (MaxHealth.Formula as IGetMetadataInfo).GetInfoNodes(tag: tag)),
                    new MetadataInfo("Stamina", items: (MaxStamina.Formula as IGetMetadataInfo).GetInfoNodes(tag: tag)),
                    new MetadataInfo("Mana", items: (MaxMana.Formula as IGetMetadataInfo).GetInfoNodes(tag: tag)),
                })
            };
            return nodes;
        }
    }
}
