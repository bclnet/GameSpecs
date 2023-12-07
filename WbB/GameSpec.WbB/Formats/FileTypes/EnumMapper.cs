using GameSpec.WbB.Formats.Props;
using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameSpec.WbB.Formats.FileTypes
{
    [PakFileType(PakFileType.EnumMapper)]
    public class EnumMapper : FileType, IGetMetadataInfo
    {
        public readonly uint BaseEnumMap; // _base_emp_did
        public readonly NumberingType NumberingType;
        public readonly Dictionary<uint, string> IdToStringMap; // _id_to_string_map

        public EnumMapper(BinaryReader r)
        {
            Id = r.ReadUInt32();
            BaseEnumMap = r.ReadUInt32();
            NumberingType = (NumberingType)r.ReadByte();
            IdToStringMap = r.ReadC32Many<uint, string>(sizeof(uint), x => x.ReadL8Encoding(Encoding.Default));
        }

        //: FileTypes.EnumMapper
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"{nameof(EnumMapper)}: {Id:X8}", items: new List<MetadataInfo> {
                    BaseEnumMap != 0 ? new MetadataInfo($"BaseEnumMap: {BaseEnumMap:X8}") : null,
                    NumberingType != NumberingType.Undefined ? new MetadataInfo($"NumberingType: {NumberingType}") : null,
                    IdToStringMap.Count > 0 ? new MetadataInfo("IdToStringMap", items: IdToStringMap.OrderBy(x => x.Key).Select(x => new MetadataInfo($"{x.Key}: {x.Value::X8}"))) : null,
                })
            };
            return nodes;
        }
    }
}
