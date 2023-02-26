using GameSpec.Valve.Formats.Blocks.Animation;
using GameSpec.Valve.Formats.Blocks.Animation.SegmentDecoders;
using System.Collections.Generic;
using System.Linq;

namespace GameSpec.Valve.Formats.Blocks
{
    //was:Resource/ResourceTypes/Model
    public class DATAModel : DATABinaryKV3OrNTRO //, IValveModelInfo
    {
        Skeleton _cachedSkeleton;
        public Skeleton Skeleton => _cachedSkeleton ?? (_cachedSkeleton = Skeleton.FromModelData(Data));
        readonly IDictionary<(VBIB VBIB, int MeshIndex), VBIB> remappedVBIBCache = new Dictionary<(VBIB VBIB, int MeshIndex), VBIB>();

        List<CCompressedAnimQuaternion> CachedAnimations;

        public int[] GetRemapTable(int meshIndex)
        {
            var remapTableStarts = Data.Get<int[]>("m_remappingTableStarts");
            if (remapTableStarts.Length <= meshIndex) return null;

            // Get the remap table and invert it for our construction method
            var remapTable = Data.Get<int[]>("m_remappingTable").Select(i => (int)i);
            var start = (int)remapTableStarts[meshIndex];
            return remapTable.Skip(start).Take(Skeleton.LocalRemapTable.Length).ToArray();
        }

        public VBIB RemapBoneIndices(VBIB vbib, int meshIndex)
        {
            if (Skeleton.Bones.Length == 0) return vbib;
            if (remappedVBIBCache.TryGetValue((vbib, meshIndex), out var res)) return res;
            res = vbib.RemapBoneIndices(VBIB.CombineRemapTables(new int[][] { GetRemapTable(meshIndex), Skeleton.LocalRemapTable }));
            remappedVBIBCache.Add((vbib, meshIndex), res);
            return res;
        }

        public IEnumerable<(int MeshIndex, string MeshName, long LoDMask)> GetReferenceMeshNamesAndLoD()
        {
            var refLODGroupMasks = Data.Get<int[]>("m_refLODGroupMasks");
            var refMeshes = Data.Get<string[]>("m_refMeshes");
            var result = new List<(int MeshIndex, string MeshName, long LoDMask)>();
            for (var meshIndex = 0; meshIndex < refMeshes.Length; meshIndex++)
            {
                var refMesh = refMeshes[meshIndex];
                if (!string.IsNullOrEmpty(refMesh)) result.Add((meshIndex, refMesh, refLODGroupMasks[meshIndex]));
            }
            return result;
        }

        public IEnumerable<(DATAMesh Mesh, int MeshIndex, string Name, long LoDMask)> GetEmbeddedMeshesAndLoD()
            => GetEmbeddedMeshes().Zip(Data.Get<int[]>("m_refLODGroupMasks"), (l, r) => (l.Mesh, l.MeshIndex, l.Name, r));

        public IEnumerable<(DATAMesh Mesh, int MeshIndex, string Name)> GetEmbeddedMeshes()
        {
            var meshes = new List<(DATAMesh Mesh, int MeshIndex, string Name)>();
            if (Parent.ContainsBlockType<CTRL>())
            {
                var ctrl = Parent.GetBlockByType<CTRL>() as DATABinaryKV3;
                var embeddedMeshes = ctrl.Data.GetArray("embedded_meshes");
                if (embeddedMeshes == null) return meshes;

                foreach (var embeddedMesh in embeddedMeshes)
                {
                    var name = embeddedMesh.Get<string>("name");
                    var meshIndex = (int)embeddedMesh.Get<int>("mesh_index");
                    var dataBlockIndex = (int)embeddedMesh.Get<int>("data_block");
                    var vbibBlockIndex = (int)embeddedMesh.Get<int>("vbib_block");

                    var mesh = Parent.GetBlockByIndex<DATAMesh>(dataBlockIndex);
                    mesh.VBIB = Parent.GetBlockByIndex<VBIB>(vbibBlockIndex);

                    var morphBlockIndex = (int)embeddedMesh.Get<int>("morph_block");
                    if (morphBlockIndex >= 0) mesh.MorphData = Parent.GetBlockByIndex<DATAMorph>(morphBlockIndex);

                    meshes.Add((mesh, meshIndex, name));
                }
            }

            return meshes;
        }

