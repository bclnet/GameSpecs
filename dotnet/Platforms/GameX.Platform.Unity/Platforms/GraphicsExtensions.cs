using UnityEngine.Rendering;

namespace GameX.Platforms
{
    public struct MaterialTerrain { }

    public struct MaterialBlended
    {
        public BlendMode SrcBlendMode;
        public BlendMode DstBlendMode;
    }

    public struct MaterialTested
    {
        public float Cutoff;
    }
}