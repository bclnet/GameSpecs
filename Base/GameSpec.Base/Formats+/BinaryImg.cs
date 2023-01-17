using GameSpec.Metadata;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Formats
{
    public class BinaryImg : IGetMetadataInfo
    {
        public static Task<object> Factory(BinaryReader r, FileMetadata f, PakFile s) => Task.FromResult((object)new BinaryImg(r, (int)f.FileSize));

        public BinaryImg() { }
        public BinaryImg(BinaryReader r, int fileSize) => Data = r.ReadBytes(fileSize);

        public byte[] Data;

        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag) => new List<MetadataInfo> {
            new MetadataInfo(null, new MetadataContent { Type = "Image", Name = Path.GetFileName(file.Path), Value = Data }),
        };
    }
}
