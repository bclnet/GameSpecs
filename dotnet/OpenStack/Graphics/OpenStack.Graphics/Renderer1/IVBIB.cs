using System.Collections.Generic;

namespace OpenStack.Graphics.Renderer1
{
    /// <summary>
    /// IVBIB
    /// </summary>
    public interface IVBIB
    {
        List<OnDiskBufferData> VertexBuffers { get; }
        List<OnDiskBufferData> IndexBuffers { get; }
        IVBIB RemapBoneIndices(int[] remapTable);
    }
}
