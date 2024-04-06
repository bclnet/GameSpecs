using OpenStack.Graphics;
using OpenStack.Graphics.DirectX;
using System;
using System.Numerics;

namespace GameX.Platforms
{
    public static class UnityExtensions
    {
        public static UnityEngine.Experimental.Rendering.GraphicsFormat ToUnity(this DXGI_FORMAT source) => (UnityEngine.Experimental.Rendering.GraphicsFormat)source;
        public static UnityEngine.TextureFormat ToUnity(this TextureUnityFormat source) => (UnityEngine.TextureFormat)source;

        // NifUtils
        public static UnityEngine.Vector3 ToUnity(this Vector3 source) { MathX.Swap(ref source.Y, ref source.Z); return new UnityEngine.Vector3(source.X, source.Y, source.Z); }
        public static UnityEngine.Vector3 ToUnity(this Vector3 source, float meterInUnits) => source.ToUnity() / meterInUnits;
        public static UnityEngine.Matrix4x4 ToUnityRotationMatrix(this Matrix4x4 rotationMatrix) => new UnityEngine.Matrix4x4
        {
            m00 = rotationMatrix.M11,
            m01 = rotationMatrix.M13,
            m02 = rotationMatrix.M12,
            m03 = 0,
            m10 = rotationMatrix.M31,
            m11 = rotationMatrix.M33,
            m12 = rotationMatrix.M32,
            m13 = 0,
            m20 = rotationMatrix.M21,
            m21 = rotationMatrix.M23,
            m22 = rotationMatrix.M22,
            m23 = 0,
            m30 = 0,
            m31 = 0,
            m32 = 0,
            m33 = 1
        };
        public static UnityEngine.Quaternion ToUnityQuaternionAsRotationMatrix(this Matrix4x4 rotationMatrix) => ToQuaternionAsRotationMatrix(rotationMatrix.ToUnityRotationMatrix());
        public static UnityEngine.Quaternion ToQuaternionAsRotationMatrix(this UnityEngine.Matrix4x4 rotationMatrix) => UnityEngine.Quaternion.LookRotation(rotationMatrix.GetColumn(2), rotationMatrix.GetColumn(1));
        public static UnityEngine.Quaternion ToUnityQuaternionAsEulerAngles(this Vector3 eulerAngles)
        {
            var newEulerAngles = eulerAngles.ToUnity();
            var xRot = UnityEngine.Quaternion.AngleAxis(UnityEngine.Mathf.Rad2Deg * newEulerAngles.x, UnityEngine.Vector3.right);
            var yRot = UnityEngine.Quaternion.AngleAxis(UnityEngine.Mathf.Rad2Deg * newEulerAngles.y, UnityEngine.Vector3.up);
            var zRot = UnityEngine.Quaternion.AngleAxis(UnityEngine.Mathf.Rad2Deg * newEulerAngles.z, UnityEngine.Vector3.forward);
            return xRot * zRot * yRot;
        }
    }
}