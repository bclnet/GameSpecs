using System.Collections.Generic;
using System.Numerics;

namespace GameX.Formats.Unknown
{
    public interface IUnknownMaterial
    {
        string Name { get; }
        Vector3? Diffuse { get; } // Color:RGB
        Vector3? Specular { get; } // Color:RGB
        Vector3? Emissive { get; } // Color:RGB
        float Shininess { get; }
        float Opacity { get; }
        IEnumerable<IUnknownTexture> Textures { get; }
    }
}
