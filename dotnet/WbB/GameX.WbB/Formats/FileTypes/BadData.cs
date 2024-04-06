using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.FileTypes
{
    [PakFileType(PakFileType.BadData)]
    public class BadData : FileType, IHaveMetaInfo
    {
        public const uint FILE_ID = 0x0E00001A;

        // Key is a list of a WCIDs that are "bad" and should not exist. The value is always 1 (could be a bool?)
        public readonly IDictionary<uint, uint> Bad;

        public BadData(BinaryReader r)
        {
            Id = r.ReadUInt32();
            Bad = r.Skip(2).ReadL16TMany<uint, uint>(sizeof(uint), x => x.ReadUInt32());
        }

        //: FileTypes.BadData
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = "Bad Data", Value = string.Join(", ", Bad.Keys.OrderBy(x => x)) }),
                new MetaInfo($"{nameof(TabooTable)}: {Id:X8}")
            };
            return nodes;
        }
    }
}
