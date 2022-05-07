using GameSpec.Valve.Formats.Blocks.Animation;
using OpenStack.Graphics;
using OpenStack.Graphics.Renderer;
using System.Collections.Generic;

namespace GameSpec.Valve.Formats
{
    /// <summary>
    /// IValveModelInfo
    /// </summary>
    public interface IValveModelInfo : IModelInfo
    {
        ModelSkeleton GetSkeleton();
        IEnumerable<(string MeshName, long LoDMask)> GetReferenceMeshNamesAndLoD();
        IEnumerable<(IMeshInfo Mesh, long LoDMask)> GetEmbeddedMeshesAndLoD();
        IEnumerable<string> GetReferencedAnimationGroupNames();
        IEnumerable<ModelAnimation> GetEmbeddedAnimations();
        IEnumerable<string> GetMeshGroups();
        IEnumerable<string> GetDefaultMeshGroups();
        IEnumerable<bool> GetActiveMeshMaskForGroup(string groupName);
    }
}