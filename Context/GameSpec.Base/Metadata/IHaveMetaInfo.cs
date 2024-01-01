using GameSpec.Formats;
using System.Collections.Generic;

namespace GameSpec.Metadata
{
    public interface IHaveMetaInfo
    {
        List<MetaInfo> GetInfoNodes(MetaManager resource = null, FileSource file = null, object tag = null);
    }
}
