using GameX.WbB.Formats.Entity;
using GameX.WbB.Formats.Props;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x03. 
    /// Special thanks to Dan Skorupski for his work on Bael'Zharon's Respite, which helped fill in some of the gaps https://github.com/boardwalk/bzr
    /// </summary>
    [PakFileType(PakFileType.Animation)]
    public class Animation : FileType, IHaveMetaInfo
    {
        public readonly AnimationFlags Flags;
        public readonly uint NumParts;
        public readonly uint NumFrames;
        public readonly Frame[] PosFrames;
        public readonly AnimationFrame[] PartFrames;

        public Animation(BinaryReader r)
        {
            Id = r.ReadUInt32();
            Flags = (AnimationFlags)r.ReadUInt32();
            NumParts = r.ReadUInt32();
            NumFrames = r.ReadUInt32();
            if ((Flags & AnimationFlags.PosFrames) != 0) PosFrames = r.ReadFArray(x => new Frame(x), (int)NumFrames);
            PartFrames = r.ReadFArray(x => new AnimationFrame(x, NumParts), (int)NumFrames);
        }

        //: FileTypes.Animation
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(Animation)}: {Id:X8}", items: new List<MetaInfo> {
                    Flags.HasFlag(AnimationFlags.PosFrames) ? new MetaInfo($"PosFrames", items: PosFrames.Select(x => new MetaInfo($"{x}"))) : null,
                    new MetaInfo($"PartFrames", items: PartFrames.Select(x => new MetaInfo($"{x}")))
                })
            };
            return nodes;
        }
    }
}
