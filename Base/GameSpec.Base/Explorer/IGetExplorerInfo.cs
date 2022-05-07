using GameSpec.Formats;
using System.Collections.Generic;

namespace GameSpec.Explorer
{
    public interface IGetExplorerInfo
    {
        List<ExplorerInfoNode> GetInfoNodes(ExplorerManager resource = null, FileMetadata file = null, object tag = null);
    }
}
