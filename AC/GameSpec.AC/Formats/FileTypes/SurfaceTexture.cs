using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.FileTypes
{
    [PakFileType(PakFileType.SurfaceTexture)]
    public class SurfaceTexture : FileType, IGetMetadataInfo
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
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"{nameof(SurfaceTexture)}: {Id:X8}", items: new List<MetadataInfo> {
                    new MetadataInfo($"Unknown: {Unknown}"),
                    new MetadataInfo($"UnknownByte: {UnknownByte}"),
                    new MetadataInfo("Textures", items: Textures.Select(x => new MetadataInfo($"{x:X8}", clickable: true))),
                })
            };
            return nodes;
        }
    }
}
