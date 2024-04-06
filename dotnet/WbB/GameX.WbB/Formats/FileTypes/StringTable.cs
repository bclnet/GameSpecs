using GameX.WbB.Formats.Entity;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.FileTypes
{
    [PakFileType(PakFileType.StringTable)]
    public class StringTable : FileType, IHaveMetaInfo
    {
        public static uint CharacterTitle_FileID = 0x2300000E;

        public readonly uint Language; // This should always be 1 for English
        public readonly byte Unknown;
        public readonly StringTableData[] StringTableData;

        public StringTable(BinaryReader r)
        {
            Id = r.ReadUInt32();
            Language = r.ReadUInt32();
            Unknown = r.ReadByte();
            StringTableData = r.ReadC32FArray(x => new StringTableData(x));
        }

        //: FileTypes.StringTable
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(StringTable)}: {Id:X8}", items: new List<MetaInfo> {
                    new MetaInfo($"Language: {Language}"),
                    new MetaInfo($"Unknown: {Unknown}"),
                    new MetaInfo("String Tables", items: StringTableData.Select(x => {
                        var items = (x as IHaveMetaInfo).GetInfoNodes();
                        var name = items[0].Name;
                        items.RemoveAt(0);
                        return new MetaInfo(name, items: items);
                    })),
                })
            };
            return nodes;
        }
    }
}
