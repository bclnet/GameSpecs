using GameX.Formats.Unknown;
using System.Threading.Tasks;

namespace GameX.Cig.Transforms
{
    /// <summary>
    /// UnknownTransform
    /// </summary>
    public static class UnknownTransform
    {
        internal static bool CanTransformFileObject(PakFile left, PakFile right, object source) => Crytek.Transforms.UnknownTransform.CanTransformFileObject(left, right, source);
        internal static Task<IUnknownFileModel> TransformFileObjectAsync(PakFile left, PakFile right, object source) => Crytek.Transforms.UnknownTransform.TransformFileObjectAsync(left, right, source);
    }
}