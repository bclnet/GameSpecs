using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.FileTypes
{
    [PakFileType(PakFileType.BadData)]
    public class BadData : FileType, IGetExplorerInfo
    {
        public const uint FILE_ID = 0x0E00001A;

        // Key is a list of a WCIDs that are "bad" and should not exist. The value is always 1 (could be a bool?)
        public readonly Dictionary<uint, uint> Bad;

        public BadData(BinaryReader r)
        {
            Id = r.ReadUInt32();
            Bad = r.ReadL16Many<uint, uint>(sizeof(uint), x => x.ReadUInt32(), offset: 2);
        }

        //: FileTypes.BadData
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode(null, new ExplorerContentTab { Type = "Text", Name = "Bad Data", Value = string.Join(", ", Bad.Keys.OrderBy(x => x)) }),
                new ExplorerInfoNode($"{nameof(TabooTable)}: {Id:X8}")
            };
            return nodes;
        }
    }
}
