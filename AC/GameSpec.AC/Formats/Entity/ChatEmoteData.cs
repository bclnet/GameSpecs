using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameSpec.AC.Formats.Entity
{
    public class ChatEmoteData : IGetExplorerInfo
    {
        public readonly string MyEmote; // What the emote string is to the character doing the emote
        public readonly string OtherEmote; // What the emote string is to other characters

        public ChatEmoteData(BinaryReader r)
        {
            MyEmote = r.ReadL16String(Encoding.Default); r.AlignBoundary();
            OtherEmote = r.ReadL16String(Encoding.Default); r.AlignBoundary();
        }

        //: Entity.ChatEmoteData
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"MyEmote: {MyEmote}"),
                new ExplorerInfoNode($"OtherEmote: {OtherEmote}"),
            };
            return nodes;
        }
    }
}
