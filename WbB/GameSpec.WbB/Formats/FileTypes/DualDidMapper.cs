using GameSpec.WbB.Formats.Props;
using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameSpec.WbB.Formats.FileTypes
{
    /// <summary>
    /// EnumMapper files are 0x27 in the client_portal.dat
    /// They contain a list of Weenie IDs and their W_Class. The client uses these for items such as tracking spell components (to know if the player has all required to cast a spell).
    ///
    /// A description of each DualDidMapper is in DidMapper entry 0x25000005 (WEENIE_CATEGORIES)
    /// 27000000 - Materials
    /// 27000001 - Gems
    /// 27000002 - SpellComponents
    /// 27000003 - ComponentPacks
    /// 27000004 - TradeNotes
    /// </summary>
    [PakFileType(PakFileType.DualDidMapper)]
    public class DualDidMapper : FileType, IGetMetadataInfo
    {
        // The client/server designation is guessed based on the content in each list.
        // The keys in these two Dictionaries are common. So ClientEnumToId[key] = ClientEnumToName[key].
        public readonly NumberingType ClientIDNumberingType; // bitfield designating how the numbering is organized. Not really needed for our usage.
        public readonly Dictionary<uint, uint> ClientEnumToID; // _EnumToID
        public readonly NumberingType ClientNameNumberingType; // bitfield designating how the numbering is organized. Not really needed for our usage.
        public readonly Dictionary<uint, string> ClientEnumToName = new Dictionary<uint, string>(); // _EnumToName
        // The keys in these two Dictionaries are common. So ServerEnumToId[key] = ServerEnumToName[key].
        public readonly NumberingType ServerIDNumberingType; // bitfield designating how the numbering is organized. Not really needed for our usage.
        public readonly Dictionary<uint, uint> ServerEnumToID; // _EnumToID
        public readonly NumberingType ServerNameNumberingType; // bitfield designating how the numbering is organized. Not really needed for our usage.
        public readonly Dictionary<uint, string> ServerEnumToName; // _EnumToName

        public DualDidMapper(BinaryReader r)
        {
            Id = r.ReadUInt32();
            ClientIDNumberingType = (NumberingType)r.ReadByte();
            ClientEnumToID = r.ReadC32Many<uint, uint>(sizeof(uint), x => x.ReadUInt32());
            ClientNameNumberingType = (NumberingType)r.ReadByte();
            ClientEnumToName = r.ReadC32Many<uint, string>(sizeof(uint), x => x.ReadL8Encoding(Encoding.Default));
            ServerIDNumberingType = (NumberingType)r.ReadByte();
            ServerEnumToID = r.ReadC32Many<uint, uint>(sizeof(uint), x => x.ReadUInt32());
            ServerNameNumberingType = (NumberingType)r.ReadByte();
            ServerEnumToName = r.ReadC32Many<uint, string>(sizeof(uint), x => x.ReadL8Encoding(Encoding.Default));
        }

        //: FileTypes.DualDidMapper
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"{nameof(DualDidMapper)}: {Id:X8}", items: new List<MetadataInfo> {
                    ClientEnumToID.Count > 0 ? new MetadataInfo($"ClientIDNumberingType: {ClientIDNumberingType}") : null,
                    ClientEnumToID.Count > 0 ? new MetadataInfo("ClientEnumToID", items: ClientEnumToID.OrderBy(x => x.Key).Select(x => new MetadataInfo($"{x.Key}: {x.Value::X8}"))) : null,
                    ClientEnumToName.Count > 0 ? new MetadataInfo($"ClientNameNumberingType: {ClientNameNumberingType}") : null,
                    ClientEnumToName.Count > 0 ? new MetadataInfo("ClientEnumToName", items: ClientEnumToName.OrderBy(x => x.Key).Select(x => new MetadataInfo($"{x.Key}: {x.Value::X8}"))) : null,
                    ServerEnumToID.Count > 0 ? new MetadataInfo($"ServerIDNumberingType: {ServerIDNumberingType}") : null,
                    ServerEnumToID.Count > 0 ? new MetadataInfo("ServerEnumToID", items: ServerEnumToID.OrderBy(x => x.Key).Select(x => new MetadataInfo($"{x.Key}: {x.Value::X8}"))) : null,
                    ServerEnumToName.Count > 0 ? new MetadataInfo($"ServerNameNumberingType: {ClientIDNumberingType}") : null,
                    ServerEnumToName.Count > 0 ? new MetadataInfo("ServerEnumToName", items: ServerEnumToName.OrderBy(x => x.Key).Select(x => new MetadataInfo($"{x.Key}: {x.Value::X8}"))) : null,
                })
            };
            return nodes;
        }
    }
}
