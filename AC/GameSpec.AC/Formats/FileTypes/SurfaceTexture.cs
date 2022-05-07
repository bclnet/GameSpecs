using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.FileTypes
{
    [PakFileType(PakFileType.SurfaceTexture)]
    public class SurfaceTexture : FileType, IGetExplorerInfo
    {
        public readonly int Unknown;
        public readonly byte UnknownByte;
        public readonly uint[] Textures; // These values correspond to a Surface (0x06) entry

        public SurfaceTexture(BinaryReader r)
        {
            Id = r.ReadUInt32();
            Unknown = r.ReadInt32();
            UnknownByte = r.ReadByte();
            Textures = r.ReadL32Array<uint>(sizeof(uint));
        }

        //: FileTypes.SurfaceTexture
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"{nameof(SurfaceTexture)}: {Id:X8}", items: new List<ExplorerInfoNode> {
                    new ExplorerInfoNode($"Unknown: {Unknown}"),
                    new ExplorerInfoNode($"UnknownByte: {UnknownByte}"),
                    new ExplorerInfoNode("Textures", items: Textures.Select(x => new ExplorerInfoNode($"{x:X8}", clickable: true))),
                })
            };
            return nodes;
        }
    }
}
