using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameSpec.AC.Formats.Entity
{
    public class ChatEmoteData : IGetMetadataInfo
    {
        public readonly string MyEmote; // What the emote string is to the character doing the emote
        public readonly string OtherEmote; // What the emote string is to other characters

        public ChatEmoteData(BinaryReader r)
        {
            MyEmote = r.ReadL16Encoding(Encoding.Default); r.Align();
            OtherEmote = r.ReadL16Encoding(Encoding.Default); r.Align();
        }

        //: Entity.ChatEmoteData
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"MyEmote: {MyEmote}"),
                new MetadataInfo($"OtherEmote: {OtherEmote}"),
            };
            return nodes;
        }
    }
}