        public DATAPhysAggregateData GetEmbeddedPhys()
        {
            if (!Parent.ContainsBlockType<CTRL>()) return null;

            var ctrl = Parent.GetBlockByType<CTRL>() as DATABinaryKV3;
            var embeddedPhys = ctrl.Data.GetSub("embedded_physics");
            if (embeddedPhys == null) return null;

            var physBlockIndex = (int)embeddedPhys.Get<int>("phys_data_block");
            return Parent.GetBlockByIndex<DATAPhysAggregateData>(physBlockIndex);
        }

        public IEnumerable<string> GetReferencedPhysNames()
            => Data.Get<string[]>("m_refPhysicsData");

        public IEnumerable<string> GetReferencedAnimationGroupNames()
            => Data.Get<string[]>("m_refAnimGroups");

        public IEnumerable<CCompressedAnimQuaternion> GetEmbeddedAnimations()
        {
            var embeddedAnimations = new List<CCompressedAnimQuaternion>();
            if (!Parent.ContainsBlockType<CTRL>()) return embeddedAnimations;

            var ctrl = Parent.GetBlockByType<CTRL>() as DATABinaryKV3;
            var embeddedAnimation = ctrl.Data.GetSub("embedded_animation");
            if (embeddedAnimation == null) return embeddedAnimations;

            var groupDataBlockIndex = (int)embeddedAnimation.Get<int>("group_data_block");
            var animDataBlockIndex = (int)embeddedAnimation.Get<int>("anim_data_block");

            var animationGroup = Parent.GetBlockByIndex<DATABinaryKV3OrNTRO>(groupDataBlockIndex);
            var decodeKey = animationGroup.Data.GetSub("m_decodeKey");
            var animationDataBlock = Parent.GetBlockByIndex<DATABinaryKV3OrNTRO>(animDataBlockIndex);
            return CCompressedAnimQuaternion.FromData(animationDataBlock.Data, decodeKey, Skeleton);
        }

        public IEnumerable<CCompressedAnimQuaternion> GetAllAnimations(PakFile fileLoader)
        {
            if (CachedAnimations != null) return CachedAnimations;

            var animGroupPaths = GetReferencedAnimationGroupNames();
            var animations = GetEmbeddedAnimations().ToList();

            // Load animations from referenced animation groups
            foreach (var animGroupPath in animGroupPaths)
            {
                var animGroup = fileLoader.LoadFileObjectAsync<object>(animGroupPath + "_c");
                if (animGroup != default)
                    animations.AddRange(AnimationGroupLoader.LoadAnimationGroup(animGroup, fileLoader, Skeleton));
            }

            CachedAnimations = animations.ToList();
            return CachedAnimations;
        }

        public IEnumerable<string> GetMeshGroups()
            => Data.Get<string[]>("m_meshGroups");

        public IEnumerable<string> GetMaterialGroups()
           => Data.Get<IDictionary<string, object>>("m_materialGroups").Select(group => group.Get<string>("m_name"));

        public IEnumerable<string> GetDefaultMeshGroups()
        {
            var defaultGroupMask = Data.GetUInt64("m_nDefaultMeshGroupMask");
            return GetMeshGroups().Where((group, index) => ((ulong)(1 << index) & defaultGroupMask) != 0);
        }

        public IEnumerable<bool> GetActiveMeshMaskForGroup(string groupName)
        {
            var groupIndex = GetMeshGroups().ToList().IndexOf(groupName);
            var meshGroupMasks = Data.GetInt64Array("m_refMeshGroupMasks");
            return groupIndex >= 0
                ? meshGroupMasks.Select(mask => (mask & 1 << groupIndex) != 0)
                : meshGroupMasks.Select(_ => false);
        }
    }
}
