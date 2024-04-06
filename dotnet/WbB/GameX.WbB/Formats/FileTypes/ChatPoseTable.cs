using GameX.WbB.Formats.Entity;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameX.WbB.Formats.FileTypes
{
    [PakFileType(PakFileType.ChatPoseTable)]
    public class ChatPoseTable : FileType, IHaveMetaInfo
    {
        public const uint FILE_ID = 0x0E000007;

        // Key is a emote command, value is the state you are enter into
        public readonly IDictionary<string, string> ChatPoseHash;
        // Key is the state, value are the strings that players see during the emote
        public readonly IDictionary<string, ChatEmoteData> ChatEmoteHash;

        public ChatPoseTable(BinaryReader r)
        {
            Id = r.ReadUInt32();
            ChatPoseHash = r.Skip(2).ReadL16FMany(x => { var v = x.ReadL16Encoding(Encoding.Default); x.Align(); return v; }, x => { var v = x.ReadL16Encoding(Encoding.Default); x.Align(); return v; });
            ChatEmoteHash = r.Skip(2).ReadL16FMany(x => { var v = x.ReadL16Encoding(Encoding.Default); x.Align(); return v; }, x => new ChatEmoteData(x));
        }

        //: FileTypes.ChatPoseTable
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(ChatPoseTable)}: {Id:X8}", items: new List<MetaInfo> {
                    new MetaInfo("ChatPoseHash", items: ChatPoseHash.OrderBy(i => i.Key).Select(x => new MetaInfo($"{x.Key}: {x.Value}"))),
                    new MetaInfo("ChatEmoteHash", items: ChatEmoteHash.OrderBy(i => i.Key).Select(x => new MetaInfo($"{x.Key}", items: (x.Value as IHaveMetaInfo).GetInfoNodes(tag: tag)))),
                })
            };
            return nodes;
        }
    }
}
