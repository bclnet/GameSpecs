using OpenStack.Graphics.Renderer1;
using System.Collections.Generic;

namespace OpenStack.Graphics
{
    /// <summary>
    /// IModel
    /// </summary>
    public interface IModel
    {
        IDictionary<string, object> Data { get; }
        IVBIB RemapBoneIndices(IVBIB vbib, int meshIndex);
    }
}