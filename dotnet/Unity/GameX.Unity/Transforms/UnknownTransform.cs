using System;
using GameX.Formats.Unknown;
using System.Threading.Tasks;

namespace GameX.Unity.Transforms
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