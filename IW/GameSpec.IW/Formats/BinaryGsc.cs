using GameSpec.Formats;
using GameSpec.Metadata;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.IW.Formats
{
    // https://github.com/SE2Dev/gsc_parser
    public class BinaryGsc : IGetMetadataInfo
    {
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag) => new List<MetadataInfo> {
            new MetadataInfo(null, new MetadataContent { Type = "GSC", Name = Path.GetFileName(file.Path), Value = this })
        };
    }
}
