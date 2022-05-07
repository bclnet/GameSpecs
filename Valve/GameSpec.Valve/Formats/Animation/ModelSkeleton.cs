using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GameSpec.Valve.Formats.Blocks.Animation
{
    public class ModelSkeleton
    {
        const int BoneUsedByVertexLod0 = 0x00000400;

        public List<ModelBone> Roots { get; private set; } = new List<ModelBone>();
        public ModelBone[] Bones { get; private set; } = Array.Empty<ModelBone>();
        public int AnimationTextureSize { get; } = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelSkeleton"/> class.
        /// </summary>
        public ModelSkeleton(IDictionary<string, object> modelData)
        {
            // Check if there is any skeleton data present at all
            if (!modelData.ContainsKey("m_modelSkeleton"))
                Console.WriteLine("No skeleton data found.");

            // Get the remap table and invert it for our construction method
            var remapTable = modelData.GetInt64Array("m_remappingTable");
            var start = 0;
            var end = remapTable.Length;
            var remapTableStarts = modelData.GetInt64Array("m_remappingTableStarts");

            // we only use lod 1
            if (remapTableStarts.Length > 1)
            {
                start = (int)remapTableStarts[0];
                end = (int)remapTableStarts[1];
            }

            var invMapTable = remapTable.Skip(start).Take(end - start)
                .Select((mapping, index) => (mapping, index))
                .ToLookup(mi => mi.mapping, mi => mi.index);

            if (invMapTable.Any())
                AnimationTextureSize = invMapTable.Select(g => g.Max()).Max() + 1;

            // Construct the armature from the skeleton KV
            ConstructFromNTRO(modelData.GetSub("m_modelSkeleton"), invMapTable);
        }

        /// <summary>
        /// Construct the Armature object from mesh skeleton KV data.
        /// </summary>
        void ConstructFromNTRO(IDictionary<string, object> skeletonData, ILookup<long, int> remapTable)
        {
            var boneNames = skeletonData.Get<string[]>("m_boneName");
            var boneParents = skeletonData.GetInt64Array("m_nParent");
            var boneFlags = skeletonData.GetInt64Array("m_nFlag");
            var bonePositions = skeletonData.Get<Vector3[]>("m_bonePosParent");
            var boneRotations = skeletonData.Get<Quaternion[]>("m_boneRotParent");

            // Initialise bone array
            Bones = new ModelBone[boneNames.Length];

            //Add all bones to the list
            for (var i = 0; i < boneNames.Length; i++)
            {
                if ((boneFlags[i] & BoneUsedByVertexLod0) != BoneUsedByVertexLod0)
                    continue;
                var name = boneNames[i];
                var position = bonePositions[i];
                var rotation = boneRotations[i];
                // Create bone
                var bone = new ModelBone(name, remapTable[i].ToList(), position, rotation);
                if (boneParents[i] != -1)
                {
                    bone.SetParent(Bones[boneParents[i]]);
                    Bones[boneParents[i]].AddChild(bone);
                }
                Bones[i] = bone;
            }
            FindRoots();
        }

        /// <summary>
        /// Find all skeleton roots (bones without a parent).
        /// </summary>
        void FindRoots()
        {
            Roots = new List<ModelBone>();
            foreach (var bone in Bones)
                if (bone != null && bone.Parent == null)
                    Roots.Add(bone);
        }
    }
}
