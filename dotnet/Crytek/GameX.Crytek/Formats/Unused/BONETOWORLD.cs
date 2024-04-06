//using System.IO;
//using System.Numerics;
//using static OpenStack.Debug;

//namespace GameX.Cry.Formats.Models
//{
//    /// <summary>
//    /// BONETOWORLD contains the world space location/rotation of a bone.
//    /// </summary>
//    public struct BONETOWORLD
//    {
//        public float[,] boneToWorld;//  4x3 structure

//        public void ReadBoneToWorld(BinaryReader r)
//        {
//            boneToWorld = new float[3, 4];
//            for (var i = 0; i < 3; i++) for (var j = 0; j < 4; j++) boneToWorld[i, j] = r.ReadSingle();
//            //Log($"boneToWorld: {boneToWorld[i, j]:F7}");
//            return;
//        }

//        /// <summary>
//        /// Returns the world space rotational matrix in a Math.net 3x3 matrix.
//        /// </summary>
//        /// <returns>Matrix33</returns>
//        public Matrix3x3 GetBoneToWorldRotationMatrix() => new Matrix3x3
//        {
//            M11 = boneToWorld[0, 0],
//            M12 = boneToWorld[0, 1],
//            M13 = boneToWorld[0, 2],
//            M21 = boneToWorld[1, 0],
//            M22 = boneToWorld[1, 1],
//            M23 = boneToWorld[1, 2],
//            M31 = boneToWorld[2, 0],
//            M32 = boneToWorld[2, 1],
//            M33 = boneToWorld[2, 2]
//        };

//        public Vector3 GetBoneToWorldTranslationVector() => new Vector3
//        {
//            X = boneToWorld[0, 3],
//            Y = boneToWorld[1, 3],
//            Z = boneToWorld[2, 3]
//        };

//        #region Log
//#if LOG
//        public void LogBoneToWorld()
//        {
//            Log($"*** Bone to World ***");
//            Log($"{boneToWorld[0, 0]:F6}  {boneToWorld[0, 1]:F6}  {boneToWorld[0, 2]:F6} {boneToWorld[0, 3]:F6}");
//            Log($"{boneToWorld[1, 0]:F6}  {boneToWorld[1, 1]:F6}  {boneToWorld[1, 2]:F6} {boneToWorld[1, 3]:F6}");
//            Log($"{boneToWorld[2, 0]:F6}  {boneToWorld[2, 1]:F6}  {boneToWorld[2, 2]:F6} {boneToWorld[2, 3]:F6}");
//        }
//#endif
//        #endregion
//    }
//}