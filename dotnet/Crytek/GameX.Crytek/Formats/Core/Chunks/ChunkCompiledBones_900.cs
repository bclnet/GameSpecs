using GameX.Crytek.Formats.Models;
using System.Collections.Generic;
using System.IO;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkCompiledBones_900 : ChunkCompiledBones
    {
        public override void Read(BinaryReader r)
        {
            base.Read(r);

            NumBones = r.ReadInt32();
            for (var i = 0; i < NumBones; i++)
            {
                var bone = new CompiledBone();
                bone.ReadCompiledBone_900(r);
                // First bone read is root bone
                if (RootBone == null) RootBone = bone;
                BoneList.Add(bone);
            }

            // Post bone read setup. Parents, children, etc.
            // Add the ChildID to the parent bone. This will help with navigation.
            var boneNames = r.ReadCStringArray(NumBones);
            for (var i = 0; i < NumBones; i++)
            {
                BoneList[i].boneName = boneNames[i];
                SetParentBone(BoneList[i]);
                AddChildIDToParent(BoneList[i]);
            }

            // Add to SkinningInfo
            var skin = GetSkinningInfo();
            skin.CompiledBones = new List<CompiledBone>();
            skin.HasSkinningInfo = true;
            skin.CompiledBones = BoneList;
        }

        void SetParentBone(CompiledBone bone)
        {
            // offsetParent is really parent index.
            if (bone.offsetParent != -1)
            {
                bone.parentID = BoneList[bone.offsetParent].ControllerID;
                bone.ParentBone = BoneList[bone.offsetParent];
            }
        }
    }
}
