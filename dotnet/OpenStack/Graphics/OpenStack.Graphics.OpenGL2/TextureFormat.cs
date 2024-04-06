using System;

namespace OpenStack.Graphics.OpenGL2
{
    public class TextureFormat : IEquatable<TextureFormat>
    {
        public TextureGLFormat SurfaceFormat { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool HasWrappingUVs { get; set; }

        public TextureFormat(TextureGLFormat surfaceFormat, int width, int height, bool hasWrappingUVs)
        {
            SurfaceFormat = surfaceFormat;
            Width = width;
            Height = height;
            HasWrappingUVs = hasWrappingUVs;
        }

        public float GetBytesPerPixel()
            => SurfaceFormat switch
            {
                TextureGLFormat.DXT1 => 0.5f,
                TextureGLFormat.RGBA32323232F => 4.0f,
                _ => 1.0f,
            };

        public bool Equals(TextureFormat textureFormat)
            => SurfaceFormat == textureFormat.SurfaceFormat && Width == textureFormat.Width && Height == textureFormat.Height && HasWrappingUVs == textureFormat.HasWrappingUVs;

        public override int GetHashCode()
        {
            var hash = 0;
            hash = (hash * 397) ^ SurfaceFormat.GetHashCode();
            hash = (hash * 397) ^ Width.GetHashCode();
            hash = (hash * 397) ^ Height.GetHashCode();
            hash = (hash * 397) ^ HasWrappingUVs.GetHashCode();
            return hash;
        }

        public override string ToString() => $"SurfaceFormat: {SurfaceFormat}, Width: {Width}, Height: {Height}, HasWrappingUVs: {HasWrappingUVs}";
    }
}
