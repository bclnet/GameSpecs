using System.Runtime.InteropServices;
using GlIndex = System.Int32;

namespace System.NumericsX
{
    // this is used for calculating unsmoothed normals and tangents for deformed models
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DominantTri
    {
        public GlIndex v2, v3;
        public fixed float normalizationScale[3];
    }
}
