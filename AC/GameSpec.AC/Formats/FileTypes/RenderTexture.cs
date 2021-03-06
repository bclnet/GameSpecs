using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x15.
    /// These are references to the textures for the DebugConsole
    ///
    /// This is identical to SurfaceTexture.
    ///
    /// As defined in DidMapper.UNIQUEDB (0x25000002)
    /// 0x15000000 = ConsoleOutputBackgroundTexture
    /// 0x15000001 = ConsoleInputBackgroundTexture
    /// </summary>
    [PakFileType(PakFileType.RenderTexture)]
    public class RenderTexture : FileType, IGetMetadataInfo
    {
        public readonly int Unknown;
        public readonly byte UnknownByte;
        public readonly uint[] Textures; // These values correspond to a Surface (0x06) entry

        public RenderTexture(BinaryReader r)
        {
            Id = r.ReadUInt32();
            Unknown = r.ReadInt32();
            UnknownByte = r.ReadByte();
            Textures = r.ReadL32Array<uint>(sizeof(uint));
        }

        //: New
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"{nameof(RenderTexture)}: {Id:X8}", items: new List<MetadataInfo> {
                })
            };
            return nodes;
        }
    }
}
