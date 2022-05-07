using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class SceneDesc : IGetExplorerInfo
    {
        public readonly SceneType[] SceneTypes;

        public SceneDesc(BinaryReader r)
            => SceneTypes = r.ReadL32Array(x => new SceneType(x));

        //: Entity.SceneDesc
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode("SceneTypes", items: SceneTypes.Select((x, i) => new ExplorerInfoNode($"{i}", items: (x as IGetExplorerInfo).GetInfoNodes()))),
            };
            return nodes;
        }
    }
}
