using GameSpec.Formats.Unknown;
using System.Threading.Tasks;

namespace GameSpec.Rsi.Transforms
{
    /// <summary>
    /// UnknownTransform
    /// </summary>
    public static class UnknownTransform
    {
        internal static bool CanTransformFileObject(PakFile left, PakFile right, object source) => Cry.Transforms.UnknownTransform.CanTransformFileObject(left, right, source);
        internal static Task<IUnknownFileModel> TransformFileObjectAsync(PakFile left, PakFile right, object source) => Cry.Transforms.UnknownTransform.TransformFileObjectAsync(left, right, source);
    }
}