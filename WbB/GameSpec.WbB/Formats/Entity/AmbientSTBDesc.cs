using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.WbB.Formats.Entity
{
    public class AmbientSTBDesc : IGetMetadataInfo
    {
        public readonly uint STBId;
        public readonly AmbientSoundDesc[] AmbientSounds;

        public AmbientSTBDesc(BinaryReader r)
        {
            STBId = r.ReadUInt32();
            AmbientSounds = r.ReadL32Array(x => new AmbientSoundDesc(x));
        }

        //: Entity.AmbientSoundTableDesc
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"Ambient Sound Table ID: {STBId:X8}", clickable: true),
                new MetadataInfo($"Ambient Sounds", items: AmbientSounds.Select((x, i)
                    => new MetadataInfo($"{i}", items: (x as IGetMetadataInfo).GetInfoNodes()))),
            };
            return nodes;
        }
    }
}
