using GameSpec.AC.Formats.Entity;
using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.FileTypes
{
    [PakFileType(PakFileType.StringTable)]
    public class StringTable : FileType, IGetExplorerInfo
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
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"{nameof(StringTable)}: {Id:X8}", items: new List<ExplorerInfoNode> {
                    new ExplorerInfoNode($"Language: {Language}"),
                    new ExplorerInfoNode($"Unknown: {Unknown}"),
                    new ExplorerInfoNode("String Tables", items: StringTableData.Select(x => {
                        var items = (x as IGetExplorerInfo).GetInfoNodes();
                        var name = items[0].Name;
                        items.RemoveAt(0);
                        return new ExplorerInfoNode(name, items: items);
                    })),
                })
            };
            return nodes;
        }
    }
}
