using System;

namespace OpenStack.Graphics.OpenGL2
{
    public class TextureFormatChain : IEquatable<TextureFormatChain>
    {
        public TextureFormat TextureFormat { get; set; }
        public int AtlasChainIdx { get; set; }

        public TextureFormatChain(TextureFormat textureFormat, int atlasChainIdx)
        {
            TextureFormat = textureFormat;
            AtlasChainIdx = atlasChainIdx;
        }

        public bool Equals(TextureFormatChain textureFormatChain)
            => TextureFormat.Equals(textureFormatChain.TextureFormat) && AtlasChainIdx == textureFormatChain.AtlasChainIdx;

        public override int GetHashCode()
        {
            var hash = 0;
            hash = (hash * 397) ^ TextureFormat.GetHashCode();
            hash = (hash * 397) ^ AtlasChainIdx.GetHashCode();
            return hash;
        }
    }
}
