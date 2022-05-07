using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class AmbientSTBDesc : IGetExplorerInfo
    {
        public readonly uint STBId;
        public readonly AmbientSoundDesc[] AmbientSounds;

        public AmbientSTBDesc(BinaryReader r)
        {
            STBId = r.ReadUInt32();
            AmbientSounds = r.ReadL32Array(x => new AmbientSoundDesc(x));
        }

        //: Entity.AmbientSoundTableDesc
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"Ambient Sound Table ID: {STBId:X8}", clickable: true),
                new ExplorerInfoNode($"Ambient Sounds", items: AmbientSounds.Select((x, i)
                    => new ExplorerInfoNode($"{i}", items: (x as IGetExplorerInfo).GetInfoNodes()))),
            };
            return nodes;
        }
    }
}
