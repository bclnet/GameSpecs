using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameX.WbB.Formats.Entity
{
    public class ChatEmoteData : IHaveMetaInfo
    {
        public readonly string MyEmote; // What the emote string is to the character doing the emote
        public readonly string OtherEmote; // What the emote string is to other characters

        public ChatEmoteData(BinaryReader r)
        {
            MyEmote = r.ReadL16Encoding(Encoding.Default); r.Align();
            OtherEmote = r.ReadL16Encoding(Encoding.Default); r.Align();
        }

        //: Entity.ChatEmoteData
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"MyEmote: {MyEmote}"),
                new MetaInfo($"OtherEmote: {OtherEmote}"),
            };
            return nodes;
        }
    }
}
