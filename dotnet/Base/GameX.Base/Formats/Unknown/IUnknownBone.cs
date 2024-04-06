using System.Numerics;

namespace GameX.Formats.Unknown
{
    public interface IUnknownBone
    {
        string Name { get; }
        Matrix4x4 WorldToBone { get; } // 4x3 matrix
        Matrix4x4 BoneToWorld { get; } // 4x3 matrix of world translations/rotations of the bones.
    }
}
