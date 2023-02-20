using GameSpec.Metadata;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Formats
{
    public class BinarySnd : IGetMetadataInfo
    {
        public static Task<object> Factory(BinaryReader r, FileMetadata f, PakFile s) => Task.FromResult((object)new BinarySnd(r, (int)f.FileSize));

        public BinarySnd() { }
        public BinarySnd(BinaryReader r, int fileSize) => Data = r.ReadBytes(fileSize);

        public byte[] Data;

        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag) => new List<MetadataInfo> {
            new MetadataInfo(null, new MetadataContent { Type = "AudioPlayer", Name = Path.GetFileName(file.Path), Value = new MemoryStream(Data), Tag = Path.GetExtension(file.Path) }),
        };
    }
}
