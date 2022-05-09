using GameSpec.AC.Formats.Entity;
using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.FileTypes
{
    [PakFileType(PakFileType.StringTable)]
    public class StringTable : FileType, IGetMetadataInfo
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
            StringTableData = r.ReadC32Array(x => new StringTableData(x));
        }

        //: FileTypes.StringTable
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"{nameof(StringTable)}: {Id:X8}", items: new List<MetadataInfo> {
                    new MetadataInfo($"Language: {Language}"),
                    new MetadataInfo($"Unknown: {Unknown}"),
                    new MetadataInfo("String Tables", items: StringTableData.Select(x => {
                        var items = (x as IGetMetadataInfo).GetInfoNodes();
                        var name = items[0].Name;
                        items.RemoveAt(0);
                        return new MetadataInfo(name, items: items);
                    })),
                })
            };
            return nodes;
        }
    }
}
