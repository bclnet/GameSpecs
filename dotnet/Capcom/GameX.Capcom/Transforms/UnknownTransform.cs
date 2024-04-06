using GameX.Formats.Unknown;
using System;
using System.Threading.Tasks;

namespace GameX.Capcom.Transforms
{
    /// <summary>
    /// UnknownTransform
    /// </summary>
    public static class UnknownTransform
    {
        internal static bool CanTransformFileObject(PakFile left, PakFile right, object source) => false;
        internal static Task<IUnknownFileModel> TransformFileObjectAsync(PakFile left, PakFile right, object source) => throw new NotImplementedException();
    }
}