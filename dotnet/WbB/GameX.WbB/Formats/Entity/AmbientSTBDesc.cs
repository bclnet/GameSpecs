using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class AmbientSTBDesc : IHaveMetaInfo
    {
        public readonly uint STBId;
        public readonly AmbientSoundDesc[] AmbientSounds;

        public AmbientSTBDesc(BinaryReader r)
        {
            STBId = r.ReadUInt32();
            AmbientSounds = r.ReadL32FArray(x => new AmbientSoundDesc(x));
        }

        //: Entity.AmbientSoundTableDesc
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Ambient Sound Table ID: {STBId:X8}", clickable: true),
                new MetaInfo($"Ambient Sounds", items: AmbientSounds.Select((x, i)
                    => new MetaInfo($"{i}", items: (x as IHaveMetaInfo).GetInfoNodes()))),
            };
            return nodes;
        }
    }
}
