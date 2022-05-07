using GameSpec.AC.Formats.Entity;
using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.FileTypes
{
    [PakFileType(PakFileType.NameFilterTable)]
    public class NameFilterTable : FileType, IGetExplorerInfo
    {
        public const uint FILE_ID = 0x0E000020;

        // Key is a list of a WCIDs that are "bad" and should not exist. The value is always 1 (could be a bool?)
        public readonly Dictionary<uint, NameFilterLanguageData> LanguageData;

        public NameFilterTable(BinaryReader r)
        {
            Id = r.ReadUInt32();
            LanguageData = r.ReadL8Many<uint, NameFilterLanguageData>(sizeof(uint), x => new NameFilterLanguageData(x), offset: 1);
        }

        //: FileTypes.GeneratorTable
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"{nameof(NameFilterTable)}: {Id:X8}", items: LanguageData.Select(
                    x => new ExplorerInfoNode($"{x.Key}", items: (x.Value as IGetExplorerInfo).GetInfoNodes(tag: tag))
                ))
            };
            return nodes;
        }
    }
}
