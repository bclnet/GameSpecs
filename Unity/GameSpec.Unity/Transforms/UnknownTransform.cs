using System;
using GameSpec.Formats.Unknown;
using System.Threading.Tasks;

namespace GameSpec.Unity.Transforms
{
    /// <summary>
    /// UnknownTransform
    /// </summary>
    public static class UnknownTransform
    {
        internal static bool CanTransformFileObject(PakFile left, PakFile right, object source) => throw new NotImplementedException();
        internal static Task<IUnknownFileModel> TransformFileObjectAsync(PakFile left, PakFile right, object source) => throw new NotImplementedException();
    }
}