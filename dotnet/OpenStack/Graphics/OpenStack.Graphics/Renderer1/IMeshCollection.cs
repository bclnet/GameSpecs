using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenStack.Graphics.Renderer1
{
    //was:Render/RenderableMesh.IMeshCollection
    public interface IMeshCollection
    {
        IEnumerable<RenderableMesh> RenderableMeshes { get; }
    }
}
