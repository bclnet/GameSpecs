using UnityEngine.Rendering;

namespace GameSpec.Graphics
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