using GameSpec.AC.Formats.Entity;
using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameSpec.AC.Formats.FileTypes
{
    [PakFileType(PakFileType.ChatPoseTable)]
    public class ChatPoseTable : FileType, IGetExplorerInfo
    {
        public const uint FILE_ID = 0x0E000007;

        // Key is a emote command, value is the state you are enter into
        public readonly Dictionary<string, string> ChatPoseHash;
        // Key is the state, value are the strings that players see during the emote
        public readonly Dictionary<string, ChatEmoteData> ChatEmoteHash;

        public ChatPoseTable(BinaryReader r)
        {
            Id = r.ReadUInt32();
            ChatPoseHash = r.ReadL16Many(x => { var v = x.ReadL16String(Encoding.Default); x.AlignBoundary(); return v; }, x => { var v = x.ReadL16String(Encoding.Default); x.AlignBoundary(); return v; }, offset: 2);
            ChatEmoteHash = r.ReadL16Many(x => { var v = x.ReadL16String(Encoding.Default); x.AlignBoundary(); return v; }, x => new ChatEmoteData(x), offset: 2);
        }

        //: FileTypes.ChatPoseTable
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"{nameof(ChatPoseTable)}: {Id:X8}", items: new List<ExplorerInfoNode> {
                    new ExplorerInfoNode("ChatPoseHash", items: ChatPoseHash.OrderBy(i => i.Key).Select(x => new ExplorerInfoNode($"{x.Key}: {x.Value}"))),
                    new ExplorerInfoNode("ChatEmoteHash", items: ChatEmoteHash.OrderBy(i => i.Key).Select(x => new ExplorerInfoNode($"{x.Key}", items: (x.Value as IGetExplorerInfo).GetInfoNodes(tag: tag)))),
                })
            };
            return nodes;
        }
    }
}
