using OpenStack.Graphics.Renderer2;
using System;

namespace OpenStack.Graphics.OpenGL2
{
    public class SurfaceTexturePalette : IEquatable<SurfaceTexturePalette>
    {
        public uint OrigSurfaceId { get; set; }  // 0x8 surface id
        public uint SurfaceTextureId { get; set; }      // 0x5 surface texture id
        public IPaletteChanges PaletteChanges { get; set; }

        public SurfaceTexturePalette(uint origSurfaceId, uint surfaceTextureId, IPaletteChanges paletteChanges = null)
        {
            OrigSurfaceId = origSurfaceId;
            SurfaceTextureId = surfaceTextureId;
            PaletteChanges = paletteChanges;
        }

        public bool Equals(SurfaceTexturePalette stp)
        {
            if (OrigSurfaceId != stp.OrigSurfaceId) return false;
            if (SurfaceTextureId != stp.SurfaceTextureId) return false;
            if (PaletteChanges == null && stp.PaletteChanges == null) return true;
            if (PaletteChanges == null && stp.PaletteChanges != null || PaletteChanges != null && stp.PaletteChanges == null) return false;
            if (!PaletteChanges.Equals(stp.PaletteChanges)) return false;
            return true;
        }

        public override int GetHashCode()
        {
            var hash = 0;
            hash = (hash * 397) ^ OrigSurfaceId.GetHashCode();
            hash = (hash * 397) ^ SurfaceTextureId.GetHashCode();
            if (PaletteChanges != null) hash = (hash * 397) ^ PaletteChanges.GetHashCode();
            return hash;
        }
    }
}
