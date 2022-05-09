using GameSpec.Formats;
using System.Collections.Generic;

namespace GameSpec.Metadata
{
    public interface IGetMetadataInfo
    {
        List<MetadataInfo> GetInfoNodes(MetadataManager resource = null, FileMetadata file = null, object tag = null);
    }
}
