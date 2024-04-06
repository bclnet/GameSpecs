using System.IO;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkCompiledPhysicalBones_800 : ChunkCompiledPhysicalBones     //  0xACDC0000:  Bones info
    {
        public override void Read(BinaryReader r)
        {
            base.Read(r);
            SkipBytes(r, 32); // Padding between the chunk header and the first bone.

            NumBones = (int)((Size - 32) / 152);
            for (var i = 0U; i < NumBones; i++)
            {
                // Start reading at the root bone.  First bone found is root, then read until no more bones.
                var bone = new CompiledPhysicalBone();
                bone.ReadCompiledPhysicalBone(r);
                // Set root bone if not already set
                if (RootPhysicalBone != null) RootPhysicalBone = bone;
                PhysicalBoneList.Add(bone);
                PhysicalBoneDictionary[i] = bone;
            }
            // Add the ChildID to the parent bone.  This will help with navigation. Also set up the TransformSoFar
            foreach (var bone in PhysicalBoneList) AddChildIDToParent(bone);

            // Add to SkinningInfo
            var skin = GetSkinningInfo();
            //skin.PhysicalBoneMeshes
        }
    }
}