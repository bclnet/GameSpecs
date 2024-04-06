using GameX.Crytek.Formats.Models;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static OpenStack.Debug;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public abstract class ChunkCompiledBones : Chunk //  0xACDC0000:  Bones info
    {
        public string RootBoneName;         // Controller ID?  Name?  Not sure yet.
        public CompiledBone RootBone;       // First bone in the data structure.  Usually Bip01
        public int NumBones;                // Number of bones in the chunk

        // Bones are a bit different than Node Chunks, since there is only one CompiledBones Chunk, and it contains all the bones in the model.
        public List<CompiledBone> BoneList = new List<CompiledBone>();

        public List<CompiledBone> GetAllChildBones(CompiledBone bone) => bone.numChildren > 0 ? BoneList.Where(a => bone.childIDs.Contains(a.ControllerID)).ToList() : null;

        public List<string> GetBoneNames() => BoneList.Select(a => a.boneName).ToList();

        protected void AddChildIDToParent(CompiledBone bone)
        {
            if (bone.parentID != 0) BoneList.FirstOrDefault(a => a.ControllerID == bone.parentID)?.childIDs.Add(bone.ControllerID); // Should only be one parent.
        }

        public override string ToString()
            => $@"Chunk Type: {ChunkType}, ID: {ID:X}";

        #region Log
#if LOG
        public override void LogChunk()
        {
            Log($"*** START CompiledBone Chunk ***");
            Log($"    ChunkType:           {ChunkType}");
            Log($"    Node ID:             {ID:X}");
        }

        /// <summary>
        /// Writes the results of common matrix math.  For testing purposes.
        /// </summary>
        /// <param name="localRotation">The matrix that the math functions will be applied to.</param>
        void LogMatrices(Matrix3x3 localRotation)
        {
            localRotation.LogMatrix3x3("Regular");
            localRotation.Inverse().LogMatrix3x3("Inverse");
            localRotation.Conjugate().LogMatrix3x3("Conjugate");
            localRotation.ConjugateTranspose().LogMatrix3x3("Conjugate Transpose");
        }
#endif
        #endregion
    }
}
//public Dictionary<int, CompiledBone> BoneDictionary = new Dictionary<int, CompiledBone>(); // Dictionary of all the CompiledBone objects based on parent offset(?).
//public CompiledBone GetParentBone(CompiledBone bone, int boneIndex) => bone.offsetParent != 0 ? BoneDictionary[boneIndex + bone.offsetParent] : null; // Should only be one parent.

////bone.ParentBone = BoneMap[i + bone.offsetParent];
//bone.ParentBone = GetParentBone(bone, i);
//bone.parentID = bone.ParentBone != null ? bone.ParentBone.ControllerID : 0;
//if (bone.parentID != 0)
//{
//    localRotation = GetParentBone(bone, i).boneToWorld.GetBoneToWorldRotationMatrix().ConjugateTransposeThisAndMultiply(bone.boneToWorld.GetBoneToWorldRotationMatrix());
//    localTranslation = GetParentBone(bone, i).LocalRotation * (bone.LocalTranslation - GetParentBone(bone, i).boneToWorld.GetBoneToWorldTranslationVector());
//}
//else
//{
//    localTranslation = bone.boneToWorld.GetBoneToWorldTranslationVector();
//    localRotation = bone.boneToWorld.GetBoneToWorldRotationMatrix();
//}
//bone.LocalTransform = GetTransformFromParts(localTranslation, localRotation);