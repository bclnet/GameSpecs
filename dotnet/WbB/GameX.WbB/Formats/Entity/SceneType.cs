using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class SceneType : IHaveMetaInfo
    {
        public uint StbIndex;
        public uint[] Scenes;

        public SceneType(BinaryReader r)
        {
            StbIndex = r.ReadUInt32();
            Scenes = r.ReadL32TArray<uint>(sizeof(uint));
        }

        //: Entity.SceneType
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"SceneTableIdx: {StbIndex}"),
                new MetaInfo("Scenes", items: Scenes.Select(x => new MetaInfo($"{x:X8}", clickable: true))),
            };
            return nodes;
        }
    }
}
