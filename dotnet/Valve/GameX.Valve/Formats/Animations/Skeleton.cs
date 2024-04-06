using OpenStack.Graphics.Renderer1.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GameX.Valve.Formats.Animations
{
    //was:Resource/ResourceTypes/ModelAnimation/Skeleton
    public class Skeleton : ISkeleton
    {
        [Flags]
        public enum ModelSkeletonBoneFlags //was:Resource/Enums/ModelSkeletonBoneFlags
        {
            NoBoneFlags = 0x0,
            BoneFlexDriver = 0x4,
            Cloth = 0x8,
            Physics = 0x10,
            Attachment = 0x20,
            Animation = 0x40,
            Mesh = 0x80,
            Hitbox = 0x100,
            RetargetSrc = 0x200,
            BoneUsedByVertexLod0 = 0x400,
            BoneUsedByVertexLod1 = 0x800,
            BoneUsedByVertexLod2 = 0x1000,
            BoneUsedByVertexLod3 = 0x2000,
            BoneUsedByVertexLod4 = 0x4000,
            BoneUsedByVertexLod5 = 0x8000,
            BoneUsedByVertexLod6 = 0x10000,
            BoneUsedByVertexLod7 = 0x20000,
            BoneMergeRead = 0x40000,
            BoneMergeWrite = 0x80000,
            BlendPrealigned = 0x100000,
            RigidLength = 0x200000,
            Procedural = 0x400000,
        }

        public Bone[] Roots { get; private set; }
        public Bone[] Bones { get; private set; }
        public int[] LocalRemapTable { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Skeleton"/> class.
        /// </summary>
        public static Skeleton FromModelData(IDictionary<string, object> modelData)
        {
            // Check if there is any skeleton data present at all
            if (!modelData.ContainsKey("m_modelSkeleton")) Console.WriteLine("No skeleton data found.");
            // Construct the armature from the skeleton KV
            return new Skeleton(modelData.GetSub("m_modelSkeleton"));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Skeleton"/> class.
        /// </summary>
        public Skeleton(IDictionary<string, object> skeletonData)
        {
            var boneNames = skeletonData.Get<string[]>("m_boneName");
            var boneParents = skeletonData.GetInt64Array("m_nParent");
            var boneFlags = skeletonData.GetInt64Array("m_nFlag").Select(flags => (ModelSkeletonBoneFlags)flags).ToArray();
            var bonePositions = skeletonData.Get<Vector3[]>("m_bonePosParent");
            var boneRotations = skeletonData.Get<Quaternion[]>("m_boneRotParent");

            LocalRemapTable = new int[boneNames.Length];
            var currentRemappedBone = 0;
            for (var i = 0; i < LocalRemapTable.Length; i++)
                LocalRemapTable[i] = (boneFlags[i] & ModelSkeletonBoneFlags.BoneUsedByVertexLod0) != 0
                    ? currentRemappedBone++
                    : -1;

            // Initialise bone array
            Bones = Enumerable.Range(0, boneNames.Length)
                .Where(i => (boneFlags[i] & ModelSkeletonBoneFlags.BoneUsedByVertexLod0) != 0)
                .Select((boneID, i) => new Bone(i, boneNames[boneID], bonePositions[boneID], boneRotations[boneID]))
                .ToArray();

            for (var i = 0; i < LocalRemapTable.Length; i++)
            {
                var remappeBoneID = LocalRemapTable[i];
                if (remappeBoneID != -1 && boneParents[i] != -1)
                {
                    var remappedParent = LocalRemapTable[boneParents[i]];
                    Bones[remappeBoneID].SetParent(Bones[remappedParent]);
                }
            }

            // Create an empty root list
            Roots = Bones.Where(bone => bone.Parent == null).ToArray();
        }
    }
}
