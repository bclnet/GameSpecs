using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class SceneType : IGetExplorerInfo
    {
        public uint StbIndex;
        public uint[] Scenes;

        public SceneType(BinaryReader r)
        {
            StbIndex = r.ReadUInt32();
            Scenes = r.ReadL32Array<uint>(sizeof(uint));
        }

        //: Entity.SceneType
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"SceneTableIdx: {StbIndex}"),
                new ExplorerInfoNode("Scenes", items: Scenes.Select(x => new ExplorerInfoNode($"{x:X8}", clickable: true))),
            };
            return nodes;
        }
    }
}
