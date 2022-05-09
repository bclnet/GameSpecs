using GameSpec.AC.Formats.Props;
using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameSpec.AC.Formats.FileTypes
{
    /// <summary>
    /// EnumMapper files are 0x25 in the client_portal.dat
    /// They contain, as the name implies, a map of different enumeration types to a DataID value (item that exist in a client_portal.dat file)
    /// A description of each DidMapper is in DidMapper entry 0x25000000
    /// </summary>
    [PakFileType(PakFileType.DidMapper)]
    public class DidMapper : FileType, IGetMetadataInfo
    {
        // The client/server designation is guessed based on the content in each list.
        // The keys in these two Dictionaries are common. So ClientEnumToId[key] = ClientEnumToName[key].
        public readonly NumberingType ClientIDNumberingType; // bitfield designating how the numbering is organized. Not really needed for our usage.
        public readonly Dictionary<uint, uint> ClientEnumToID; // _EnumToID
        public readonly NumberingType ClientNameNumberingType; // bitfield designating how the numbering is organized. Not really needed for our usage.
        public readonly Dictionary<uint, string> ClientEnumToName; // _EnumToName
        // The keys in these two Dictionaries are common. So ServerEnumToId[key] = ServerEnumToName[key].
        public readonly NumberingType ServerIDNumberingType; // bitfield designating how the numbering is organized. Not really needed for our usage.
        public readonly Dictionary<uint, uint> ServerEnumToID; // _EnumToID
        public readonly NumberingType ServerNameNumberingType; // bitfield designating how the numbering is organized. Not really needed for our usage.
        public readonly Dictionary<uint, string> ServerEnumToName; // _EnumToName

        public DidMapper(BinaryReader r)
        {
            Id = r.ReadUInt32();
            ClientIDNumberingType = (NumberingType)r.ReadByte();
            ClientEnumToID = r.ReadC32Many<uint, uint>(sizeof(uint), x => x.ReadUInt32());
            ClientNameNumberingType = (NumberingType)r.ReadByte();
            ClientEnumToName = r.ReadC32Many<uint, string>(sizeof(uint), x => x.ReadL8String(Encoding.Default));
            ServerIDNumberingType = (NumberingType)r.ReadByte();
            ServerEnumToID = r.ReadC32Many<uint, uint>(sizeof(uint), x => x.ReadUInt32());
            ServerNameNumberingType = (NumberingType)r.ReadByte();
            ServerEnumToName = r.ReadC32Many<uint, string>(sizeof(uint), x => x.ReadL8String(Encoding.Default));
        }

        //: FileTypes.DidMapper
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"{nameof(DidMapper)}: {Id:X8}", items: new List<MetadataInfo> {
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
