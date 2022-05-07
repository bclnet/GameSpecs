using GameSpec.AC.Formats.Entity;
using GameSpec.AC.Formats.Props;
using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x03. 
    /// Special thanks to Dan Skorupski for his work on Bael'Zharon's Respite, which helped fill in some of the gaps https://github.com/boardwalk/bzr
    /// </summary>
    [PakFileType(PakFileType.Animation)]
    public class Animation : FileType, IGetExplorerInfo
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
            if ((Flags & AnimationFlags.PosFrames) != 0) PosFrames = r.ReadTArray(x => new Frame(x), (int)NumFrames);
            PartFrames = r.ReadTArray(x => new AnimationFrame(x, NumParts), (int)NumFrames);
        }

        //: FileTypes.Animation
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"{nameof(Animation)}: {Id:X8}", items: new List<ExplorerInfoNode> {
                    Flags.HasFlag(AnimationFlags.PosFrames) ? new ExplorerInfoNode($"PosFrames", items: PosFrames.Select(x => new ExplorerInfoNode($"{x}"))) : null,
                    new ExplorerInfoNode($"PartFrames", items: PartFrames.Select(x => new ExplorerInfoNode($"{x}")))
                })
            };
            return nodes;
        }
    }
}
