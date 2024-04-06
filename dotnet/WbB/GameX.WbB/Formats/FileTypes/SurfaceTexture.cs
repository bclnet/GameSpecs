using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.FileTypes
{
    [PakFileType(PakFileType.SurfaceTexture)]
    public class SurfaceTexture : FileType, IHaveMetaInfo
    {
        public readonly int Unknown;
        public readonly byte UnknownByte;
        public readonly uint[] Textures; // These values correspond to a Surface (0x06) entry

        public SurfaceTexture(BinaryReader r)
        {
            Id = r.ReadUInt32();
            Unknown = r.ReadInt32();
            UnknownByte = r.ReadByte();
            Textures = r.ReadL32TArray<uint>(sizeof(uint));
        }

        //: FileTypes.SurfaceTexture
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(SurfaceTexture)}: {Id:X8}", items: new List<MetaInfo> {
                    new MetaInfo($"Unknown: {Unknown}"),
                    new MetaInfo($"UnknownByte: {UnknownByte}"),
                    new MetaInfo("Textures", items: Textures.Select(x => new MetaInfo($"{x:X8}", clickable: true))),
                })
            };
            return nodes;
        }
    }
}
