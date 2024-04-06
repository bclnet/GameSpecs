using System;

namespace GameX.Formats.Unknown
{
    public interface IUnknownTexture
    {
        [Flags]
        public enum Map
        {
            Diffuse = 1 << 0,
            Bumpmap = 1 << 1,
            Specular = 1 << 2,
            Environment = 1 << 3,
            Decal = 1 << 4,
            SubSurface = 1 << 5,
            Opacity = 1 << 6,
            Detail = 1 << 7,
            Heightmap = 1 << 8,
            BlendDetail = 1 << 9,
            Custom = 1 << 10,
        }

        string Path { get; }
        Map Maps { get; }
    }
}
