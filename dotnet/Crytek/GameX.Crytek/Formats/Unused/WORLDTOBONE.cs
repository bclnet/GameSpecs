//using System.Collections.Generic;
//using System.IO;
//using System.Numerics;
//using static OpenStack.Debug;

//namespace GameX.Cry.Formats.Models
//{
//    /// <summary>
//    /// WORLDTOBONE is also the Bind Pose Matrix (BPM)
//    /// </summary>
//    public struct WORLDTOBONE
//    {
//        public float[,] worldToBone;   //  4x3 structure

//        public void GetWorldToBone(BinaryReader r)
//        {
//            worldToBone = new float[3, 4];
//            for (var i = 0; i < 3; i++) for (var j = 0; j < 4; j++) worldToBone[i, j] = r.ReadSingle();
//            //Log($"worldToBone: {worldToBone[i, j]:F7}");
//            return;
//        }

//        public Matrix4x4 GetMatrix44() => new Matrix4x4
//        {
//            M11 = worldToBone[0, 0],
//            M12 = worldToBone[0, 1],
//            M13 = worldToBone[0, 2],
//            M14 = worldToBone[0, 3],
//            M21 = worldToBone[1, 0],
//            M22 = worldToBone[1, 1],
//            M23 = worldToBone[1, 2],
//            M24 = worldToBone[1, 3],
//            M31 = worldToBone[2, 0],
//            M32 = worldToBone[2, 1],
//            M33 = worldToBone[2, 2],
//            M34 = worldToBone[2, 3],
//            M41 = 0,
//            M42 = 0,
//            M43 = 0,
//            M44 = 1
//        };

//        internal Matrix3x3 GetWorldToBoneRotationMatrix() => new Matrix3x3
//        {
//            M11 = worldToBone[0, 0],
//            M12 = worldToBone[0, 1],
//            M13 = worldToBone[0, 2],
//            M21 = worldToBone[1, 0],
//            M22 = worldToBone[1, 1],
//            M23 = worldToBone[1, 2],
//            M31 = worldToBone[2, 0],
//            M32 = worldToBone[2, 1],
//            M33 = worldToBone[2, 2]
//        };

//        internal Vector3 GetWorldToBoneTranslationVector() => new Vector3
//        {
//            X = worldToBone[0, 3],
//            Y = worldToBone[1, 3],
//            Z = worldToBone[2, 3]
//        };

//        #region Log
//#if LOG
//        public void LogWorldToBone()
//        {
//            //Log("     *** World to Bone ***");
//            Log($"     {worldToBone[0, 0]:F7}  {worldToBone[0, 1]:F7}  {worldToBone[0, 2]:F7}  {worldToBone[0, 3]:F7}");
//            Log($"     {worldToBone[1, 0]:F7}  {worldToBone[1, 1]:F7}  {worldToBone[1, 2]:F7}  {worldToBone[1, 3]:F7}");
//            Log($"     {worldToBone[2, 0]:F7}  {worldToBone[2, 1]:F7}  {worldToBone[2, 2]:F7}  {worldToBone[2, 3]:F7}");
//        }
//#endif
//        #endregion
//    }
//}