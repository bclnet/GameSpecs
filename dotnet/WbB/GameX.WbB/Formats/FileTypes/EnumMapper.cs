using GameX.WbB.Formats.Props;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameX.WbB.Formats.FileTypes
{
    [PakFileType(PakFileType.EnumMapper)]
    public class EnumMapper : FileType, IHaveMetaInfo
    {
        public readonly uint BaseEnumMap; // _base_emp_did
        public readonly NumberingType NumberingType;
        public readonly IDictionary<uint, string> IdToStringMap; // _id_to_string_map

        public EnumMapper(BinaryReader r)
        {
            Id = r.ReadUInt32();
            BaseEnumMap = r.ReadUInt32();
            NumberingType = (NumberingType)r.ReadByte();
            IdToStringMap = r.ReadC32TMany<uint, string>(sizeof(uint), x => x.ReadL8Encoding(Encoding.Default));
        }

        //: FileTypes.EnumMapper
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(EnumMapper)}: {Id:X8}", items: new List<MetaInfo> {
                    BaseEnumMap != 0 ? new MetaInfo($"BaseEnumMap: {BaseEnumMap:X8}") : null,
                    NumberingType != NumberingType.Undefined ? new MetaInfo($"NumberingType: {NumberingType}") : null,
                    IdToStringMap.Count > 0 ? new MetaInfo("IdToStringMap", items: IdToStringMap.OrderBy(x => x.Key).Select(x => new MetaInfo($"{x.Key}: {x.Value::X8}"))) : null,
                })
            };
            return nodes;
        }
    }
}
