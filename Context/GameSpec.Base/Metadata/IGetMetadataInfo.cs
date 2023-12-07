using GameSpec.Formats;
using System.Collections.Generic;

namespace GameSpec.Metadata
{
    public interface IGetMetadataInfo
    {
        List<MetadataInfo> GetInfoNodes(MetadataManager resource = null, FileSource file = null, object tag = null);
    }
}
