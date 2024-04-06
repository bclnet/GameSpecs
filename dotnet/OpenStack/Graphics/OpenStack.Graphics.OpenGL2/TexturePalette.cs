using OpenStack.Graphics.Renderer2;
using System;

namespace OpenStack.Graphics.OpenGL2
{
    public class TexturePalette : IEquatable<TexturePalette>
    {
        public uint TextureId { get; set; } // 0x6 texture file id
        public IPaletteChanges PaletteChanges { get; set; }

        public TexturePalette(uint textureId, IPaletteChanges paletteChanges = null)
        {
            TextureId = textureId;
            PaletteChanges = paletteChanges;
        }

        public bool Equals(TexturePalette tp)
        {
            if (TextureId != tp.TextureId) return false;
            if (PaletteChanges == null && tp.PaletteChanges == null) return true;
            if (PaletteChanges == null && tp.PaletteChanges != null || PaletteChanges != null && tp.PaletteChanges == null) return false;
            if (!PaletteChanges.Equals(tp.PaletteChanges)) return false;
            return true;
        }

        public override int GetHashCode()
        {
            var hash = 0;
            hash = (hash * 397) ^ TextureId.GetHashCode();
            if (PaletteChanges != null) hash = (hash * 397) ^ PaletteChanges.GetHashCode();
            return hash;
        }
    }
}
