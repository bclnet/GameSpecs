using System.Collections.Generic;
using System.Numerics;

namespace OpenStack.Graphics.Renderer1
{
    /// <summary>
    /// IMeshInfo
    /// </summary>
    public interface IMesh
    {
        IDictionary<string, object> Data { get; }

        IVBIB VBIB { get; }

        void GetBounds();
        Vector3 MinBounds { get; }
        Vector3 MaxBounds { get; }
    }
}