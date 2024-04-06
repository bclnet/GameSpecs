using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using static OpenStack.Debug;

namespace GameX.Crytek.Formats.Models
{
    /// < summary >
    /// This is the same as BoneDescData
    /// </ summary >
    public class CompiledBone
    {
        public uint ControllerID { get; set; }
        public PhysicsGeometry[] physicsGeometry;   // 2 of these. One for live objects, other for dead (ragdoll?)
        public float mass;                          // 0xD8 ?
        public Matrix3x4 WorldToBone;               // 4x3 matrix
        public Matrix3x4 BoneToWorld;               // 4x3 matrix of world translations/rotations of the bones.
        public string boneName;                     // String256 in old terms; convert to a real null terminated string.
        public int limbID;                         // ID of this limb... usually just 0xFFFFFFFF
        public int offsetParent;                    // offset to the parent in number of CompiledBone structs (584 bytes)
        public int offsetChild;                     // Offset to the first child to this bone in number of CompiledBone structs
        public uint numChildren;                    // Number of children to this bone

        public Matrix4x4 BindPoseMatrix;            // Calculated WorldToBone matrix for library_controllers
        public long offset;                         // Calculated position in the file where this bone started.
        public uint parentID;                       // Calculated controllerID of the parent bone put into the Bone Dictionary (the key)
        public List<uint> childIDs = new List<uint>(); // Calculated controllerIDs of the children to this bone.

        public CompiledBone ParentBone;

        // Because Cryengine tends to store transform relative to world, we have to add all the transforms from the node to the root.  Calculated, row major.
        public Matrix4x4 LocalTransform
            => ParentBone == null
                ? BoneToWorld.ToMatrix4x4()
                : ParentBone.BoneToWorld.ToMatrix4x4() * BoneToWorld.ToMatrix4x4();

        /// <summary>
        /// // Reads just a single 584 byte entry of a bone. At the end the seek position will be advanced, so keep that in mind.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <returns></returns>
        public void ReadCompiledBone_800(BinaryReader r)
        {
            ControllerID = r.ReadUInt32();          // unique id of bone (generated from bone name)
            physicsGeometry = new PhysicsGeometry[2];
            physicsGeometry[0].ReadPhysicsGeometry(r); // lod 0 is the physics of alive body, 
            physicsGeometry[1].ReadPhysicsGeometry(r); // lod 1 is the physics of a dead body
            mass = r.ReadSingle();
            WorldToBone = r.ReadMatrix3x4();
            BindPoseMatrix = WorldToBone.ConvertToTransformMatrix();
            BoneToWorld = r.ReadMatrix3x4();
            boneName = r.ReadFYString(256);
            limbID = r.ReadInt32();
            offsetParent = r.ReadInt32();
            numChildren = r.ReadUInt32();
            offsetChild = r.ReadInt32();
        }

        /// <summary>
        /// Reads just a single 3xx byte entry of a bone.
        /// </summary>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public void ReadCompiledBone_801(BinaryReader r)
        {
            ControllerID = r.ReadUInt32();                 // unique id of bone (generated from bone name)
            limbID = r.ReadInt32();
            r.Skip(208);
            boneName = r.ReadFYString(48);
            offsetParent = r.ReadInt32();
            numChildren = r.ReadUInt32();
            offsetChild = r.ReadInt32();
            // TODO:  This may be quaternion and translation vectors. 
            WorldToBone = r.ReadMatrix3x4();
            BindPoseMatrix = WorldToBone.ConvertToTransformMatrix();
            BoneToWorld = new Matrix3x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0);
        }

        public void ReadCompiledBone_900(BinaryReader r)
        {
            ControllerID = r.ReadUInt32();                 // unique id of bone (generated from bone name)
            limbID = r.ReadInt32();
            offsetParent = r.ReadInt32();
            var relativeQuat = r.ReadQuaternion();
            var relativeTranslation = new Vector3(z: r.ReadSingle(), y: r.ReadSingle(), x: -r.ReadSingle());
            var worldQuat = r.ReadQuaternion();
            var worldTranslation = new Vector3(z: r.ReadSingle(), y: r.ReadSingle(), x: -r.ReadSingle());
            BindPoseMatrix = Matrix4x4.CreateFromQuaternion(relativeQuat);
            BindPoseMatrix.M14 = relativeTranslation.X;
            BindPoseMatrix.M24 = relativeTranslation.Y;
            BindPoseMatrix.M34 = relativeTranslation.Z;
            BindPoseMatrix.M41 = 0;
            BindPoseMatrix.M42 = 0;
            BindPoseMatrix.M43 = 0;
            BindPoseMatrix.M44 = 1.0f;
            BoneToWorld = Matrix3x4.CreateFromParts(worldQuat, worldTranslation);
        }

        #region Log
#if LOG
        public void LogCompiledBone()
        {
            // Output the bone to the console
            Log($"*** Compiled bone {boneName}");
            Log($"    Parent Name: {parentID}");
            Log($"    Offset in file: {offset:X}");
            Log($"    Controller ID: {ControllerID}");
            Log($"    World To Bone: {BoneToWorld}");
            Log($"    Limb ID: {limbID}");
            Log($"    Parent Offset: {offsetParent}");
            Log($"    Child Offset:  {offsetChild}");
            Log($"    Number of Children:  {numChildren}");
            Log($"*** End Bone {boneName}");
        }
#endif
        #endregion
    }
}