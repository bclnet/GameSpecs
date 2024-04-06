using GameX.Valve.Formats.Animations;
using GameX.Valve.Formats.Blocks;
using OpenStack.Graphics;
using System.Collections.Generic;

namespace GameX.Valve.Formats
{
    /// <summary>
    /// IValveModel
    /// </summary>
    public interface IValveModel : IModel
    {
        Skeleton Skeleton { get; }

        IEnumerable<(int MeshIndex, string MeshName, long LoDMask)> GetReferenceMeshNamesAndLoD();
        IEnumerable<(DATAMesh Mesh, int MeshIndex, string Name, long LoDMask)> GetEmbeddedMeshesAndLoD();
        IEnumerable<bool> GetActiveMeshMaskForGroup(string groupName);
        IEnumerable<string> GetMeshGroups();
        IEnumerable<string> GetDefaultMeshGroups();
        IEnumerable<Animation> GetAllAnimations(IOpenGraphic graphic);
        DATAPhysAggregateData GetEmbeddedPhys();
        IEnumerable<string> GetReferencedPhysNames();
    }
}