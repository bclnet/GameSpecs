using OpenStack;
using OpenStack.Graphics;
using System;

namespace GameSpec.Platforms
{
    public abstract class TextureBuilderBase<Texture>
    {
        public static int MaxTextureMaxAnisotropy
        {
            get => PlatformStats.MaxTextureMaxAnisotropy;
            set => PlatformStats.MaxTextureMaxAnisotropy = value;
        }

        public abstract Texture DefaultTexture { get; }
        public abstract Texture BuildTexture(ITexture info, Range? range = null);
        public abstract Texture BuildSolidTexture(int width, int height, float[] rgba);
        public abstract Texture BuildNormalMap(Texture source, float strength);
        public abstract void DeleteTexture(Texture texture);
    }
}
