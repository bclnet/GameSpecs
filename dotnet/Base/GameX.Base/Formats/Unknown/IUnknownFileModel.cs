using System.Collections.Generic;

namespace GameX.Formats.Unknown
{
    public interface IUnknownFileModel : IUnknownFileObject
    {
        IEnumerable<IUnknownModel> Models { get; }
        IEnumerable<UnknownMesh> Meshes { get; }
        IEnumerable<IUnknownMaterial> Materials { get; }
        IEnumerable<IUnknownProxy> Proxies { get; }
        IUnknownSkin SkinningInfo { get; }
        IEnumerable<string> RootNodes { get; }
    }
}
