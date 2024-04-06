using GameX.WbB.Formats.Entity;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.FileTypes
{
    [PakFileType(PakFileType.NameFilterTable)]
    public class NameFilterTable : FileType, IHaveMetaInfo
    {
        public const uint FILE_ID = 0x0E000020;

        // Key is a list of a WCIDs that are "bad" and should not exist. The value is always 1 (could be a bool?)
        public readonly IDictionary<uint, NameFilterLanguageData> LanguageData;

        public NameFilterTable(BinaryReader r)
        {
            Id = r.ReadUInt32();
            LanguageData = r.Skip(1).ReadL8TMany<uint, NameFilterLanguageData>(sizeof(uint), x => new NameFilterLanguageData(x));
        }

        //: FileTypes.GeneratorTable
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(NameFilterTable)}: {Id:X8}", items: LanguageData.Select(
                    x => new MetaInfo($"{x.Key}", items: (x.Value as IHaveMetaInfo).GetInfoNodes(tag: tag))
                ))
            };
            return nodes;
        }
    }
}
