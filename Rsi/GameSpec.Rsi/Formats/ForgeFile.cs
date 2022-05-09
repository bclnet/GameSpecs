using GameSpec.Metadata;
using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Rsi.Formats
{
    public class ForgeFile : IDisposable, IGetMetadataInfo
    {
        public static Task<object> Factory(BinaryReader r, FileMetadata m, PakFile s)
        {
            var file = new ForgeFile(r);
            file.Read(r);
            return Task.FromResult((object)file);
        }

        public ForgeFile() { }
        public ForgeFile(BinaryReader r) => Read(r);

        public void Dispose()
        {
            Reader?.Dispose();
            Reader = null;
        }

        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo("DatabasePak", items: new List<MetadataInfo> {
                    new MetadataInfo($"FileSize: [FileSize]"),
                })
            };
            return nodes;
        }

        public BinaryReader Reader { get; private set; }

        public void Read(BinaryReader r)
            => Reader = r;
    }
}
