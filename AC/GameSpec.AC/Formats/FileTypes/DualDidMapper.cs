using GameSpec.AC.Formats.Props;
using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameSpec.AC.Formats.FileTypes
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
    public class DualDidMapper : FileType, IGetExplorerInfo
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
            ClientEnumToName = r.ReadC32Many<uint, string>(sizeof(uint), x => x.ReadL8String(Encoding.Default));
            ServerIDNumberingType = (NumberingType)r.ReadByte();
            ServerEnumToID = r.ReadC32Many<uint, uint>(sizeof(uint), x => x.ReadUInt32());
            ServerNameNumberingType = (NumberingType)r.ReadByte();
            ServerEnumToName = r.ReadC32Many<uint, string>(sizeof(uint), x => x.ReadL8String(Encoding.Default));
        }

        //: FileTypes.DualDidMapper
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"{nameof(DualDidMapper)}: {Id:X8}", items: new List<ExplorerInfoNode> {
                    ClientEnumToID.Count > 0 ? new ExplorerInfoNode($"ClientIDNumberingType: {ClientIDNumberingType}") : null,
                    ClientEnumToID.Count > 0 ? new ExplorerInfoNode("ClientEnumToID", items: ClientEnumToID.OrderBy(x => x.Key).Select(x => new ExplorerInfoNode($"{x.Key}: {x.Value::X8}"))) : null,
                    ClientEnumToName.Count > 0 ? new ExplorerInfoNode($"ClientNameNumberingType: {ClientNameNumberingType}") : null,
                    ClientEnumToName.Count > 0 ? new ExplorerInfoNode("ClientEnumToName", items: ClientEnumToName.OrderBy(x => x.Key).Select(x => new ExplorerInfoNode($"{x.Key}: {x.Value::X8}"))) : null,
                    ServerEnumToID.Count > 0 ? new ExplorerInfoNode($"ServerIDNumberingType: {ServerIDNumberingType}") : null,
                    ServerEnumToID.Count > 0 ? new ExplorerInfoNode("ServerEnumToID", items: ServerEnumToID.OrderBy(x => x.Key).Select(x => new ExplorerInfoNode($"{x.Key}: {x.Value::X8}"))) : null,
                    ServerEnumToName.Count > 0 ? new ExplorerInfoNode($"ServerNameNumberingType: {ClientIDNumberingType}") : null,
                    ServerEnumToName.Count > 0 ? new ExplorerInfoNode("ServerEnumToName", items: ServerEnumToName.OrderBy(x => x.Key).Select(x => new ExplorerInfoNode($"{x.Key}: {x.Value::X8}"))) : null,
                })
            };
            return nodes;
        }
    }
}
